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
    public partial class PaymentByNoPage : ContentPage
    {
        public PaymentByNoPage()
        {
            InitializeComponent();
            BindingContext = this;
            cv.ItemsSource = DataStore.Payments;
            Title = DataStore.Payments.Select(x => x.NoInvoice).FirstOrDefault();
        }
    }
}