using iSpeakMobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace iSpeakMobile.Services
{
    public class WebApiService
    {
        private readonly string baseurl = "http://ispeakgroup.com/";
        //private readonly string baseurl = "http://devkredit.amartahonda.com/";

        public async Task<TokenIdentity> LoginIdentityApi(string username, string password)
        {
            using (var client = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("grant_type", "password")
                });

                var request = await client.PostAsync(baseurl + "Token", formContent);
                var content = await request.Content.ReadAsStringAsync();
                TokenIdentity model = JsonConvert.DeserializeObject<TokenIdentity>(content);
                return model;
            }
        }

        public async Task<User> UserLoginApi(string username)
        {
            using (var client = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username)
                });

                var request = await client.PostAsync(baseurl + "api/userlogin", formContent);
                var content = await request.Content.ReadAsStringAsync();
                User response = JsonConvert.DeserializeObject<User>(content);
                return response;
            }
        }

        public async Task<List<Invoice>> InvoiceApi(string username)
        {
            using (var client = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username)
                });

                var request = await client.PostAsync(baseurl + "api/invoice", formContent);
                var content = await request.Content.ReadAsStringAsync();
                List<Invoice> response = JsonConvert.DeserializeObject<List<Invoice>>(content);
                return response;
            }
        }

        public async Task<List<Payment>> PaymentApi(string username)
        {
            using (var client = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username)
                });

                var request = await client.PostAsync(baseurl + "api/payment", formContent);
                var content = await request.Content.ReadAsStringAsync();
                List<Payment> response = JsonConvert.DeserializeObject<List<Payment>>(content);
                return response;
            }
        }

        public async Task<List<Session>> SessionApi(string username)
        {
            using (var client = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username)
                });

                var request = await client.PostAsync(baseurl + "api/session", formContent);
                var content = await request.Content.ReadAsStringAsync();
                List<Session> response = JsonConvert.DeserializeObject<List<Session>>(content);
                List<Session> response_edit = new List<Session>();
                foreach (var r in response)
                {
                    response_edit.Add(new Session
                    {
                        SaleInvoiceItems_Id = r.SaleInvoiceItems_Id,
                        Date = r.Date,
                        Lesson = r.Lesson,
                        Hour = r.Hour,
                        Tutor = r.Tutor,
                        Review = r.Review.Replace("\n", Environment.NewLine)
                    });
                }
                return response_edit;
            }
        }
    }
}
