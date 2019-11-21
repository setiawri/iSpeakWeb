using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using iSpeakMobile.Services;
using iSpeakMobile.Views;
using iSpeakMobile.Models;

namespace iSpeakMobile
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            SetMainPage();
            //MainPage = new NavigationPage(new SessionPage());
            //DependencyService.Register<MockDataStore>();
            //MainPage = new AppShell();
        }

        private void SetMainPage()
        {
            if (string.IsNullOrEmpty(Settings.AccessToken))
            {
                MainPage = new LoginShell();
            }
            else
            {
                if (DateTime.UtcNow.AddMinutes(30) > Settings.ExpirationToken)
                {
                    MainPage = new LoginShell();
                }
                else
                {
                    MainPage = new AppShell();
                }
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
