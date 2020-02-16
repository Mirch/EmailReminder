using Blazored.LocalStorage;
using EmailReminder.Shared.Models;
﻿using EmailReminder.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EmailReminder.WebApp.Services
{
    public class EmailReminderService
    {
        private readonly HttpClient _client;
        private readonly ILocalStorageService _localStorage;

        public EmailReminderService(
            HttpClient client,
            ILocalStorageService localStorage)
        {
            _client = client;
            _localStorage = localStorage;
        }

        public async Task<Reminder> CreateReminderAsync(Reminder reminder)
        {
            var content = new StringContent(JsonConvert.SerializeObject(reminder), Encoding.UTF8, "application/json");

            var result = await _client.PostAsync($"/api/reminders", content);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Could not create reminder.");
            }

            var resultContent = await result.Content.ReadAsStringAsync();
            var resultReminder = JsonConvert.DeserializeObject<Reminder>(resultContent);

            return resultReminder;
        }
      
        public async Task<IEnumerable<Reminder>> GetRemindersAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var result = await _client.GetAsync($"/api/reminders/all");

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Could not retrieve reminders.");
            }

            var resultContent = await result.Content.ReadAsStringAsync();
            var resultReminders = JsonConvert.DeserializeObject<IEnumerable<Reminder>>(resultContent);

            return resultReminders;
        }
        public async Task<bool> ConfirmUserAsync(EmailConfirmation confirmation)
        {
            var content = new StringContent(JsonConvert.SerializeObject(confirmation), Encoding.UTF8, "application/json");

            var result = await _client.PostAsync($"/api/authentication/confirm", content);

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(EmailConfirmation confirmation)
        {
            var content = new StringContent(JsonConvert.SerializeObject(confirmation), Encoding.UTF8, "application/json");

            var result = await _client.PostAsync($"/api/authentication/login", content);

            if (!result.IsSuccessStatusCode)
            {
                return false;
            }

            var resultContent = await result.Content.ReadAsStringAsync();
            
            await _localStorage.SetItemAsync("token", resultContent);

            return true;
        }

        public async Task<bool> GetLoginToken(string email)
        {
            string emailParameter = HttpUtility.UrlEncode(email);

            var result = await _client.GetAsync($"/api/authentication/loginToken?email={emailParameter}");

            return result.IsSuccessStatusCode;
        }
    }
}
