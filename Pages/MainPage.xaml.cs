using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using System.Collections.Generic;
using Windows.UI.Xaml;
using System.Linq;
using ClockIn_Desktop.Classes;
using System;
using System.Threading.Tasks;

namespace ClockIn_Desktop
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            MainAuth = new Auth();

            this.InitializeComponent();
            SetProgressRing(true);

            ShiftFilters[0] = DateTime.Now.Date.AddMonths(-1);
            ShiftFilters[1] = DateTime.Now.Date;

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            NavView.SelectedItem = NavView.MenuItems.ElementAt(0);

            StartUp();

            SetProgressRing(false);
        }

        private async void StartUp()
        {
            if (MainAuth.UserID == null)
            {
                LoginNavigate();
            }
            else
            {
                await MainAuth.GenerateAccessToken();

                await RefreshShifts();
                await RefreshUsers();

                User user = UserList.Find(x => x._ID == MainAuth.UserID);
                MainAuth.UserName = $"{user.FirstName} {user.LastName}";

                ShiftListNavigate();
            }
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs args)
        {
            NavView.IsPaneOpen = false;
        }

        private async void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;

            if (mainFrame.SourcePageType.Equals(typeof(Pages.LoginPage)))
            {
                await Utility.ShowDialog("Navigation Disabled", "You cannnot navigate to other parts of the app until you login!");
                return;
            }
            
            if (args.IsSettingsInvoked)
            {
                mainFrame.Navigate(typeof(Pages.SettingsPage), this);
            }
            else
            {
                switch (item.Tag)
                {
                    case "shifts":
                        mainFrame.Navigate(typeof(Pages.ShiftList), this);
                        break;
                    case "users":
                        mainFrame.Navigate(typeof(Pages.UserList), this);
                        break;
                    case "export":
                        mainFrame.Navigate(typeof(Pages.ExportPage), this);
                        break;
                    default:
                        Debug.WriteLine("This tag has not been defined in moving!");
                        break;
                }
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (NavView.IsBackEnabled)
            {
                ShiftListNavigate();
            }
        }

        public void SetProgressRing(bool active)
        {
            MainProgressRing.IsActive = active;
        }

        public void LoginNavigate()
        {
            mainFrame.Navigate(typeof(Pages.LoginPage), this);
            NavView.IsBackEnabled = false;
        }

        public void ShiftListNavigate()
        {
            mainFrame.Navigate(typeof(Pages.ShiftList), this);
            NavView.IsBackEnabled = false;
        }

        public void ShiftViewNavigate()
        {
            mainFrame.Navigate(typeof(Pages.ShiftView), this);
            NavView.IsBackEnabled = mainFrame.CanGoBack;
        }

        public async Task<bool> RefreshShifts()
        {
            List<Shift> backup = ShiftList;
            for (int i = 0; i < 3; i++)
            {
                ShiftList = await Utility.LoadShifts(Utility.APIURL + "shifts", MainAuth);
                ShiftList = ShiftList.OrderBy(x => x.Start).ToList();
                if (ShiftList != null) return true;
            }
            ShiftList = backup;
            await Utility.ShowDialog("API Connection Error", "There was an error retrieving shifts. Information displayed might be outdated!");
            return false;
        }

        public async Task<bool> RefreshUsers()
        {
            List<User> backup = UserList;
            for (int i = 0; i < 3; i++)
            {
                UserList = await Utility.LoadUsers(Utility.APIURL + "users", MainAuth);
                if (UserList != null) return true;
            }
            UserList = backup;
            await Utility.ShowDialog("API Connection Error", "There was an error retrieving users. Information displayed might be outdated!");
            return false;
        }

        public void LogOut()
        {
            MainAuth.LogOut();
            LoginNavigate();
        }

        public List<Shift> ShiftList { get; set; } = new List<Shift>();
        public List<User> UserList { get; set; } = new List<User>();
        public Shift SelectedShift { get; set; }
        public DateTime[] ShiftFilters { get; set; } = new DateTime[2];
        public Auth MainAuth { get; }
    }
}
