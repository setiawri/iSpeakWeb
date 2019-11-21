using iSpeakMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iSpeakMobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeaderView : ContentView
    {
        private readonly Services.WebApiService webApiService = new Services.WebApiService();

        public HeaderView()
        {
            InitializeComponent();
            GetProfile();
        }

        public async void GetProfile()
        {
            var user_login = await webApiService.UserLoginApi(Settings.LoginUser);
            lblFullname.Text = user_login.Fullname;
            lblUsername.Text = "( " + user_login.Username + " )";
        }
    }
}