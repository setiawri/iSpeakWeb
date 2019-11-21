using iSpeakMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iSpeakMobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        private readonly Services.WebApiService webApiService = new Services.WebApiService();

        public HomePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetData();
        }

        public async void GetData()
        {
            var list = await webApiService.InvoiceApi(Settings.LoginUser);
            cv.ItemsSource = list;
            DataStore.Invoices = list;
        }

        private async void Payment_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string No = button.CommandParameter.ToString();
            var list = await webApiService.PaymentApi(Settings.LoginUser);
            DataStore.Payments = list.Where(x => x.NoInvoice == No).ToList();
            await Shell.Current.GoToAsync($"paymentbyno");
        }

        private async void Session_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Guid Id = new Guid(button.CommandParameter.ToString());
            var list = await webApiService.SessionApi(Settings.LoginUser);
            DataStore.Sessions = list.Where(x => x.SaleInvoiceItems_Id == Id).ToList();
            await Shell.Current.GoToAsync($"sessionbyid");
        }
    }
}