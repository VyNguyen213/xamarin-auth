using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OAuth.Models;
using OAuth.ViewModels;
using Xamarin.Auth;
using Xamarin.Forms;

namespace OAuth.Views
{
    public partial class LoginPage : ContentPage
    {
        Account account;
        AccountStore store;
    
        public LoginPage()
        {
            InitializeComponent();

            BindingContext = new LoginViewModel();
            store = AccountStore.Create();
        }

        void GoogleAuth_Clicked(object sender, EventArgs e)
        {
            GoogleLogin();
        }

        private void GoogleLogin()
        {
            account = store.FindAccountsForService(Constants.AppName).FirstOrDefault();
        
            var authenticator = new OAuth2Authenticator(
                                        Constants.iOSClientId,
                                        null,
                                        Constants.Scope,
                                        new Uri(Constants.AuthorizeUrl),
                                        new Uri(Constants.iOSRedirectUrl),
                                        new Uri(Constants.AccessTokenUrl),
                                        null,
                                        true);
                                        
            authenticator.Completed += OnAuthCompleted;
            authenticator.Error += OnAuthError;

            AuthenticationState.Authenticator = authenticator;

            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            presenter.Login(authenticator);
        }
        
        void OnAuthError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= OnAuthCompleted;
                authenticator.Error -= OnAuthError;
            }

            //Debug.WriteLine("Authentication error: " + e.Message);
        }
        
        async void OnAuthCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= OnAuthCompleted;
                authenticator.Error -= OnAuthError;
            }

            User user = null;
            if (e.IsAuthenticated)
            {
                // If the user is authenticated, request their basic user data from Google
                // UserInfoUrl = https://www.googleapis.com/oauth2/v2/userinfo
                var request = new OAuth2Request("GET", new Uri(Constants.UserInfoUrl), null, e.Account);
                var response = await request.GetResponseAsync();
                if (response != null)
                {
                    // Deserialize the data and store it in the account store
                    // The users email address will be used to identify data in SimpleDB
                    string userJson = await response.GetResponseTextAsync();
                    user = JsonConvert.DeserializeObject<User>(userJson);
                }

                if (account != null)
                {
                    store.Delete(account, Constants.AppName);
                }

                await store.SaveAsync(account = e.Account, Constants.AppName);
                await DisplayAlert("Email address", user.Email, "OK");
            }
        }
    }
}
