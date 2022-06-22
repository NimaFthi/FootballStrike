using System.Collections.Generic;

namespace Models
{
    public class GameLog
    {
        public int id;
        public string type;
        public Dictionary<string, object> parameters;

        public GameLog(int id,string type,Dictionary<string, object> parameters)
        {
            this.id = id;
            this.type = type;
            this.parameters = parameters;
        }
    }
}