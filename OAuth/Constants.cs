using System;
namespace OAuth
{
    public static class Constants
    {
        public static string AppName = "OAuth2App";

        // For Google login, configure at https://console.developers.google.com/
        public static string iOSClientId = "954853910072-6osnii3vjd8jqi2v7m6vl94iovghmre3.apps.googleusercontent.com";
        public static string AndroidClientId = "<insert Android client ID here>";

        // These values do not need changing
        public static string Scope = "https://www.googleapis.com/auth/userinfo.email";
        public static string AuthorizeUrl = "https://accounts.google.com/o/oauth2/auth";
        public static string AccessTokenUrl = "https://www.googleapis.com/oauth2/v4/token";
        public static string UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

        // Set these to reversed iOS/Android client ids, with :/oauth2redirect appended
        public static string iOSRedirectUrl = "com.googleusercontent.apps.954853910072-6osnii3vjd8jqi2v7m6vl94iovghmre3:/oauth2redirect";
        public static string AndroidRedirectUrl = "<insert Android redirect URL here>:/oauth2redirect";
    }
}
