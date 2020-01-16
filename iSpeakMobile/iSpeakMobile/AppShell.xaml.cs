using iSpeakMobile.Models;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace iSpeakMobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        readonly Dictionary<string, Type> routes = new Dictionary<string, Type>();
        public Dictionary<string, Type> Routes { get { return routes; } }

        public ICommand InfoCommand => new Command<string>(async (url) =>
        {
            var supportsUri = await Launcher.CanOpenAsync(url);
            if (supportsUri)
                await Launcher.OpenAsync(url);
        });

        public ICommand LogoutCommand => new Command(() =>
        {
            Settings.AccessToken = "";
            Settings.LoginUser = "";
        });

        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            BindingContext = this;
        }

        void RegisterRoutes()
        {
            routes.Add("paymentbyno", typeof(Views.PaymentByNoPage));
            routes.Add("sessiondetails", typeof(Views.SessionDetailsPage));
            routes.Add("sessionbyid", typeof(Views.SessionByIdPage));
            routes.Add("schedule", typeof(Views.SchedulePage));
            routes.Add("scheduleadd", typeof(Views.ScheduleAddPage));
            routes.Add("scheduleedit", typeof(Views.ScheduleEditPage));

            foreach (var item in routes)
            {
                Routing.RegisterRoute(item.Key, item.Value);
            }
        }

        private void Logout_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new LoginShell();
        }
    }
}
