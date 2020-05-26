using ClockIn_Desktop.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockIn_Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserList : Page
    {
        private MainPage mainPage;
        private List<User> searchList;
        private string order = "ASC";

        public UserList()
        {
            InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            searchList = new List<User>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = (MainPage)e.Parameter;

            LoadUsersIntoTable();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            User row = null;
            UserEditorDialog userEditor;

            if (button.Content.Equals("Add User"))
            {
                userEditor = new UserEditorDialog(new User(), "Add", mainPage.MainAuth);
                await userEditor.ShowAsync();
                LoadUsersIntoTable();
            }
            else if (UserListView.SelectedItems.Count > 0)
            {
                try
                {
                    row = (User)UserListView.SelectedItem;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                if (button.Content.Equals("Edit User"))
                {
                    userEditor = new UserEditorDialog(row, "Edit", mainPage.MainAuth);

                    await userEditor.ShowAsync();
                }
                else if (button.Content.Equals("Delete User"))
                {
                    ContentDialog DeleteCheck = new ContentDialog()
                    {
                        Title = "Delete User",
                        Content = "Are you sure you want to delete the user: " + row.FirstName + " " + row.LastName +  "?",
                        PrimaryButtonText = "Yes",
                        CloseButtonText = "No"
                    };

                    var result = await DeleteCheck.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        string jsonString = $"{{\"UserID\":\"{row._ID}\"}}";
                        await Utility.SendToApi(Utility.AUTHURL + "remove", jsonString, "POST", mainPage.MainAuth.AccessToken);

                        jsonString = JsonConvert.SerializeObject(row);
                        var updateResult = await Utility.SendToApi(Utility.APIURL + "users/" + row._ID, jsonString, "DELETE", mainPage.MainAuth.AccessToken);
                    }
                }
                else if (button.Content.Equals("Reset Password"))
                {
                    ContentDialog ResetCheck = new ContentDialog()
                    {
                        Title = "Delete User",
                        Content = "Are you sure you want to reset " + row.FirstName + " " + row.LastName + "'s password?",
                        PrimaryButtonText = "Yes",
                        CloseButtonText = "No"
                    };

                    var result = await ResetCheck.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        string jsonString = $"{{\"UserID\":\"{row._ID}\"}}";
                        var updateResult = await Utility.SendToApi(Utility.AUTHURL + "reset", jsonString, "POST", mainPage.MainAuth.AccessToken);
                        await Utility.ShowPassword(updateResult["password"]);
                    }
                }
                
                LoadUsersIntoTable();
            }
            else
            {
                await Utility.ShowDialog("No User Selected", "Please select a user before using this option!");
            }
        }

        private async void LoadUsersIntoTable()
        {
            mainPage.SetProgressRing(true);
            await mainPage.RefreshUsers();
            UserListView.ItemsSource = mainPage.UserList;
            mainPage.SetProgressRing(false);
        }

        private void UserListView_Sorting(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumnEventArgs e)
        {
            if (order.Equals("ASC"))
            {
                Debug.WriteLine(order);
                mainPage.UserList = mainPage.UserList.OrderBy(o => o.FirstName).ToList();
                UserListView.ItemsSource = mainPage.UserList;
                order = "DESC";
            }
            else
            {
                Debug.WriteLine(order);
                mainPage.UserList = mainPage.UserList.OrderByDescending(o => o.FirstName).ToList();
                UserListView.ItemsSource = mainPage.UserList;
                order = "ASC";
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Length > 0)
            {
                searchList.Clear();

                foreach (User user in mainPage.UserList)
                {
                    string name = user.FirstName + " " + user.LastName;
                    string pattern = "^.*" + SearchTextBox.Text + ".*$";

                    Match m = Regex.Match(name, pattern, RegexOptions.IgnoreCase);

                    if (m.Success)
                    {
                        searchList.Add(user);
                    }
                }

                UserListView.ItemsSource = null;
                UserListView.ItemsSource = searchList;
            }
            else
            {
                UserListView.ItemsSource = mainPage.UserList;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsersIntoTable();
        }
    }
}
