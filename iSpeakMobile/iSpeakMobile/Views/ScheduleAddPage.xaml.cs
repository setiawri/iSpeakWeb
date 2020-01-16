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
    public partial class ScheduleAddPage : ContentPage
    {
        private readonly Services.WebApiService webApiService = new Services.WebApiService();

        public ScheduleAddPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            DateTime timeStart = new DateTime(1970, 1, 1, tpStart.Time.Hours, tpStart.Time.Minutes, 0);
            DateTime timeEnd = new DateTime(1970, 1, 1, tpEnd.Time.Hours, tpEnd.Time.Minutes, 0);
            var added = await webApiService.ScheduleAddApi(Settings.LoginUser, pDay.SelectedItem.ToString(), timeStart.ToString("HH:mm"), timeEnd.ToString("HH:mm"), txtNotes.Text);
            if (string.IsNullOrEmpty(added.Message))
            {
                await Shell.Current.GoToAsync($"schedule");
            }
            else
            {
                await DisplayAlert("Error", added.Message, "OK");
            }
        }
    }
}