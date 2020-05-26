﻿using ClockIn_Desktop.Dialogs;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockIn_Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private MainPage mainPage;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = (MainPage)e.Parameter;
            helloTextBlock.Text = $"Hello {mainPage.MainAuth.UserName}";
        }

        private void Logout_Button_Click(object sender, RoutedEventArgs e)
        {
            mainPage.LogOut();
        }

        private async void Change_Button_Click(object sender, RoutedEventArgs e)
        {
            PasswordChangeDialog dialog = new PasswordChangeDialog(mainPage.MainAuth.UserID);
            await dialog.ShowAsync();
        }
    }
}
