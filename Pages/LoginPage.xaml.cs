using ClockIn_Desktop.Classes;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Security.Credentials;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockIn_Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private MainPage mainPage;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = (MainPage)e.Parameter;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = emailTextBox.Text;
            string password = passwordBox.Password;

            string json = $"{{ \"Email\": \"{email}\", \"Password\": \"{password}\" }}";

            Debug.WriteLine(Utility.AUTHURL + "login");

            Dictionary<string, string> response = await Utility.SendToApi(Utility.AUTHURL + "login", json, "POST", "");

            switch (response["message"])
            {
                case "Logged In":
                    if (response["UserType"] != "1")
                    {
                        await Utility.ShowDialog("Incorrect Account Type", "User's May Not Access This Application");
                        return;
                    }

                    mainPage.SetProgressRing(true);

                    var vault = new PasswordVault();
                    var credential = new PasswordCredential("ClockIn", response["UserID"], response["RefreshToken"]);
                    vault.Add(credential);

                    mainPage.MainAuth.RefreshToken = response["RefreshToken"];
                    mainPage.MainAuth.UserID = response["UserID"];

                    bool result = await mainPage.MainAuth.GenerateAccessToken();

                    await mainPage.RefreshShifts();
                    await mainPage.RefreshUsers();

                    User user = mainPage.UserList.Find(x => x._ID == mainPage.MainAuth.UserID);
                    mainPage.MainAuth.UserName = $"{user.FirstName} {user.LastName}";

                    mainPage.SetProgressRing(false);

                    if (!result)
                    {
                        mainPage.LogOut();
                    }
                    else
                    {
                        mainPage.ShiftListNavigate();
                    }

                    break;
                case "Bad Credentials":
                case "Cannot Find User":
                    passwordBox.Password = "";
                    await Utility.ShowDialog("Bad Credentials", "Incorrect Username or Password!");
                    break;
            }
        }

        private void StackPanel_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                LoginButton_Click(null, null);
            }
        }
    }
}
