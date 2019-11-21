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
    public partial class SessionDetailsPage : ContentPage
    {
        public string Name
        {
            set
            {
                BindingContext = DataStore.Sessions.Where(x => x.Date == Uri.UnescapeDataString(value)).FirstOrDefault();
            }
        }

        public SessionDetailsPage()
        {
            InitializeComponent();
        }
    }
}