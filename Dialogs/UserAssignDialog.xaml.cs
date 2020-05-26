using ClockIn_Desktop.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockIn_Desktop.Pages
{
    public sealed partial class UserAssignDialog : ContentDialog
    {
        private List<User> userList;
        private List<CheckBox> checkboxList;

        private Shift shift;
        private Auth auth;

        public UserAssignDialog(List<User> userList, Shift shift, Auth auth)
        {
            this.userList = userList;
            this.shift = shift;
            this.auth = auth;
            checkboxList = new List<CheckBox>();

            InitializeComponent();
            SetupCheckBoxes();
        }

        private void SetupCheckBoxes()
        {
            foreach (User user in userList){
                string id = user._ID;

                CheckBox box = new CheckBox();
                box.Content = user.FirstName + " " + user.LastName;
                if (shift.Users.Contains(id)) box.IsChecked = true;

                checkboxList.Add(box);
                userStackPanel.Children.Add(box);
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            foreach (CheckBox box in checkboxList)
            {
                int index = checkboxList.IndexOf(box);
                string id = userList[index]._ID;
                if (box.IsChecked == true)
                {
                    if (!shift.Users.Contains(id)) shift.Users.Add(id);
                }
                else
                {
                    if (shift.Users.Contains(id)) shift.Users.Remove(id);
                    if (shift.ClockIn.ContainsKey(id)) shift.ClockIn.Remove(id);
                    if (shift.ClockOut.ContainsKey(id)) shift.ClockOut.Remove(id);
                    string json = "{\"shiftID\": \"" + shift._ID + "\",\"userID\" : \"" + id + "\"}";
                    await Utility.SendToApi(Utility.APIURL + "clock/reset", json, "DELETE", auth.AccessToken);
                }
            }

            string jsonString = JsonConvert.SerializeObject(shift);
            Debug.WriteLine(jsonString);
            var updateResult = await Utility.SendToApi(Utility.APIURL + "shifts/" + shift._ID, jsonString, "POST", auth.AccessToken);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
