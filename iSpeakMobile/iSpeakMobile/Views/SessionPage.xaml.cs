using iSpeakMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Extended;
using Xamarin.Forms.Xaml;

namespace iSpeakMobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SessionPage : ContentPage
    {
        //private readonly Services.WebApiService webApiService = new Services.WebApiService();

        public SessionPage()
        {
            InitializeComponent();
            //BindingContext = this;
            //GetData();
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListView lv = (ListView)sender;
            Session session = (Session)lv.SelectedItem;
            await Shell.Current.GoToAsync($"sessiondetails?name={session.Date}");
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    GetData();
        //}

        //public async void GetData()
        //{
        //    var list = await webApiService.SessionApi(Settings.LoginUser);
        //    cv.ItemsSource = list;
        //    DataStore.Sessions = list;
        //}

        //async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    string tanggal = (e.CurrentSelection.FirstOrDefault() as Session).Date;
        //    await Shell.Current.GoToAsync($"sessiondetails?name={tanggal}");
        //}
    }
}