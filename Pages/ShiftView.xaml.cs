using ClockIn_Desktop.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockIn_Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShiftView : Page
    {
        private List<Shift> shiftList;
        public ShiftView()
        {
            InitializeComponent();

            LoadShiftsIntoTable();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender; ;
            Shift row = null;
            ShiftEditorDialog shiftEditor;

            if (button.Content.Equals("Add Shift"))
            {
                shiftEditor = new ShiftEditorDialog(new Shift(), "Add");
                await shiftEditor.ShowAsync();
                LoadShiftsIntoTable();
            }
            else if (ShiftListView.SelectedItems.Count > 0)
            {
                try
                {
                    row = (Shift)ShiftListView.SelectedItem;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                Debug.WriteLine(button.Content);

                if (button.Content.Equals("Edit Shift")){

                    shiftEditor = new ShiftEditorDialog(row, "Edit");

                    await shiftEditor.ShowAsync();
                }
                else if (button.Content.Equals("Delete Shift")){

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
                        string jsonString = JsonConvert.SerializeObject(row);

                        Utility.UpdateShift("http://localhost:3000/shifts/delete", jsonString); 
                    }
                }
                
                LoadShiftsIntoTable();
            }
        }

        private void LoadShiftsIntoTable()
        {
            shiftList = Utility.LoadShifts(@"http://api.clockin.uk/shifts");
            ShiftListView.ItemsSource = shiftList;
        }
    }
}
