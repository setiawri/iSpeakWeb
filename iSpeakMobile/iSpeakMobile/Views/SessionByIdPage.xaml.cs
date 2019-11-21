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
    public partial class SessionByIdPage : ContentPage
    {
        public SessionByIdPage()
        {
            InitializeComponent();
            BindingContext = this;
            cv.ItemsSource = DataStore.Sessions;
            Title = DataStore.Sessions.Select(x => x.Lesson).FirstOrDefault();
        }
    }
}