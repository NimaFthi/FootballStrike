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
    private int _logNumber;
    public int LogNumber => _logNumber;

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
                print($"Status Code: {response.code} Body: {response._data} MatchID: {_matchId}");
                if (response._data.ToString() != "{}")
                {
                    var matches = JsonConvert.DeserializeObject<Dictionary<string,Match>>(response._data.ToString());
                    _matchId = GetEmptyMatchID(matches);
                }
                else
                {
                    _matchId = 1;
                }

            }, headers);
    }


    private void MessageHandler(WSResponse response)
    {
        Match match;
        switch (response.method)
        {
            case SocketEvents.JoinMatch:
                match = response.GetData<Dictionary<string, Match>>()["match"];
                if (match.userIds.Count > 1)
                {
                    _onJoinMatch?.Invoke();
                }

                MatchManager.Instance.Match = match;
                break;
            case SocketEvents.UpdateMatch:
                match = response.GetData<Dictionary<string, Match>>()["match"];
                if (match.logs.Count > 0)
                {
                    onGameAction?.Invoke(match.logs[_logNumber]);
                    _logNumber++;
                }
                else
                {
                    if (match.userIds.Count > 1)
                    {
                        _onJoinMatch?.Invoke();
                    }
                }
                break;
        }
    }

    private int GetEmptyMatchID(Dictionary<string,Match> matches)
    {
        if (matches == null)
        {
            return 1;
        }
        var lastMatchKey = matches.Keys.ToArray()[matches.Keys.Count - 1];
        
        if (matches[lastMatchKey].userIds.Count < 2)
        {
            return matches[lastMatchKey].id;
        }
        return matches[lastMatchKey].id + 1;
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