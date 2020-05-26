using ClockIn_Desktop.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ClockIn_Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShiftList : Page
    { 
        private MainPage mainPage;
        private List<Shift> filteredList;

        public ShiftList()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = (MainPage)e.Parameter;

            fromDatePicker.Date = mainPage.ShiftFilters[0].Date;
            toDatePicker.Date = mainPage.ShiftFilters[1].Date;

            LoadShiftsIntoTable();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            mainPage.SelectedShift = null;
            mainPage.ShiftViewNavigate();
        }

        private async void LoadShiftsIntoTable()
        {
            mainPage.SetProgressRing(true);
            await mainPage.RefreshShifts();
            mainPage.ShiftList = mainPage.ShiftList.OrderBy(x => x.Start).ToList();
            filteredList = mainPage.ShiftList.Where(x => x.Start.Date >= mainPage.ShiftFilters[0].Date && x.Finish.Date <= mainPage.ShiftFilters[1].Date).ToList();
            ShiftListView.ItemsSource = filteredList;
            mainPage.SetProgressRing(false);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadShiftsIntoTable();
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime from = fromDatePicker.Date.Value.Date;
            DateTime to = toDatePicker.Date.Value.Date;

            if (from >= to)
            {
                await Utility.ShowDialog("Incorrect Filter Format", "The From date must be before the To Date");
            }
            else
            {
                mainPage.ShiftFilters[0] = fromDatePicker.Date.Value.Date;
                mainPage.ShiftFilters[1] = toDatePicker.Date.Value.Date;

                LoadShiftsIntoTable();
            }
        }
        
        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            mainPage.ShiftFilters[0] = DateTime.Now.Date.AddMonths(-1);
            mainPage.ShiftFilters[1] = DateTime.Now.Date;

            fromDatePicker.Date = mainPage.ShiftFilters[0].Date;
            toDatePicker.Date = mainPage.ShiftFilters[1].Date;

            LoadShiftsIntoTable();
        }

        private void ShiftListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainPage.SelectedShift = (Shift)ShiftListView.SelectedItem;
            mainPage.ShiftViewNavigate();
        }
    }
}
