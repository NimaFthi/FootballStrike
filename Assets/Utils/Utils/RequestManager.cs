using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using _Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class RequestManager : SingletonMonoBehaviour<RequestManager>
{
    string authenticate(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }

    private static RequestManager _instance;
    public string hashSalt = "M@tc4W@st3r";

    public static string ComputeSha256Hash(string rawData)
    {
        // Create a SHA256   
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }


    public void Cancel()
    {

    }

    public void Send(RequestMethod method, string uri, Action<long, string> response, Dictionary<string,string> customHeaders = null,
        string userName = "", string sendData = "")
    {
        string authorization = authenticate(userName, ComputeSha256Hash(userName + sendData + hashSalt));
        switch (method)
        {
            case RequestMethod.GET:
                StartCoroutine(GetRequest(uri, response, authorization,customHeaders));
                break;
            case RequestMethod.POST:
                StartCoroutine(PostRequest(uri, sendData, authorization, response));
                break;
            case RequestMethod.PUT:
                StartCoroutine(PutRequest(uri, sendData, authorization, response));
                break;
        }
    }


    IEnumerator GetRequest(string uri, Action<long, string> response,
        string authorization, Dictionary<string,string> customHeaders = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(uri))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    www.SetRequestHeader(header.Key, header.Value);
                }
            }
            
            if (!string.IsNullOrEmpty(authorization))
            {
                www.SetRequestHeader("AUTHORIZATION", authorization);
            }
            yield return www.SendWebRequest();
            if (www.uri.ToString() == uri)
            {
                response(www.responseCode, www.downloadHandler.text);
            }
            else
            {
                response(500, "");
            }
        }
    }

    IEnumerator PostRequest(string uri, string postData, string authorization,
        Action<long, string> response)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(postData);
        using (UnityWebRequest www = UnityWebRequest.Post(uri, UnityWebRequest.kHttpVerbPOST))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            if (!string.IsNullOrEmpty(postData))
            {
                UploadHandlerRaw raw = new UploadHandlerRaw(bytes);
                www.uploadHandler = raw;
                www.SetRequestHeader("Content-Type", "application/json");
            }
            if (!string.IsNullOrEmpty(authorization))
            {
                www.SetRequestHeader("AUTHORIZATION", authorization);
            }
            yield return www.SendWebRequest();

            if (www.uri.ToString() == uri)
            {
                response(www.responseCode, www.downloadHandler.text);
            }
            else
            {
                response(500, "");
            }
        }
    }

    IEnumerator PutRequest(string uri, string postData, string authorization,
        Action<long, string> response)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(postData);
        using (UnityWebRequest www = UnityWebRequest.Put(uri, bytes))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            if (!string.IsNullOrEmpty(authorization))
            {
                www.SetRequestHeader("AUTHORIZATION", authorization);
            }
            yield return www.SendWebRequest();

            print(www.uri.ToString());
            if (www.uri.ToString() == uri)
            {
                response(www.responseCode, www.downloadHandler.text);
            }
            else
            {
                response(500, "");
            }
        }
    }

}

public enum RequestMethod
{
    POST,
    GET,
    PUT
}

