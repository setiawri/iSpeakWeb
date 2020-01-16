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
    public partial class SchedulePage : ContentPage
    {
        private readonly Services.WebApiService webApiService = new Services.WebApiService();

        public SchedulePage()
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
            var list = await webApiService.ScheduleApi(Settings.LoginUser);
            lv.ItemsSource = list;
            DataStore.Schedules = list;

            bool isTutor = false;
            List<TutorSchedule> ts = new List<TutorSchedule>();
            foreach (var item in list)
            {
                //tutor (not student)
                if (item.Role == "tutor")
                {
                    isTutor = true;
                    var result = await webApiService.TutorScheduleApi(item.Schedule_Id.ToString());
                    ts.Add(result);
                }
            }
            DataStore.TutorSchedules = ts;

            ToolbarItems.Clear();
            if (isTutor)
            {
                ToolbarItem toolbarItem = new ToolbarItem("+ ADD", "", () => HandleToolbarItemAsync());
                async Task HandleToolbarItemAsync()
                {
                    await Shell.Current.GoToAsync($"scheduleadd");
                }
                ToolbarItems.Add(toolbarItem);
            }
        }

        //private async void Added_Clicked(object sender, EventArgs e)
        //{
        //    await Shell.Current.GoToAsync($"scheduleadd");
        //}

        private async void lv_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListView lv = (ListView)sender;
            Schedule schedule = (Schedule)lv.SelectedItem;
            if (schedule.Role == "tutor")
            {
                await Shell.Current.GoToAsync($"scheduleedit?name={schedule.Schedule_Id}");
            }
        }
    }
}