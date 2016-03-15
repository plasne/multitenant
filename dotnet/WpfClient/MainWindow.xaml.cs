using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Authorization auth = null;

        private const string authority = "https://login.windows.net/common";
        //private const string resourceId = "https://graph.windows.net/";
        //private const string resourceId = "36bda7c5-cc23-4618-9e09-e710b2357818";
        private const string resourceId = "http://testauth.plasne.com/";
        private const string clientId = "e54b04c6-6e2b-45e8-98eb-78b5616276d2";
        private const string redirectUri = "http://testauthwpf";

        private class Authorization
        {
            public string accessToken;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        async private void login_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                message.Content = "logging in... please wait...";

                // get the access token
                AuthenticationContext authContext = new AuthenticationContext(authority, new FileCache());
                AuthenticationResult result = authContext.AcquireToken(resourceId, clientId, new Uri(redirectUri), PromptBehavior.Auto);

                // give it to the server to get a JWT
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                HttpResponseMessage response = await httpClient.GetAsync("http://testauth.plasne.com/login/token");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    auth = Newtonsoft.Json.JsonConvert.DeserializeObject<Authorization>(json);
                    message.Content = "login successful.";
                }
                else
                {
                    message.Content = "login failed.";
                    MessageBox.Show("An error occurred : " + response.ReasonPhrase);
                }

            }
            catch (AdalException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private class WhoAmI
        {
            public string id;
            public string role;
            public string rights;
        }

        async private void whoAmI_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.accessToken);
                HttpResponseMessage response = await httpClient.GetAsync("http://testauth.plasne.com/whoami");
                if (response.IsSuccessStatusCode)
                {
                    string message = await response.Content.ReadAsStringAsync();
                    WhoAmI whoAmI = Newtonsoft.Json.JsonConvert.DeserializeObject<WhoAmI>(message);
                    this.message.Content = "You are " + whoAmI.id + ".";
                }
                else
                {
                    MessageBox.Show("An error occurred : " + response.ReasonPhrase);
                }

            }
            catch (AdalException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}
