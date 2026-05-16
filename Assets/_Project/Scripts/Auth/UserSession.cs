namespace ElTesoroDeMongli.Auth
{
    public class UserSession
    {
        public int UserId { get; }
        public string Token { get; }
        public string Login { get; }

        public bool IsAuthenticated => UserId > 0 && !string.IsNullOrWhiteSpace(Token);

        public UserSession(int userId, string token, string login)
        {
            UserId = userId;
            Token = token;
            Login = login;
        }
    }
}
