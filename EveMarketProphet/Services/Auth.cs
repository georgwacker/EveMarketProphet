using eZet.EveLib.EveAuthModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EveMarketProphet.Properties;

namespace EveMarketProphet.Services
{
    public class Auth
    {
        public static Auth Instance { get; } = new Auth();

        private EveAuth EveAuthInstance { get; } = new EveAuth();
        private AuthResponse Current { get; set; }
        private DateTime ExpiresAt { get; set; }

        private Auth()
        {
            TryAuthenticationRefresh();
        }

        private string GetEncodedKey()
        {
            if (string.IsNullOrEmpty(Authentication.Default.ClientId) ||
                string.IsNullOrEmpty(Authentication.Default.ClientSecret)) return null;

            return EveAuth.Encode(Authentication.Default.ClientId, Authentication.Default.ClientSecret);
        }

        public void TryAuthentication(string authCode)
        {
            if (string.IsNullOrEmpty(authCode)) return;

            var encodedKey = GetEncodedKey();
            if (string.IsNullOrEmpty(encodedKey)) return;

            try
            {
                var response = EveAuthInstance.AuthenticateAsync(encodedKey, authCode).Result;
                Current = response;
                ExpiresAt = DateTime.Now.AddSeconds(response.ExpiresIn);
                Authentication.Default.RefreshToken = response.RefreshToken;
                Settings.Default.PrivateCrest = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Authentication unsuccessfull");
                Console.WriteLine(e.Message);
            }
            
        }

        private string TryAuthenticationRefresh()
        {
            try
            {
                if (string.IsNullOrEmpty(Authentication.Default.RefreshToken)) return null;

                var encodedKey = GetEncodedKey();
                if (string.IsNullOrEmpty(encodedKey)) return null;

                var response = EveAuthInstance.RefreshAsync(encodedKey, Authentication.Default.RefreshToken).Result;
                Current = response;
                ExpiresAt = DateTime.Now.AddSeconds(response.ExpiresIn);
                return response.AccessToken;
            }
            catch (Exception e)
            {
                Console.WriteLine("Authentication refresh unsuccessfull");
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private string GetAccessToken()
        {
            if (Current == null) return null;

            return DateTime.Now < ExpiresAt ? Current.AccessToken : TryAuthenticationRefresh();
        }

        public string CreateAuthLink(string clientId)
        {
            if (string.IsNullOrEmpty(clientId)) return null;
            return EveAuthInstance.CreateAuthLink(clientId, "/", "default", "characterLocationRead remoteClientUI characterNavigationWrite");
        }

        public int GetLocation()
        {
            if (Current == null) return 0;
            var token = GetAccessToken();

            if (!string.IsNullOrEmpty(token))
            {
                var resp = EveAuthInstance.VerifyAsync(token).Result;
                if (resp == null) return 0;

                var charId = resp.CharacterId;

                var c = new HttpClient();
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = c.GetAsync($"https://crest-tq.eveonline.com/characters/{charId}/location/");
                var json = response.Result.Content.ReadAsStringAsync().Result;
                var obj = JObject.Parse(json);

                // Offline, return Jita 4/4
                if (!obj.HasValues) return 0;

                var solarSystemId = (int)obj.SelectToken("solarSystem.id");

                return solarSystemId;
            }


            return 0;
        }

        public async void OpenMarketWindow(int typeId)
        {
            if (Current == null) return;
            var token = GetAccessToken();

            if (string.IsNullOrEmpty(token)) return;

            var charId = EveAuthInstance.VerifyAsync(token).Result.CharacterId;

            var payload = new OpenWindow
            {
                type = new Type
                {
                    href = String.Format("https://crest-tq.eveonline.com/inventory/types/{0}/", typeId),
                    id = typeId
                }
            };

            var stringPayload = JsonConvert.SerializeObject(payload);
            var postData = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                await httpClient.PostAsync($"https://crest-tq.eveonline.com/characters/{charId}/ui/openwindow/marketdetails/", postData);

                // There is no response, if the action is successful the market details window will open.
            }

        }

        public async void SetWaypoints(int systemId, bool clearOtherWaypoints)
        {
            if (Current == null) return;
            var token = GetAccessToken();

            if (string.IsNullOrEmpty(token)) return;

            var charId = EveAuthInstance.VerifyAsync(token).Result.CharacterId;

            var payload = new Waypoint
            {
                ClearOtherWaypoints = clearOtherWaypoints,
                First = false,
                SolarSystem = new Type
                {
                    href = String.Format("https://crest-tq.eveonline.com/solarsystems/{0}/", systemId),
                    id = systemId
                }
            };

            var stringPayload = JsonConvert.SerializeObject(payload);
            var postData = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                await httpClient.PostAsync($"https://crest-tq.eveonline.com/characters/{charId}/ui/autopilot/waypoints/", postData);

                // There is no response, if the action is successful the market details window will open.
            }
        }

        private class OpenWindow
        {
            [JsonProperty("type")]
            public Type type { get; set; }
        }

        private class Type
        {
            [JsonProperty("href")]
            public string href { get; set; }

            [JsonProperty("id")]
            public int id { get; set; }
        }

        private class Waypoint
        {
            [JsonProperty("clearOtherWaypoints")]
            public bool ClearOtherWaypoints { get; set; }

            [JsonProperty("first")]
            public bool First { get; set; }

            [JsonProperty("solarSystem")]
            public Type SolarSystem { get; set; }
        }
    }
}
