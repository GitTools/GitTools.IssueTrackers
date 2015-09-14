namespace GitTools.IssueTrackers
{
    using System;
    using JetBrains.Annotations;

    public class AuthSettings
    {
        public AuthSettings()
        {
            IsEmpty = true;
        }

        public AuthSettings([NotNull] string username, [NotNull] string password)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException("password");
            Username = username;
            Password = password;
            IsUsernameAndPasswordAuthentication = true;
        }

        public AuthSettings([NotNull] string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException("token");
            Token = token;
            IsTokenAuthentication = true;
        }

        public bool IsEmpty { get; private set; }

        public string Token { get; private set; }
        public bool IsTokenAuthentication { get; private set; }

        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool IsUsernameAndPasswordAuthentication { get; private set; }
    }
}