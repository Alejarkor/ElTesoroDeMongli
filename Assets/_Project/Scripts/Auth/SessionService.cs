namespace ElTesoroDeMongli.Auth
{
    public class SessionService
    {
        public UserSession CurrentSession { get; private set; }
        public bool IsAuthenticated => CurrentSession != null && CurrentSession.IsAuthenticated;

        public void SetSession(UserSession session)
        {
            CurrentSession = session;
        }

        public void ClearSession()
        {
            CurrentSession = null;
        }
    }
}
