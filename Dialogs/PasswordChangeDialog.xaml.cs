using ClockIn_Desktop.Classes;
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

namespace ClockIn_Desktop.Dialogs
{
    public sealed partial class PasswordChangeDialog : ContentDialog
    {
        private string UserID;

        public PasswordChangeDialog(string userID)
        {
            UserID = userID;
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            bool result = ValidationCheck();
            
            if (result)
            {
                string jsonString = $"{{ \"UserID\":\"{UserID}\", \"CurrentPass\":\"{currentPass.Password}\", \"NewPass\":\"{newPass.Password}\"}}";
                var response = await Utility.SendToApi(Utility.AUTHURL + "passwordChange", jsonString, "POST", "");

                if (!response["message"].Equals("Password Changed"))
                {
                    errorText.Text = response["message"];
                }
                else
                {
                    this.Hide();
                    await Utility.ShowDialog("Success", "Password Changed");
                }
            }
        }

        private bool ValidationCheck()
        {
            if (newPass.Password != confirmPass.Password)
            {
                errorText.Text = "New Passwords Must Match!";
                return false;
            }
            else if (String.IsNullOrEmpty(currentPass.Password) || String.IsNullOrEmpty(newPass.Password) || String.IsNullOrEmpty(confirmPass.Password))
            {
                errorText.Text = "All fields must be filled!";
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
