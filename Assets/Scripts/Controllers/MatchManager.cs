using System;
using _Scripts.Utils;
using Models;
using UnityEngine;
using UnityEngine.UI;

namespace Shady.Controllers
{
    public class MatchManager : SingletonMonoBehaviour<MatchManager>
    {
        [SerializeField] private Text _turnLabel;
        private bool _isPlayerOne;
        private int _turnNumber;
        public int TurnNumber => _turnNumber;
        public Action<bool> onMatchReceived;

        private Match _match;

        public Match Match
        {
            get
            {
                return _match;
            }
            set
            {
                _match = value;
                if (_match.userIds[0] == User.Instance.id)
                {
                    _isPlayerOne = true;
                    _turnLabel.text = "Your turn";
                }
                else
                {
                    _turnLabel.text = "Opponent turn";
                }
                onMatchReceived?.Invoke(_isPlayerOne);
            }
        }

        public bool ChangeTurn
        {
            get
            {
                _turnNumber++;
                if (_turnNumber % 2 == 0 && _isPlayerOne)
                {
                    _turnLabel.text = "Your turn";
                    return true;
                }
                if (_turnNumber % 2 != 0 && !_isPlayerOne)
                {
                    _turnLabel.text = "Your turn";
                    return true;
                }
                _turnLabel.text = "Opponent turn";
                return false;
            }
        }

    }
}