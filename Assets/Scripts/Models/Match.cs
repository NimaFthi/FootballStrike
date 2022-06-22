using System.Collections.Generic;

namespace Models
{
    public class Match
    {
        public int id;
        public List<int> userIds;
        public List<GameLog> logs;
    }
}
