using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using NHarvestApi;

namespace HarvestApps.WinStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly HarvestApi<ApiBasicAuthSettings> _harvestApi;

        public static ApplicationDataContainer SettingsContainer { get; set; }

        private static ApiBasicAuthSettings ApiSettings
        {
            get
            {
                var s = SettingsContainer.Values["HarvestAuthInfo"] as string;
                if (s != null) return JsonConvert.DeserializeObject<ApiBasicAuthSettings>(s);

                SettingsContainer.Values["HarvestAuthInfo"] = null;
                return null;
            }
            set { SettingsContainer.Values["HarvestAuthInfo"] = JsonConvert.SerializeObject(value); }
        }

        static MainPage()
        {
            SettingsContainer = ApplicationData.Current.LocalSettings;
        }

        public MainPage()
        {
            this.InitializeComponent();

            // define what authentication this app is doing
            // ideally this low-level abstraction would be hidden somewhere and DIed into a Core.IRT interface impl
            _harvestApi = new HarvestApi<ApiBasicAuthSettings>(new BasicAuthHttpClientFactory());

            if (ApiSettings == null) return;

            // set text fields from cached auth settings
            SubdomainBox.Text = ApiSettings.Subdomain;
            UsernameBox.Text = ApiSettings.Username;
            PasswordBox.PlaceholderText = "Password is stored";
        }

        private async void Submit_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Expression<Func<IHarvestResourcePathFactory, string>> uriFactoryExpression = factory => factory.WhoAmI();
            await Get<Hash>(uriFactoryExpression);
        }

        private async Task Get<T>(Expression<Func<IHarvestResourcePathFactory, string>> uriFactoryExpression)
        {
            if (!string.IsNullOrWhiteSpace(SubdomainBox.Text) && !string.IsNullOrWhiteSpace(UsernameBox.Text) &&
                !string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                var harvestAuthInfo = new ApiBasicAuthSettings(SubdomainBox.Text);
                harvestAuthInfo.SetCredentials(UsernameBox.Text, PasswordBox.Password);
                ApiSettings = harvestAuthInfo;
            }
            if (_harvestApi == null)
            {
                // TODO: tell user that authentication is hosed
                return;
            }

            // ideally this low-level abstraction would be hidden somewhere and DIed into a Core.IRT interface impl
            var response = await _harvestApi.Get<T>(ApiSettings, uriFactoryExpression);
            OutputBox.DataContext = response;
        }
    }

    class Hash
    {
        public User User { get; set; }
        public override string ToString()
        {
            return User != null ? User.ToString() : "No data";
        }
    }

    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public override string ToString()
        {
            return string.Format("User with id: {0}, first name: {1}, last name: {2}", Id, FirstName, LastName);
        }
    }

}
