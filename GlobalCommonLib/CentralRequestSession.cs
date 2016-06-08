namespace GlobalCommonLib
{
    public class CentralRequestSession
    {
        public CentralRequestSession(string username, string password)
        {
            UserName = username;
            Password = password;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
    }
}