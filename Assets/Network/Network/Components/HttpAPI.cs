using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class HttpAPI : MonoBehaviour
{
    public string apiBaseURL = "http://localhost:8080/api/";
    public string apiToken = "????";
    public string systemToken = "";
    public string userToken;

    private bool hasInternet = false;
    private Dispatcher dispatcher = new Dispatcher();

    private void Update()
    {
        dispatcher.Dispatch();
    }

    private string GetAPIUrl(string path)
    {
        if (path.Contains("http"))
            return path;
        if (path.StartsWith("/"))
            path = path.Substring(1);
        return apiBaseURL + path;
    }

    public void Post(string path, Dictionary<string, object> body, System.Action<APIResponse> callback)
    {
        StartCoroutine(_Request(true, GetAPIUrl(path), JsonConvert.SerializeObject(body), callback));
    }

    public void Post(string path, string body, System.Action<APIResponse> callback)
    {
        StartCoroutine(_Request(true, GetAPIUrl(path), (body), callback));
    }

    public void Get(string path, System.Action<APIResponse> callback)
    {
        StartCoroutine(_Request(false, GetAPIUrl(path), null, callback));
    }

    private IEnumerator _Request(bool isPost, string url, string body, System.Action<APIResponse> callback)
    {
        Debug.Log((isPost ? "POST" : "GET") + " " + url + (body ?? ""));
        UnityWebRequest request = null;
        if (isPost)
        {
            request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
            request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        }
        else
            request = isPost ? UnityWebRequest.Post(url, body) : UnityWebRequest.Get(url);

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("api-token", apiToken);
        if (!string.IsNullOrEmpty(systemToken))
            request.SetRequestHeader("system-token", systemToken);

        if (!string.IsNullOrEmpty(userToken))
            request.SetRequestHeader("user-token", userToken);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(url + "=>" + request.responseCode + "=>" + request.downloadHandler.text);
            var error = request.error;
            dispatcher.Add(() => { callback(APIResponse.Error(error, APIResponse.NETWORK_ERROR)); });
            request.Dispose();
        }
        else
        {
            try
            {
                Debug.Log(url + "=>" + request.responseCode + "=>" + request.downloadHandler.text);
                var text = request.downloadHandler.text;
                dispatcher.Add(() => { callback(APIResponse.FromJSON<APIResponse>(text)); });
                request.Dispose();
            }
            catch (System.Exception err)
            {
                Debug.LogError(err);
                int code = (int) request.responseCode;
                dispatcher.Add(() => { callback(APIResponse.Error(err.Message, code)); });
                request.Dispose();
            }
        }
    }

    public bool HasInternetAccess()
    {
        return hasInternet;
    }

    public void HardCheckInternetAccess(System.Action<bool> cb)
    {
        System.Action<bool> callback = (b) =>
        {
            Debug.Log("HardCheckInternetAccess()=>" + b);
            cb(b);
        };
        hasInternet = false;

        ExternalGET("http://google.com", (code, result) =>
        {
            if (code == 200)
            {
                callback(true);
                hasInternet = true;
            }
            else
            {
                ExternalGET("http://download.ir", (c2, r2) =>
                {
                    hasInternet = c2 == 200;
                    callback(c2 == 200);
                });
            }
        });
    }

    public void ExternalGET(string url, System.Action<long, string> cb)
    {
        StartCoroutine(_ExternalRequest(url, cb));
    }

    private IEnumerator _ExternalRequest(string url, System.Action<long, string> cb)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (!url.Contains("google.com"))
            Debug.Log(url + "=>" + request.downloadHandler.text);
        var statusCode = request.responseCode;
        var text = request.downloadHandler.text;
        cb(statusCode, text);
    }

    public IEnumerator DownloadImage(string imageUrl, System.Action<Texture2D, string> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            var error = request.error;
            dispatcher.Add(() => { callback(null, error); });
            request.Dispose();
            yield break;
        }

        var texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
        dispatcher.Add(() => { callback(texture, null); });
        request.Dispose();
    }
}