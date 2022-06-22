#define WS_DEBUG
// #define PING_DEBUG

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

// using System.Net.WebSockets;
public class WSConnectionManager : MonoBehaviour
{
    public string apiToken = "??";
    [System.NonSerialized] private string _userToken;
    public string url;
    public string origin;
    public bool autoReconnect;
    private Thread thread;
    private Dispatcher dispatcher = new Dispatcher();
    private WebSocket ws;
    private bool isConnected;

    private int requestId = 0;

    //events:
    public event System.Action onOpen = delegate {  };
    public event System.Action<ushort, string> onClose = delegate(ushort arg1, string s) {  };
    private System.Action connect_onOpen;
    private System.Action<ushort, string> connect_onClose;
    public event System.Action<WSResponse> onMessage;
    private List<WSRequestWithCallback> requestsWithCallback = new List<WSRequestWithCallback>();
    

    // Update is called once per frame
    void Update()
    {
        if (dispatcher == null)
            return;
        lock (dispatcher)
        {
            dispatcher.Dispatch();
        }
    }

    private void OnApplicationQuit()
    {
        Close();
    }

    private void WSThread()
    {
        Debug.Log("WS Thread Started =>" + this.url);
        ws = new WebSocket(this.url+"?token="+_userToken);
        ws.Origin = this.origin;
        // Debug.Log("origin=>" + ws.Origin);
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WS Connected");
            lock (dispatcher)
            {
                dispatcher.Add(() =>
                {
                    isConnected = true;
                    MyLog("onOpen");
                    if (connect_onOpen != null)
                        connect_onOpen();
                    if (onOpen != null)
                        onOpen.Invoke();
                });
            }

            // ws.Send("handshake");
        };
        ws.OnError += (sender, e) =>
        {
            lock (dispatcher)
            {
                dispatcher.Add(() =>
                {
                    MyLog("Error! " + e.Message);
                    if (e.Message.Contains("while connecting") || e.Message.Contains("opening"))
                    {
                        if (connect_onClose != null)
                            connect_onClose(500, e.Message);
                        if (onClose != null)
                            onClose(500, e.Message);
                        Close();
                    }
                });
            }
        };
        ws.OnClose += (sender, e) =>
        {
            isConnected = false;
            lock (dispatcher)
            {
                dispatcher.Add(() =>
                {
                    Debug.Log("WS DC "+gameObject.name);
                    MyLog("onClose");
                    if (connect_onClose != null)
                        connect_onClose(e.Code, e.Reason);
                    if (onClose != null)
                        onClose(e.Code, e.Reason);
                    Close();
                });
            }
        };
        ws.OnMessage += (sender, e) =>
        {
            var str = e.Data;
            Debug.Log(str);
            if (str == "handshake-answer")
            {
                isConnected = true;
                lock (dispatcher)
                {
                    dispatcher.Add(() =>
                    {
                        StopAllCoroutines();
                        StartCoroutine(PingCoroutine());
                    });
                }
                return;
            }

            if (str == "pong")
            {
#if WS_DEBUG && PING_DEBUG
                MyLog("pong");
#endif
                return;
            }

            if (!str.StartsWith("{"))
            {
                Debug.LogWarning("Got unkown answer from ws=" + str);
                return;
            }
            if (str.Contains("\n"))
            {
                var parts = str.Split('\n');
                for (var i = 0; i < parts.Length; i++)
                {
                    if(string.IsNullOrEmpty(parts[i]))
                        continue;
                    MyLog("onMessage["+i+"] => " + parts[i]);
                    HandleWSResponse(WSResponse.FromJSON<WSResponse>(parts[i]));
                }

                return;
            }
            MyLog("onMessage=>" + str);
            var response = WSResponse.FromJSON<WSResponse>(str);
            HandleWSResponse(response);
        };
        Debug.Log("WS Connecting...");
        ws.Connect();
    }

    private void HandleWSResponse(WSResponse response)
    {
        lock (dispatcher)
        {
            dispatcher.Add(() =>
            {
                //check between requests:
                for (var i = 0; i < requestsWithCallback.Count; i++)
                {
                    if (requestsWithCallback[i].request.model == response.model
                        && requestsWithCallback[i].request.method == response.method
                        && response.requestId == requestsWithCallback[i].request.requestId)
                    {
                        requestsWithCallback[i].callback(response);
                        requestsWithCallback.RemoveAt(i);
                        return;
                    }
                }

                // Debug.LogWarning("request not found goto onMessage:");
                //otherwise simple on-message:
                if (onMessage != null)
                    onMessage(response);
            });
        }
    }
    public void Connect(string userToken)
    {
        this._userToken = userToken;
    }
    private IEnumerator PingCoroutine()
    {
        while (isConnected)
        {
            yield return new WaitForSeconds(0.2f);
            ws.Send("ping");
#if WS_DEBUG && PING_DEBUG
            Debug.Log("ping");
#endif
        }
    }

    private void MyLog(object o)
    {
#if WS_DEBUG
        Debug.Log("WS =>" + o);
#endif
    }

    #region public interface

    public void Connect(string userToken,System.Action onOpen, System.Action<ushort, string> onClose)
    {
        this._userToken = userToken;
        this.connect_onOpen = onOpen;
        this.connect_onClose = onClose;
        // dispatcher = new Dispatcher();
        requestsWithCallback = new List<WSRequestWithCallback>();
        thread = new Thread(new ThreadStart(WSThread));
        thread.Start();
    }

    private void Close()
    {
        StopAllCoroutines();
        if (ws != null)
        {
            try
            {
                ws.Close();
                ws = null;
            }
            catch (System.Exception e)
            {
                Debug.LogError("ws != null error=>" + e.Message);
            }
        }

        if (thread != null)
        {
            try
            {
                thread.Abort();
                thread = null;
            }
            catch (System.Exception e)
            {
                Debug.LogError("thread != null error=>" + e.Message);
            }
        }

        if (autoReconnect)
            StartCoroutine(ReConnect());
    }

    private IEnumerator ReConnect()
    {
        yield return new WaitForSeconds(2);
        Connect(this._userToken,this.connect_onOpen, this.connect_onClose);
    }

    public bool IsConnected()
    {
        return isConnected;
    }
    public void ListenOnOpen(System.Action onOpen, bool getInitialNotif = false)
    {
        this.onOpen += onOpen;
        if (getInitialNotif && IsConnected())
            onOpen();
    }

    public void ListenOnClose(System.Action<ushort, string> onClose)
    {
        this.onClose += onClose;
    }

    public void RemoveListenOnOpen(System.Action callback)
    {
        this.onOpen -= callback;
    }

    public void RemoveListenOnClose(System.Action<ushort, string> callback)
    {
        this.onClose -= callback;
    }

    public void Send(WSRequest request)
    {
        if (!IsConnected())
            return;
        request.requestId = requestId++;
        ws.Send(request.ToJSON());
        MyLog("Send=>" + request.ToJSON());
    }

    public void Request(WSRequest request, System.Action<WSResponse> callback)
    {
        if (!IsConnected())
            return;
        request.requestId = requestId++;
        requestsWithCallback.Add(new WSRequestWithCallback(request, callback));
        ws.Send(request.ToJSON());
        MyLog("Send=>" + request.ToJSON());
    }

    #endregion
}