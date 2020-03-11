using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using ClockIn_Desktop.Classes;
using Newtonsoft.Json;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockIn_Desktop.Pages
{
    public sealed partial class ShiftEditorDialog : ContentDialog
    {
        private Shift shift;
        private string mode;

        public ShiftEditorDialog(Shift shift, string mode)
        {
            this.shift = shift;
            this.mode = mode;
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            if (mode.Equals("Add"))
            {               
                startDatePicker.Date = DateTimeOffset.Parse(DateTime.Now.Date.ToString());
                finishDatePicker.Date = DateTimeOffset.Parse(DateTime.Now.Date.ToString());
                Debug.WriteLine(startDatePicker.Date.Value.DateTime.ToString());
            }
            else if (mode.Equals("Edit"))
            {
                locationTextBox.Text = shift.Location;
                roleTextBox.Text = shift.Role;
                startDatePicker.Date = DateTimeOffset.Parse(shift.Start.Date.ToString());
                startTimePicker.Time = shift.Start.TimeOfDay;
                finishDatePicker.Date = DateTimeOffset.Parse(shift.Finish.Date.ToString());
                finishTimePicker.Time = shift.Finish.TimeOfDay;
            }
        }

        private void ShiftEditorDialog_Save(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            shift.Location = locationTextBox.Text;
            shift.Role = roleTextBox.Text;
            shift.Start = startDatePicker.Date.Value.DateTime;
            shift.Start = shift.Start.AddSeconds(startTimePicker.Time.TotalSeconds);
            shift.Finish = finishDatePicker.Date.Value.DateTime;
            shift.Finish = shift.Finish.AddSeconds(finishTimePicker.Time.TotalSeconds);

            string jsonString = JsonConvert.SerializeObject(shift);
            Debug.WriteLine(jsonString);
            
            if (mode.Equals("Add"))
            {
                Utility.UpdateShift("http://localhost:3000/shifts", jsonString);
                //Utility.UpdateShift("http://api.clockin.uk/shifts", jsonString);
            }
            else if (mode.Equals("Edit")) { 
                Utility.UpdateShift("http://localhost:3000/shifts/update", jsonString); 
                //Utility.UpdateShift("http://api.clockin.uk/shifts/update", jsonString); 
            }
        }

        private void ShiftEditorDialog_Cancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Debug.WriteLine("Cancelled");
        }

        private void DatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            DateTimeOffset date = (DateTimeOffset)sender.Date;
            sender.Date = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, new TimeSpan(0));

            Debug.WriteLine(sender.Date.ToString());
        }
    }
}
