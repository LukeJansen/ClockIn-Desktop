using ClockIn_Desktop.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockIn_Desktop.Pages
{
    public sealed partial class UserEditorDialog : ContentDialog
    {
        private User user;
        private Auth auth;

        private Dictionary<string, string> response;

        private string mode;
        private bool CanClose = false;

        public UserEditorDialog(User user, string mode, Auth auth)
        {
            this.user = user;
            this.mode = mode;
            this.auth = auth;
            this.InitializeComponent();

            var _enumval = Enum.GetValues(typeof(User.UserType)).Cast<User.UserType>();
            typeComboBox.ItemsSource = _enumval.ToList();

            Setup();
        }

        private async void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!CanClose)
                args.Cancel = true;
        }

        private void Setup()
        {
            if (mode.Equals("Edit"))
            {
                firstNameTextBox.Text = user.FirstName;
                lastNameTextBox.Text = user.LastName;
                emailTextBox.Text = user.Email;
                phoneTextBox.Text = user.Phone;
                dobDatePicker.Date = user.DOB;
                typeComboBox.SelectedItem = user.Type;
            }
        }

        private async void UserEditorDialog_Save(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (await ValidationCheck())
            {
                CanClose = true;

                user.FirstName = firstNameTextBox.Text;
                user.LastName = lastNameTextBox.Text;
                user.Phone = phoneTextBox.Text;
                user.Email = emailTextBox.Text;
                user.DOB = dobDatePicker.Date.Value.Date;
                user.Type = (User.UserType)typeComboBox.SelectedItem;

                string jsonString = JsonConvert.SerializeObject(user);

                if (mode.Equals("Add"))
                {
                    response = await Utility.SendToApi(Utility.APIURL + "users", jsonString, "POST", auth.AccessToken);
                    jsonString = $"{{\"UserID\": \"{response["UserID"]}\"}}";
                    response = await Utility.SendToApi(Utility.AUTHURL + "register", jsonString, "POST", auth.AccessToken);
                    await Utility.ShowPassword(response["password"]);
                }
                else if (mode.Equals("Edit"))
                {
                    await Utility.SendToApi(Utility.APIURL + "users/" + user._ID, jsonString, "POST", auth.AccessToken);
                }
            }
        }

        private void UserEditorDialog_Cancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            CanClose = true;
            Debug.WriteLine("Cancelled");
        }

        private async Task<bool> ValidationCheck()
        {
            errorTextBox.Text = "";

            //First Name Check
            if (string.IsNullOrEmpty(firstNameTextBox.Text))
            {
                errorTextBox.Text = "First Name cannot be empty!";
                firstNameTextBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            else
            {
                firstNameTextBox.ClearValue(TextBox.BorderBrushProperty);
            }

            //Last Name Check
            if (string.IsNullOrEmpty(lastNameTextBox.Text))
            {
                errorTextBox.Text = "Last Name cannot be empty!";
                lastNameTextBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            else
            {
                lastNameTextBox.ClearValue(TextBox.BorderBrushProperty);
            }

            //Phone Check
            if (string.IsNullOrEmpty(phoneTextBox.Text))
            {
                errorTextBox.Text = "Phone Number cannot be empty!";
                phoneTextBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            else if (phoneTextBox.Text.Length != 11)
            {
                errorTextBox.Text = "Phone Number is not 11 digits in length!";
                phoneTextBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            else
            {
                phoneTextBox.ClearValue(TextBox.BorderBrushProperty);
            }

            //Email Check
            if (string.IsNullOrEmpty(emailTextBox.Text))
            {
                errorTextBox.Text = "Email Address cannot be empty!";
                emailTextBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            else if (!emailTextBox.Text.Contains("@") || !emailTextBox.Text.Contains(".") || emailTextBox.Text.EndsWith("."))
            {
                errorTextBox.Text = "Email Address entered is not valid!";
                emailTextBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            else
            {
                emailTextBox.ClearValue(TextBox.BorderBrushProperty);
            }

            //DOB Check
            if (!dobDatePicker.Date.HasValue)
            {
                errorTextBox.Text = "Date has not been selected!";
                dobDatePicker.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            else
            {
                dobDatePicker.ClearValue(TextBox.BorderBrushProperty);
            }

            //User Type Check
            if (typeComboBox.SelectedItem == null)
            {
                errorTextBox.Text = "User Type has not been selected!";
                typeComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            else
            {
                typeComboBox.ClearValue(TextBox.BorderBrushProperty);
            }

            return true;
        }
    }
}
