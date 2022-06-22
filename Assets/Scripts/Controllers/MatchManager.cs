using _Scripts.Utils;
using Models;
using UnityEngine;

namespace Shady.Controllers
{
    public class MatchManager : SingletonMonoBehaviour<MatchManager>
    {
        private bool _isPlayerOne;
        private int _turnNumber;
        public int TurnNumber => _turnNumber;

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
                }
            }
        }

        public bool ChangeTurn
        {
            get
            {
                _turnNumber++;
                if (_turnNumber % 2 == 0 && _isPlayerOne)
                {
                    return true;
                }
                if (_turnNumber % 2 != 0 && !_isPlayerOne)
                {
                    return true;
                }

                return false;
            }
        }

    }
}