using ClockIn_Desktop.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockIn_Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExportPage : Page
    {
        DateTime from, to;
        StorageFile file;
        MainPage mainPage;

        public ExportPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = (MainPage)e.Parameter;
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            mainPage.SetProgressRing(true);

            if (fromDatePicker.Date == null || toDatePicker.Date == null)
            {
                await Utility.ShowDialog("Date Not Selected", "You must enter a date into both From and To!");
            }
            else
            {
                from = fromDatePicker.Date.Value.Date;
                to = toDatePicker.Date.Value.Date;

                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Comma Seperated Value", new List<string>() { ".csv" });
                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";

                file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);

                    //// write to file
                    //await FileIO.WriteTextAsync(file, file.Name);
                    //// Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                    //// Completing updates may require Windows to ask for user input.
                    //FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    //if (status == FileUpdateStatus.Complete)
                    //{
                    //    //File Update Complete
                    //}

                    switch (filterBox.SelectedIndex)
                    {
                        case 0:
                            ExportShifts();
                            break;
                        case 1:
                            ExportUsersHours();
                            break;
                        default:
                            Debug.WriteLine("Value not defined!");
                            break;
                    }
                }

                mainPage.SetProgressRing(false);
            }
        }

        private async void ExportShifts()
        {
            List<Shift> filteredList = mainPage.ShiftList.Where(x => x.Start.Date >= from.Date && x.Finish.Date <= to.Date).ToList();
            List<string> lines = new List<string>();
            lines.Add("Location, Role, Start, Finish");

            foreach (Shift shift in filteredList)
            {
                lines.Add(
                      shift.Location + "," 
                    + shift.Role + "," 
                    + shift.Start.ToShortDateString() + " " + shift.Start.ToShortTimeString() + "," 
                    + shift.Finish.ToShortDateString() + " " + shift.Finish.ToShortTimeString());
            }

            await FileIO.WriteLinesAsync(file, lines);
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            if (status == FileUpdateStatus.Complete)
            {
                await Utility.ShowDialog("Export Complete", "Your export is complete!");
            }
        }

        private async void ExportUsersHours()
        {
            List<Shift> filteredList = mainPage.ShiftList.Where(x => x.Start.Date >= from.Date && x.Finish.Date <= to.Date).ToList();
            List<string> lines = new List<string>();
            Dictionary<string, double> userHours = new Dictionary<string, double>();
            lines.Add("First Name, Last Name, Time Worked");

            foreach (Shift shift in filteredList)
            {
                foreach(string user in shift.ClockIn.Keys)
                {
                    DateTime inTime = DateTime.Parse(shift.ClockIn[user]);
                    DateTime outTime = DateTime.Parse(shift.ClockOut[user]);

                    TimeSpan ts = outTime - inTime;

                    if (userHours.ContainsKey(user))
                    {
                        userHours[user] += ts.TotalHours;
                    }
                    else
                    {
                        userHours[user] = ts.TotalHours;
                    }
                }
            }

            foreach(string userID in userHours.Keys)
            {
                User user = mainPage.UserList.Find(x => x._ID == userID);

                lines.Add(
                      user.FirstName + "," 
                    + user.LastName + ","
                    + userHours[userID]);
            }

            await FileIO.WriteLinesAsync(file, lines);
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            if (status == FileUpdateStatus.Complete)
            {
                await Utility.ShowDialog("Export Complete", "Your export is complete!");
            }
        }
    }
}
