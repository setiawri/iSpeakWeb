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
    public partial class PaymentPage : ContentPage
    {
        private readonly Services.WebApiService webApiService = new Services.WebApiService();

        public PaymentPage()
        {
            InitializeComponent();
            BindingContext = this;
            GetData();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetData();
        }

        public async void GetData()
        {
            var list = await webApiService.PaymentApi(Settings.LoginUser);
            cv.ItemsSource = list;
            DataStore.Payments = list;
        }
    }
}