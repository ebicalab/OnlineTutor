using System;
using System.Collections;
using RestServer;
using RestServer.NetCoreServer;
using UnityEngine;
using UnityEngine.Networking;


// Keycloak-Discovery URL: http://localhost:8080/realms/test/.well-known/openid-configuration
namespace de.bearo.restserver.Samples.OAuthClientExample {
    
    // This example needs a running Keycloak server with a realm "test" and a client "test" with a valid redirect uri. You can import the realm from
    // the file "realm-export.json" in this folder. An example keycloak docker-compose file is included in this directory as well.
    public class OAuthAuthCodeClient : MonoBehaviour {
        public RestServer.RestServer restServer;
        
        [Header("OAuth Settings")]
        public string serverAuthUrl //= "https://www.patreon.com/oauth2/authorize";
            = "http://localhost:8080/realms/test/protocol/openid-connect/auth";
        
        public string serverTokenUrl //= "https://www.patreon.com/api/oauth2/token";
            = "http://localhost:8080/realms/test/protocol/openid-connect/token";
        
        public string clientId;
        
        public string clientSecret;
        
        public string scope;
        
        [Header("Dynamically computed")]
        public string redirectUri;
        
        public string state;
        
        public string receivedAuthCode;
        private bool _resolveOAuthCode = false;
        private bool _allowCallback = false;
        
        public AccessTokenDTO AccessTokenDto;
        
        void Start() {
            redirectUri = "http://localhost:" + restServer.port + "/oauth_callback";
            
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/oauth_callback", request => {
                if (_allowCallback == false) {
                    request.CreateResponse()
                        .Body("OAuth not started.", MimeType.TEXT_HTML)
                        .SendAsync();
                    return;
                }
                
                var uri = request.RequestUri;
                // Example: http://localhost/oauth_callback?code=J2Mwg9ts55JYGVz5BQtsjR5Kdp0T7T&state=13bcfb4f19d1044669433116b74d8ffb
                
                _allowCallback = false;
                
                var returnedAuthCode = request.QueryParameters["code"];
                var returnedState = request.QueryParameters["state"];
                if (returnedState != state) {
                    ThreadingHelper.Instance.ExecuteAsync(() => {
                        Debug.Log("Invalid state returned from OAuth Server - not storing access token");
                        _resolveOAuthCode = false;
                    }, "OAuthAuthCodeClient.OAuthCallback");
                }
                else {
                    ThreadingHelper.Instance.ExecuteAsync(() => {
                        Debug.Log($"Returned auth code: {returnedAuthCode}");
                        receivedAuthCode = returnedAuthCode;
                        _resolveOAuthCode = true;
                    }, "OAuthAuthCodeClient.OAuthCallback");
                }
                
                request.CreateResponse()
                    .Header(HttpHeader.CONTENT_TYPE, MimeType.TEXT_HTML)
                    .Body($"OK.")
                    .SendAsync();
            });
        }
        
        void Update() {
            if (!_resolveOAuthCode) {
                return;
            }
            
            StartCoroutine(DoResolveAuthCode());
            _resolveOAuthCode = false;
        }
        
        public IEnumerator DoResolveAuthCode() {
            var form = new WWWForm();
            form.AddField("client_id", clientId);
            form.AddField("client_secret", clientSecret);
            form.AddField("code", receivedAuthCode);
            form.AddField("grant_type", "authorization_code");
            form.AddField("redirect_uri", redirectUri);
            
            var uwr = UnityWebRequest.Post(serverTokenUrl, form);
            yield return uwr.SendWebRequest();
            
            if (uwr.result != UnityWebRequest.Result.Success) {
                Debug.LogError(uwr.error);
                Debug.LogError(uwr.downloadHandler.error);
                Debug.LogError(uwr.downloadHandler.text);
            }
            else {
                Debug.Log("Received: " + uwr.downloadHandler.text);
                
                var tokenDto = JsonUtility.FromJson<AccessTokenDTO>(uwr.downloadHandler.text);
                Debug.Log($"Resolved access token <{tokenDto.access_token}>, expires in {tokenDto.expires_in} seconds.");
            }
        }
        
        public void DoLink() {
            state = Guid.NewGuid().ToString();
            
            StopAllCoroutines();
            _allowCallback = true;
            _resolveOAuthCode = false;
            
            var url = $"{serverAuthUrl}?" +
                      $"client_id={HttpUtility.UrlEncode(clientId)}&" +
                      $"redirect_uri={HttpUtility.UrlEncode(redirectUri)}&" +
                      $"scope={HttpUtility.UrlEncode(scope)}&" +
                      $"state={HttpUtility.UrlEncode(state)}&" +
                      $"response_type=code";
            
            
            Debug.Log("Starting system browser with url: " + url);
            Application.OpenURL(url);
        }
        
        public class AccessTokenDTO {
            public string access_token;
            public string refresh_token;
            
            public string expires_in;
            public string scope;
        }
    }
}