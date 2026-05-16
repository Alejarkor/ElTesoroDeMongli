using System;

namespace ElTesoroDeMongli.API.Models
{
    [Serializable]
    public class LoginRequest
    {
        public string mail;
        public string password;

        public LoginRequest(string mail, string password)
        {
            this.mail = mail;
            this.password = password;
        }
    }

    [Serializable]
    public class RegisterRequest
    {
        public string mail;
        public string password;
        public string nickname;

        public RegisterRequest(string mail, string password, string nickname)
        {
            this.mail = mail;
            this.password = password;
            this.nickname = nickname;
        }
    }

    [Serializable]
    public class LoginResponse : ApiResponse
    {
        public LoginContent content;
    }

    [Serializable]
    public class LoginContent
    {
        public int user_id;
        public string token;
    }

    [Serializable]
    public class RegisterResponse : ApiResponse
    {
    }
}
