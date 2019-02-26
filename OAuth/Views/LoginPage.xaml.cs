using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OAuth.Models;
using OAuth.ViewModels;
using Xamarin.Auth;
using Xamarin.Forms;
using System.Net.Http;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using OAuth.Services;

namespace OAuth.Views
{
    public partial class LoginPage : ContentPage
    {
    
        OidcClient client;
        LoginResult result;
        
        Account account;
        AccountStore store;
        
        Lazy<HttpClient> _apiClient = new Lazy<HttpClient>(()=>new HttpClient()); 
    
        public LoginPage()
        {
            InitializeComponent();

            BindingContext = new LoginViewModel();
            store = AccountStore.Create();
        }
        
        async void Login_Clicked(object sender, EventArgs e)
        {
            var browser = DependencyService.Get<IBrowser>();

            var options = new OidcClientOptions
            {
                Authority = "https://demo.identityserver.io",
                ClientId = "native.hybrid",
                Scope = "openid profile email api offline_access",
                RedirectUri = "xamarinformsclients://callback",
                Browser = browser,

                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };
            
            client = new OidcClient(options);
            
            result = await client.LoginAsync(new LoginRequest());

            if (result.IsError)
            {
                await DisplayAlert("Access Token", "Login error", "OK");
                return;
            }
            
            await DisplayAlert("Access Token", result.AccessToken, "OK");

     
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

        void Facebook_Clicked(object sender, System.EventArgs e)
        {
            var auth = new OAuth2Authenticator(Constants.FbClientId, "",
                                            new Uri("https://m.facebook.com/dialog/oauth/"),
                                            new Uri("https://www.facebook.com/connect/login_success.html"),
                                            null, false);
                                            
            auth.Completed += OnFbAuthCompleted;
            auth.Error += OnAuthError;

            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            presenter.Login(auth);
        }

        async void OnFbAuthCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= OnFbAuthCompleted;
                authenticator.Error -= OnAuthError;
            }

            User user = null;
            if (e.IsAuthenticated)
            {
                // If the user is authenticated, request their basic user data from Google
                // FacebookEmailUrl = "https://graph.facebook.com/me?fields=email&access_token={accessToken}"

                var token = e.Account.Properties["access_token"];
                var param = new Dictionary<string, string>();
                param.Add("access_token", token);
                
                var request = new OAuth2Request("GET", new Uri("https://graph.facebook.com/me?fields=email"), param, e.Account);
                var response = await request.GetResponseAsync();
                if (response != null)
                {
                    // Deserialize the data and store it in the account store
                    // The users email address will be used to identify data in SimpleDB
                    string userJson = await response.GetResponseTextAsync();
                    var email = JsonConvert.DeserializeObject<FacebookEmail>(userJson);
                    
                    await DisplayAlert("OnFbAuthCompleted", email.Email, "OK");
                }

                
                
            }
        }
    }
}
