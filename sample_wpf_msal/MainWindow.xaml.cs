using Microsoft.Identity.Client;
using Microsoft.Identity.Client.NativeInterop;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sample_wpf_msal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static string ClientId = "My App Client Id";

        private static string Tenant = "organizations";
        private static IPublicClientApplication _clientApp;

        string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me/ownedDevices";

        string[] scopes = new string[] { "user.read", "DeviceManagementManagedDevices.ReadWrite.All" };

        private async void OnInit(object sender, RoutedEventArgs e)
        {
            _clientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, Tenant)
                .WithDefaultRedirectUri()
                .Build();

            var accounts = await _clientApp.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();
            AuthenticationResult authResult = null;
            try
            {

                authResult = await _clientApp.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent.
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                authResult = await _clientApp.AcquireTokenInteractive(scopes)
                    .WithAccount(accounts.FirstOrDefault())
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
            }

            if (authResult == null)
            {
                throw new Exception("Authentication error");
            }

            var getVal = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
            MessageBox.Show(getVal);

        }

        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}