using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using System.Collections.Generic;
using Windows.UI.Xaml;
using System.Linq;

namespace ClockIn_Desktop
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems.ElementAt(0);
            mainFrame.Navigate(typeof(Pages.ShiftView));
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs args)
        {
            NavView.IsPaneOpen = false;
        }

        private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;
            switch (item.Tag)
            {
                case "shifts":
                    mainFrame.Navigate(typeof(Pages.ShiftView));
                    break;
                case "users":
                    mainFrame.Navigate(typeof(Pages.UserView));
                    break;
                default:
                    Debug.WriteLine("This tag has not been defined in moving!");
                    break;
            }   
        }
    }
}
