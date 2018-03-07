using System;

namespace Rock.Security.Authentication.Auth0
{
    internal class Auth0ManagementUserInfo
    {
        public string email { get; set; }
        public bool email_verified { get; set; }
        public User_Metadata user_metadata { get; set; }
        public DateTime updated_at { get; set; }
        public string name { get; set; }
        public string picture { get; set; }
        public string user_id { get; set; }
        public string nickname { get; set; }
        public Identity[] identities { get; set; }
        public DateTime created_at { get; set; }
        public App_Metadata app_metadata { get; set; }
        public string last_ip { get; set; }
        public DateTime last_login { get; set; }
        public int logins_count { get; set; }
    }

    internal class User_Metadata
    {
        public string given_name { get; set; }
        public string family_name { get; set; }
    }

    internal class App_Metadata
    {
        public string country { get; set; }
        public object[] phones { get; set; }
        public string role { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
    }

    internal class Identity
    {
        public string user_id { get; set; }
        public string provider { get; set; }
        public string connection { get; set; }
        public bool isSocial { get; set; }
    }
}
