using System;
using System.Collections;
using RestServer;
using RestServer.NetCoreServer;
using UnityEngine;
using UnityEngine.Networking;

namespace de.bearo.restserver.Samples.ItchIOOAuthExample {
    /// <summary>
    /// Provides functionality for linking and authenticating with itch.io using "OAuth".
    /// Note that:
    /// - itch.io has a very weird OAuth implementation that is not really OAuth.
    /// - itch.io documentation hasn't been updated, for example: https://github.com/itchio/itch.io/issues/1498
    /// - this is an example implementation, you can copy or extend this class to your needs.
    /// - this implementation doesn't stop the rest server after the successful login, you should do that or deactivate the callback endpoints
    /// </summary>
    public class ItchIOOauthLinker : MonoBehaviour {
        public RestServer.RestServer restServer;
        
        [Header("Itch.io OAuth Settings")]
        public string clientId;
        
        public string scope = "profile profile:games";
        
        public int accountRetries = 10;
        public float accountRetryDelay = 0.25f;
        
        [Header("Dynamically computed")]
        public string redirectUri;
        
        public string state;
        
        public string callbackHtml;
        
        [Header("Result - store this in a secure place")]
        public string accessToken;
        
        [Header("API result after linking")]
        [TextArea]
        public string accountCredentialsInfoJson;
        
        [TextArea]
        public string accountMeJson;
        
        [TextArea]
        public string accountMyGamesJson;
        
        private bool _resolveAccountInfos = false;
        private bool _allowCallback = false;
        
        public void Start() {
            redirectUri = "http://localhost:" + restServer.port + "/oauth_callback";
            callbackHtml = CreateCallbackPage(restServer.port);
            
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/oauth_callback", request => {
                request.CreateResponse()
                    .Header(HttpHeader.CONTENT_TYPE, MimeType.TEXT_HTML)
                    .Body(callbackHtml)
                    .SendAsync();
            });
            
            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/oauth_callback_finish", request => {
                if (_allowCallback == false) {
                    Debug.Log("Callback not allowed - ignoring.");
                    request.CreateResponse()
                        .Body("OAuth not started.", MimeType.TEXT_HTML)
                        .SendAsync();
                    return;
                }
                
                var returnedAccessToken = request.QueryParameters["access_token"];
                var returnedState = request.QueryParameters["state"];
                
                _allowCallback = false;
                if (returnedState != state) {
                    ThreadingHelper.Instance.ExecuteAsync(() => {
                        Debug.Log("Invalid state returned from Itch.io - not storing access token");
                        _resolveAccountInfos = false;
                    }, "ItchIOOauthLinker.OAuthCallback");
                }
                else {
                    ThreadingHelper.Instance.ExecuteAsync(() => {
                        accessToken = returnedAccessToken;
                        _resolveAccountInfos = true;
                    }, "ItchIOOauthLinker.OAuthCallback");
                }
                
                request.CreateResponse().SendAsync();
            });
        }
        
        public void Update() {
            if (!_resolveAccountInfos) {
                return;
            }
            
            Debug.Log($"Access token received {accessToken} from Itch.io.");
            StartCoroutine(RequestCredentialsInfoCoroutine());
            StartCoroutine(RequestMeCoroutine());
            StartCoroutine(RequestMyGamesCoroutine());
            _resolveAccountInfos = false;
        }
        
        public IEnumerator RequestCredentialsInfoCoroutine() {
            var tries = accountRetries;
            
            while (tries > 0) {
                var uwr = UnityWebRequest.Get($"https://itch.io/api/1/{accessToken}/credentials/info");
                yield return uwr.SendWebRequest();
                
                if (uwr.result == UnityWebRequest.Result.Success) {
                    accountCredentialsInfoJson = uwr.downloadHandler.text;
                    Debug.Log($"Resolved credentials info: {accountCredentialsInfoJson}");
                    yield break;
                }
                
                yield return new WaitForSeconds(accountRetryDelay);
                tries--;
            }
        }
        
        public IEnumerator RequestMeCoroutine() {
            var tries = accountRetries;
            
            while (tries > 0) {
                var uwr = UnityWebRequest.Get($"https://itch.io/api/1/{accessToken}/me");
                yield return uwr.SendWebRequest();
                
                if (uwr.result == UnityWebRequest.Result.Success) {
                    accountMeJson = uwr.downloadHandler.text;
                    Debug.Log($"Resolved me account infos: {accountMeJson}");
                    yield break;
                }
                
                yield return new WaitForSeconds(accountRetryDelay);
                tries--;
            }
        }
        
        public IEnumerator RequestMyGamesCoroutine() {
            var tries = accountRetries;
            
            while (tries > 0) {
                var uwr = UnityWebRequest.Get($"https://itch.io/api/1/{accessToken}/my-games");
                yield return uwr.SendWebRequest();
                
                if (uwr.result == UnityWebRequest.Result.Success) {
                    accountMyGamesJson = uwr.downloadHandler.text;
                    Debug.Log($"Resolved my games: {accountMyGamesJson}");
                    yield break;
                }
                
                yield return new WaitForSeconds(accountRetryDelay);
                tries--;
            }
        }
        
        public void DoLink() {
            state = Guid.NewGuid().ToString();
            
            StopAllCoroutines();
            _resolveAccountInfos = false;
            accountCredentialsInfoJson = "";
            accountMeJson = "";
            accountMyGamesJson = "";
            _allowCallback = true;
            
            var url = $"https://itch.io/user/oauth?" +
                      $"client_id={HttpUtility.UrlEncode(clientId)}&" +
                      $"scope={HttpUtility.UrlEncode(scope)}&" +
                      $"redirect_uri={HttpUtility.UrlEncode(redirectUri)}&" +
                      $"state={HttpUtility.UrlEncode(state)}&" +
                      $"response_type=token";
            
            
            Debug.Log("Starting system browser with url: " + url);
            Application.OpenURL(url);
        }
        
        protected string CreateCallbackPage(int port) {
            return $@"<!DOCTYPE html> 
<html>
<head>
    <title>Itch.io OAuth Callback</title>
</head>
<body onload=""script();"">
    <h1>Itch.io OAuth Callback</h1>
    <p>Close this window and return to the game.</p>
    <script type=""text/javascript"">
        function script() {{
            // first, remove the '#' from the hash part of the URL
            var queryString = window.location.hash.slice(1);
            var params = new URLSearchParams(queryString);
            var accessToken = params.get(""access_token"");
            var state = params.get(""state"");

            // then, send the access token to the game via XHR request

            var xhr = new XMLHttpRequest();
            xhr.open(""POST"", ""http://localhost:{port}/oauth_callback_finish?access_token="" + accessToken + ""&state="" + state, true);
            xhr.send();
        }}
    </script>
</body>";
        }
    }
}