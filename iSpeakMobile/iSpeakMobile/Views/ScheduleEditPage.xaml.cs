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
    [QueryProperty("Name", "name")]
    public partial class ScheduleEditPage : ContentPage
    {
        private readonly Services.WebApiService webApiService = new Services.WebApiService();
        private string schedule_id;

        public string Name
        {
            set
            {
                var schedule = DataStore.TutorSchedules.Where(x => x.Id.ToString() == Uri.UnescapeDataString(value)).FirstOrDefault();
                BindingContext = schedule;
                schedule_id = schedule.Id.ToString();
                pDay.SelectedIndex = int.Parse(schedule.DayOfWeek);
                tpStart.Time = new TimeSpan(schedule.StartTime.Hour, schedule.StartTime.Minute, 0);
                tpEnd.Time = new TimeSpan(schedule.EndTime.Hour, schedule.EndTime.Minute, 0);
            }
        }

        public ScheduleEditPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            DateTime timeStart = new DateTime(1970, 1, 1, tpStart.Time.Hours, tpStart.Time.Minutes, 0);
            DateTime timeEnd = new DateTime(1970, 1, 1, tpEnd.Time.Hours, tpEnd.Time.Minutes, 0);
            var edit = await webApiService.ScheduleEditApi(Settings.LoginUser, schedule_id, pDay.SelectedItem.ToString(), timeStart.ToString("HH:mm"), timeEnd.ToString("HH:mm"), txtNotes.Text);
            if (string.IsNullOrEmpty(edit.Message))
            {
                await Shell.Current.GoToAsync($"schedule");
            }
            else
            {
                await DisplayAlert("Error", edit.Message, "OK");
            }
        }
    }
}