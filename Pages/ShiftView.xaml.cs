using ClockIn_Desktop.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ClockIn_Desktop.Pages
{
    public sealed partial class ShiftView : Page
    {
        private MainPage mainPage;
        private Shift shift;
        private Dictionary<string, List<TimePicker>> gridElements;
        private string mode;

        public ShiftView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
            gridElements = new Dictionary<string, List<TimePicker>>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = (MainPage)e.Parameter;
            shift = mainPage.SelectedShift;
            LoadShift();
        }

        private void LoadShift()
        {
            if (shift == null)
            {
                mode = "Add";
                titleText.Text = "Add Shift";
                shift = new Shift();
            }
            else
            {
                mode = "Edit";
                titleText.Text = "Edit Shift";

                locationTextBox.Text = shift.Location;
                roleTextBox.Text = shift.Role;
                startDatePicker.Date = shift.Start.Date;
                startTimePicker.Time = shift.Start.TimeOfDay;
                finishDatePicker.Date = shift.Finish.Date;
                finishTimePicker.Time = shift.Finish.TimeOfDay;

                foreach (string userID in shift.Users)
                {
                    User user = mainPage.UserList.Find(i => i._ID == userID);

                    RowDefinition row = new RowDefinition();
                    userGrid.RowDefinitions.Add(row);

                    TextBlock nameText = new TextBlock();
                    nameText.Margin = new Thickness(0, 25, 25, 0);
                    nameText.Text = user.FirstName + " " + user.LastName;

                    userGrid.Children.Add(nameText);
                    Grid.SetRow(nameText, userGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(nameText, 0);

                    if (shift.ClockIn.ContainsKey(userID))
                    {
                        TimePicker inElement = new TimePicker();
                        inElement.Margin = new Thickness(0, 25, 25, 0);
                        inElement.ClockIdentifier = "12HourClock";
                        DateTime time = DateTime.Parse(shift.ClockIn[userID], null, DateTimeStyles.RoundtripKind);
                        inElement.Time = time.TimeOfDay;

                        Button resetButton = new Button();
                        resetButton.Padding = new Thickness(25, 10, 25, 10);
                        resetButton.Content = "Reset";
                        resetButton.Tag = userID;
                        resetButton.Click += ResetButton_Click;

                        userGrid.Children.Add(inElement);
                        userGrid.Children.Add(resetButton);
                        gridElements[userID] = new List<TimePicker>();
                        gridElements[userID].Add(inElement);
                        Grid.SetRow(inElement, userGrid.RowDefinitions.Count - 1);
                        Grid.SetRow(resetButton, userGrid.RowDefinitions.Count - 1);
                        Grid.SetColumn(inElement, 1);
                        Grid.SetColumn(resetButton, 3);
                    }
                    else
                    {
                        TextBlock inElement = new TextBlock();
                        inElement.Margin = new Thickness(0, 25, 25, 0);
                        inElement.Text = "Not ClockedIn!";

                        userGrid.Children.Add(inElement);
                        Grid.SetRow(inElement, userGrid.RowDefinitions.Count - 1);
                        Grid.SetColumn(inElement, 1);
                    }
                    
                    
                    if (shift.ClockOut.ContainsKey(userID))
                    {
                        TimePicker outElement = new TimePicker();
                        outElement.Margin = new Thickness(0, 25, 25, 0);
                        outElement.ClockIdentifier = "12HourClock";
                        DateTime time = DateTime.Parse(shift.ClockOut[userID], null, DateTimeStyles.RoundtripKind);
                        outElement.Time = time.TimeOfDay;

                        userGrid.Children.Add(outElement);
                        gridElements[userID].Add(outElement);
                        Grid.SetRow(outElement, userGrid.RowDefinitions.Count - 1);
                        Grid.SetColumn(outElement, 2);
                    }
                    else
                    {
                        TextBlock outElement = new TextBlock();
                        outElement.Margin = new Thickness(0, 25, 25, 0);
                        outElement.Text = "Not ClockedOut!";

                        userGrid.Children.Add(outElement);
                        Grid.SetRow(outElement, userGrid.RowDefinitions.Count - 1);
                        Grid.SetColumn(outElement, 2);
                    }
                }
            }
        }

        private async void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode.Equals("Add"))
            {
                await Utility.ShowDialog("Not Available!", "You must save the shift before assigning users!");
            }
            else
            {
                UserAssignDialog assignDialog = new UserAssignDialog(mainPage.UserList, shift, mainPage.MainAuth);

                await assignDialog.ShowAsync();

                mainPage.RefreshShifts();
                mainPage.SelectedShift = mainPage.ShiftList.Find(i => i._ID == shift._ID);

                mainPage.ShiftViewNavigate();
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            string userID = (string)((Button)sender).Tag;
            string shiftID = shift._ID;
            string json = "{\"shiftID\": \"" + shiftID + "\",\"userID\" : \"" + userID + "\"}";

            ContentDialog DeleteCheck = new ContentDialog()
            {
                Title = "Reset Clock Status",
                Content = "Are you sure you want to reset this user's clock status?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            var result = await DeleteCheck.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await Utility.SendToApi(Utility.APIURL + "clock/reset", json, "DELETE", mainPage.MainAuth.AccessToken);

                mainPage.RefreshShifts();
                mainPage.SelectedShift = mainPage.ShiftList.Find(i => i._ID == shiftID);

                mainPage.ShiftViewNavigate();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationCheck())
            {
                shift.Location = locationTextBox.Text;
                shift.Role = roleTextBox.Text;
                shift.Start = startDatePicker.Date.Value.Date;
                shift.Start = shift.Start.AddSeconds(startTimePicker.Time.TotalSeconds);
                shift.Finish = finishDatePicker.Date.Value.Date;
                shift.Finish = shift.Finish.AddSeconds(finishTimePicker.Time.TotalSeconds);

                foreach (string userID in gridElements.Keys)
                {
                    User user = mainPage.UserList.Find(i => i._ID == userID);
                    Debug.WriteLine(gridElements[userID].Count);
                    if (gridElements[userID].Count >= 1)
                    {
                        DateTime clockIn = shift.Start.Date;
                        shift.ClockIn[userID] = clockIn.AddSeconds(gridElements[userID][0].Time.TotalSeconds).ToUniversalTime().ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz");
                    }
                    if (gridElements[userID].Count >= 2)
                    {
                        DateTime clockOut = shift.Finish.Date;
                        shift.ClockOut[userID] = clockOut.AddSeconds(gridElements[userID][1].Time.TotalSeconds).ToUniversalTime().ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz");
                    }
                }

                string jsonString = JsonConvert.SerializeObject(shift, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                });

                if (mode.Equals("Add"))
                {
                    await Utility.SendToApi(Utility.APIURL + "shifts", jsonString, "POST", mainPage.MainAuth.AccessToken);
                }
                else if (mode.Equals("Edit"))
                {
                    await Utility.SendToApi(Utility.APIURL + "shifts/" + shift._ID, jsonString, "POST", mainPage.MainAuth.AccessToken);
                }

                mainPage.ShiftListNavigate();
            }
        }
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog DeleteCheck = new ContentDialog()
            {
                Title = "Delete Shift",
                Content = "Are you sure you want to delete this shift?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            var result = await DeleteCheck.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string jsonString = JsonConvert.SerializeObject(shift);
                var updateResult = await Utility.SendToApi(Utility.APIURL + "shifts/" + shift._ID, jsonString, "DELETE", mainPage.MainAuth.AccessToken);

                mainPage.ShiftListNavigate();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mainPage.ShiftListNavigate();
        }

        private bool ValidationCheck()
        {
            SolidColorBrush errorBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            errorTextBox.Text = "";

            // Location Check
            if (string.IsNullOrEmpty(locationTextBox.Text))
            {
                errorTextBox.Text = "Location cannot be empty!";
                locationTextBox.BorderBrush = errorBrush;
                return false;
            }
            else
            {
                locationTextBox.ClearValue(BorderBrushProperty);
            }

            // Role Check
            if (string.IsNullOrEmpty(roleTextBox.Text))
            {
                errorTextBox.Text = "Role cannot be empty!";
                roleTextBox.BorderBrush = errorBrush;
                return false;
            }
            else
            {
                roleTextBox.ClearValue(BorderBrushProperty);
            }

            // Start Check
            if (!startDatePicker.Date.HasValue)
            {
                errorTextBox.Text = "You must choose a start date!";
                startDatePicker.BorderBrush = errorBrush;
                return false;
            }
            else
            {
                startDatePicker.ClearValue(BorderBrushProperty);
            }

            // Finish Check
            if (!finishDatePicker.Date.HasValue)
            {
                errorTextBox.Text = "You must choose a finish date!";
                finishDatePicker.BorderBrush = errorBrush;
                return false;
            }
            else
            {
                finishDatePicker.ClearValue(BorderBrushProperty);
            }

            DateTime start = startDatePicker.Date.Value.Date.AddSeconds(startTimePicker.Time.TotalSeconds);
            DateTime finish = finishDatePicker.Date.Value.Date.AddSeconds(finishTimePicker.Time.TotalSeconds);

            if (finish <= start)
            {
                errorTextBox.Text = "Finish must be after the start date!";
                startDatePicker.BorderBrush = errorBrush;
                finishDatePicker.BorderBrush = errorBrush;
                return false;
            }
            else
            {
                startDatePicker.ClearValue(BorderBrushProperty);
                finishDatePicker.ClearValue(BorderBrushProperty);
            }

            return true;
        }
    }
}
