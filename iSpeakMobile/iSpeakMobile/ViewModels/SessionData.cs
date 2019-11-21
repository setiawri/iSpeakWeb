using iSpeakMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpeakMobile.ViewModels
{
    public class SessionData
    {
        private readonly Services.WebApiService webApiService = new Services.WebApiService();

        private readonly List<string> _data = new List<string>
        {
            "Harry","Agung","Sulistyo","Rhyza","Delfino","Tantri","Aswarawati","Yupi","Anggoro","Putro","Trie","Rizka","Novianty","Putri","Apri","Aswara","Aga","Aswanto"
        };

        public async Task<List<string>> GetItemAsync(int pageIndex, int pageSize)
        {
            await Task.Delay(2000);
            return _data.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public async Task<List<Session>> GetSessionAsync(int pageIndex, int pageSize)
        {
            await Task.Delay(2000);
            var data = await webApiService.SessionApi(Settings.LoginUser);
            DataStore.Sessions = data;
            return data.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }
    }
}