namespace Models
{
    public class User
    {

        private static User _instance;

        public static User Instance
        {
            get
            {
                if (_instance == null)
                {
                    new User();
                }
                return _instance;
            }
        }

        private User()
        {
            _instance = this;
        }
        
        public int id;
        public string username;
    }
}