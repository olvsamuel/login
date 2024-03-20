
namespace login.Database
{
    public class Tokens
    {
        private static readonly Dictionary<int, TokenData> TokenDictionary = new Dictionary<int, TokenData>();
        private static int NextId = 1;

        public static void AddToken(string token, string refreshToken, int idSession, string email)
        {
            TokenDictionary.Add(NextId++, new TokenData(token, refreshToken, idSession, email, DateTime.Now));
        }

        public static List<TokenData> GetTokens(string? email = null, int? idSession = 0, string? token = null, string? refreshToken = null)
        {
            IQueryable<TokenData> queryable = TokenDictionary.Values.AsQueryable();

            if (email != null)
                queryable = queryable.Where(x => x.Email == email);

            if (idSession > 0)
                queryable = queryable.Where(x => x.IdSession == idSession);

            if (token != null)
                queryable = queryable.Where(x => x.Token == token);

            if (refreshToken != null)
                queryable = queryable.Where(x => x.RefreshToken == refreshToken);

            return queryable.ToList();
        }

        public static void NewSession(string email, string token, string refreshToken, string? oldToken = null)
        {
            bool updated = UpdateSession(email, token, refreshToken, oldToken);
            if (updated == true) return;

            (bool sessionLimit, int idSession) = IsUserSessionLimitReached(email);
            if (sessionLimit == true)
                RemoveSession(email, idSession);

            AddToken(token, refreshToken, idSession, email);
        }
        
        private static (bool, int) IsUserSessionLimitReached(string email)
        {
            int UserSessions = TokenDictionary.Values.Where(x => x.Email == email).Count();
            bool Sessions = UserSessions >= 3;

            if (Sessions == true)
            {
                int lastSession = TokenDictionary.Values.Where(x => x.Email == email).OrderBy(x => x.CreatedAt).First().IdSession;
                return (Sessions, lastSession);
            }

            List<int> ExistingSession = TokenDictionary.Values.Where(x => x.Email == email).Select(x => x.IdSession).ToList();
            int NextId = Enumerable.Range(1, 3).Except(ExistingSession).First();

            return (Sessions, NextId);
        }

        private static bool UpdateSession(string email, string token, string refreshToken, string? oldToken = null)
        {
            IQueryable<TokenData> queryable = TokenDictionary.Values.AsQueryable();
            queryable = queryable.Where(x => x.Email == email && refreshToken == x.RefreshToken);

            if (oldToken != null)
                queryable = queryable.Where(x => x.Token != oldToken);

            TokenData? sessionToUpdate = queryable.FirstOrDefault();

            if (sessionToUpdate != null)
            {
                sessionToUpdate.Token = token;

                return true;
            }

            return false;
        }

        private static void RemoveSession(string email, int idSession)
        {
            TokenData sessionToRemove = TokenDictionary.Values.Where(x => x.Email == email && x.IdSession == idSession).First();
            if (sessionToRemove != null)
            {
                TokenDictionary.Remove(sessionToRemove.IdSession);
            }
        }

    }

    public class TokenData
    {
        public TokenData(string token, string refreshToken, int idSession, string email, DateTime createdAt)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email, name and refresh token are required");
            }
            Token = token;
            RefreshToken = refreshToken;
            IdSession = idSession;
            Email = email;
            CreatedAt = createdAt;
        }

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int IdSession { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
