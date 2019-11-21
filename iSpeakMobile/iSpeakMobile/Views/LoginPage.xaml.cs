using iSpeakMobile.Models;
using iSpeakMobile.ViewModels;
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
    public partial class LoginPage : ContentPage
    {
        private readonly Services.WebApiService webApiService = new Services.WebApiService();

        public LoginPage()
        {
            InitializeComponent();
            this.animationView.IsVisible = false;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            this.animationView.IsVisible = true;
            var data = (LoginViewModel)BindingContext;

            var result = await webApiService.LoginIdentityApi(data.Username, data.Password);
            if (!string.IsNullOrEmpty(result.access_token))
            {
                Settings.LoginUser = result.userName;
                Settings.AccessToken = result.access_token;
                Settings.ExpirationToken = DateTime.UtcNow.AddMinutes(30);
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                this.animationView.IsVisible = false;
                await DisplayAlert("Login", result.error_description, "OK");
            }
        }
    }
}