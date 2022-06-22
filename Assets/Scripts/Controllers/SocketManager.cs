using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Utils;
using Models;
using Newtonsoft.Json;
using Shady.Controllers;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(WSConnectionManager))]
public class SocketManager : SingletonMonoBehaviour<SocketManager>
{
    private WSConnectionManager _connectionManager;
    private int _matchId;
    [SerializeField] private UnityEvent _onJoinMatch;
    public Action<GameLog> onGameAction;
    private void Start()
    {
        _connectionManager = GetComponent<WSConnectionManager>();
        _connectionManager.onMessage += MessageHandler;
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            ["api-token"] = "omid",
            ["system-token"] = "behzad"
        };
        RequestManager.Instance.Send(RequestMethod.GET, "http://194.59.170.180:3101/api/matches/view",
            (statusCode, responseBody) =>
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(responseBody);

                if (response._data.ToString() != "{}")
                {
                    var matches = JsonConvert.DeserializeObject<List<Match>>(response._data.ToString());
                    _matchId = GetEmptyMatchID(matches);
                }
                else
                {
                    _matchId = 1;
                }

                print($"Status Code: {response.code} Body: {response._data} MatchID: {_matchId}");
            }, headers);
    }

    private void MessageHandler(WSResponse response)
    {
        switch (response.method)
        {
            case SocketEvents.JoinMatch:
                _onJoinMatch?.Invoke();
                var match = response.GetData<Match>();
                MatchManager.Instance.Match = match;
                break;
            case SocketEvents.SendAction:
                onGameAction?.Invoke(response.GetData<GameLog>());
                break;
        }
    }

    private int GetEmptyMatchID(List<Match> matches)
    {
        if (matches == null)
        {
            return 1;
        }

        matches.OrderByDescending(t => t.id);
        if (matches[0].userIds.Count < 2)
        {
            return matches[0].id;
        }

        return matches[0].id + 1;
    }

    public void ConnectToServer(string userToken)
    {
        User.Instance.id = int.Parse(userToken);
        print("CONNECT");
        _connectionManager.Connect(userToken, () =>
        {
            print("Open Connection");
            JoinMatch();
        }, (code, body) => { print($"Error Code: {code} Body: {body}"); });
    }

    private void JoinMatch()
    {
        var param = new Dictionary<string, object>
        {
            ["matchId"] = _matchId
        };
        WSRequest request = new WSRequest("game", SocketEvents.JoinMatch, param);
        _connectionManager.Send(request);
    }

    public void SendAction(GameLog log)
    {
        var param = new Dictionary<string, object>
        {
            ["matchId"] = _matchId,
            ["log"] = log
        };
        WSRequest request = new WSRequest("game", SocketEvents.SendAction, param);
        _connectionManager.Send(request);
    }
}