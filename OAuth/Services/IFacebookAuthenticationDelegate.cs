using System;
using OAuth.Models;

namespace OAuth.Services
{
    public interface IFacebookAuthenticationDelegate
    {
        void OnAuthenticationCompletedAsync(FacebookOAuthToken token);
        void OnAuthenticationFailed(string message, Exception exception);
        void OnAuthenticationCanceled();
    }
}
