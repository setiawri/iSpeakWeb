using iSpeakMobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Extended;

namespace iSpeakMobile.ViewModels
{
    public class SessionViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private const int PageSize = 10;
        readonly SessionData _sessionData = new SessionData();

        public InfiniteScrollCollection<Session> Items { get; set; }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public SessionViewModel()
        {
            Items = new InfiniteScrollCollection<Session>
            {
                OnLoadMore = async () =>
                {
                    IsBusy = true;
                    var page = Items.Count / PageSize;
                    var items = await _sessionData.GetSessionAsync(page, PageSize);
                    IsBusy = false;
                    return items;
                },
                OnCanLoadMore = () =>
                {
                    return Items.Count < DataStore.Sessions.Count;
                }
            };

            DownloadDataAsync();
        }

        private async Task DownloadDataAsync()
        {
            var items = await _sessionData.GetSessionAsync(0, PageSize);
            Items.AddRange(items);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
