using UnityEngine;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class UnityServer : MonoBehaviour
{
    private HttpListener listener;
    private string url = "http://localhost:8080/";

    void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add(url);
        try
        {
            listener.Start();
            Debug.Log("Listening for requests at " + url);
            Task.Run(() => HandleIncomingConnections());
        }
        catch (HttpListenerException e)
        {
            Debug.LogError("HttpListenerException: " + e.Message);
        }
    }

    private async Task HandleIncomingConnections()
    {
        while (listener.IsListening)
        {
            HttpListenerContext ctx = await listener.GetContextAsync();
            _ = ProcessRequest(ctx);
        }
    }

    private async Task ProcessRequest(HttpListenerContext ctx)
    {
        HttpListenerRequest req = ctx.Request;
        HttpListenerResponse resp = ctx.Response;

        if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/unity/player-location")
        {
            string jsonResponse = GetPlayerLocation();
            await SendResponse(resp, jsonResponse);
        }
        else if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/unity/player-look-direction")
        {
            string jsonResponse = GetPlayerLookDirection();
            await SendResponse(resp, jsonResponse);
        }

        resp.Close();
    }

    private string GetPlayerLocation()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Student");

        if (playerObject != null)
        {
            Vector3 playerPosition = playerObject.transform.position;
            var playerPositionJson = new
            {
                x = playerPosition.x,
                y = playerPosition.y,
                z = playerPosition.z
            };

            return JsonUtility.ToJson(playerPositionJson);
        }
        else
        {
            return "{\"error\": \"Player object not found\"}";
        }
    }

    private string GetPlayerLookDirection()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Student");

        if (playerObject != null)
        {
            Vector3 lookDirection = playerObject.transform.forward;
            var lookDirectionJson = new
            {
                x = lookDirection.x,
                y = lookDirection.y,
                z = lookDirection.z
            };

            return JsonUtility.ToJson(lookDirectionJson);
        }
        else
        {
            return "{\"error\": \"Player object not found\"}";
        }
    }

    private async Task SendResponse(HttpListenerResponse resp, string response)
    {
        byte[] data = Encoding.UTF8.GetBytes(response);
        resp.ContentType = "application/json";
        resp.ContentEncoding = Encoding.UTF8;
        resp.ContentLength64 = data.LongLength;
        await resp.OutputStream.WriteAsync(data, 0, data.Length);
    }

    void OnApplicationQuit()
    {
        listener.Stop();
        listener.Close();
    }
}
