using Newtonsoft.Json;
using System;
using System.Text;
using EveMarketProphet.Properties;
using Flurl;
using Flurl.Http;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Windows;
using Microsoft.CSharp.RuntimeBinder;

namespace EveMarketProphet.Services
{
    public class Auth
    {
        public static Auth Instance { get; } = new Auth();
        public static RNGCryptoServiceProvider Rand = new RNGCryptoServiceProvider();
        public static SHA256CryptoServiceProvider Sha = new SHA256CryptoServiceProvider();

        private AuthResponse Current { get; set; }
        private DateTime ExpiresAt { get; set; }

        public const string ClientID = "96a101c951eb47239caf8cc1fca0a9e7";
        public const string CallbackURL = "http://localhost:8989/callback/";

        private string CurrentChallengeBase { get; set; }
        private string CurrentStateSecret { get; set; }

        private Auth()
        {
            TryAuthenticationRefresh();
        }

        private string TryAuthenticationRefresh()
        {
            try
            {
                if (string.IsNullOrEmpty(Authentication.Default.RefreshToken)) return null;

                var req = "https://login.eveonline.com/v2/oauth/token";
                var result = req.WithHeaders(new
                {
                    Content_Type = "application/x-www-form-urlencoded",
                    Host = "login.eveonline.com"
                })
                .PostUrlEncodedAsync(new
                {
                    grant_type = "refresh_token",
                    refresh_token = Authentication.Default.RefreshToken,
                    client_id = ClientID
                }).ReceiveJson<AuthResponse>().Result;

                Current = result;
                ExpiresAt = DateTime.Now.AddSeconds(result.ExpiresIn);
                Authentication.Default.RefreshToken = result.RefreshToken;
                Authentication.Default.Save();

                return result.AccessToken;
            }
            catch (Exception e)
            {
                Console.WriteLine("Authentication refresh unsuccessful");
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private string GetAccessToken()
        {
            if (Current == null) return null;

            return DateTime.Now < ExpiresAt ? Current.AccessToken : TryAuthenticationRefresh();
        }

        public string GenerateRandomString(int length)
        {
            var buf = new byte[length];
            Rand.GetBytes(buf);
            return Base64UrlEncoder.Encode(buf);
        }

        public void RequestToken(string authCode, string state)
        {
            var req = "https://login.eveonline.com/v2/oauth/token";
            var result = req.WithHeaders(new
            {
                Content_Type = "application/x-www-form-urlencoded",
                Host = "login.eveonline.com"
            })
            .PostUrlEncodedAsync(new
            {
                grant_type = "authorization_code",
                code = authCode,
                client_id = ClientID,
                code_verifier = CurrentChallengeBase
            }).ReceiveJson<AuthResponse>().Result;

            Current = result;
            ExpiresAt = DateTime.Now.AddSeconds(result.ExpiresIn);
            Authentication.Default.RefreshToken = result.RefreshToken;
            Authentication.Default.CharName = GetCharacterName();
            Authentication.Default.Save();

            MessageBox.Show("Authentication successful", "EMP - ESI Authentication", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public string CreateAuthLink()
        {
            CurrentChallengeBase = GenerateRandomString(32);
            var hash = Sha.ComputeHash(Encoding.UTF8.GetBytes(CurrentChallengeBase));
            var challenge = Base64UrlEncoder.Encode(hash);
            CurrentStateSecret = GenerateRandomString(8);

            var auth = "https://login.eveonline.com/v2/oauth/authorize/"
                        .SetQueryParams(new
                        {
                            response_type = "code",
                            redirect_uri = CallbackURL,
                            client_id = ClientID,
                            scope = "esi-location.read_location.v1 esi-ui.open_window.v1 esi-ui.write_waypoint.v1",
                            code_challenge = challenge,
                            code_challenge_method = "S256",
                            state = CurrentStateSecret
                        });

            return auth;
        }

        public string GetCharacterName()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token)) return null;

            var url = "https://esi.tech.ccp.is/verify/";
            dynamic info = url.WithOAuthBearerToken(token).GetJsonAsync().Result;
            return info.CharacterName;
        }

        public long GetCharacterID()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token)) return 0;

            var url = "https://esi.tech.ccp.is/verify/";
            dynamic info = url.WithOAuthBearerToken(token).GetJsonAsync().Result;
            return info.CharacterID;
        }

        public int GetLocation()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token)) return 0;

            var character_id = GetCharacterID();
            if (character_id == 0) return 0;

            var url = $"https://esi.evetech.net/latest/characters/{character_id}/location/";

            dynamic location = url.WithOAuthBearerToken(token).GetJsonAsync().Result;
            var solarSystemId = 0;

            try
            {
                solarSystemId = (int)location.solar_system_id;
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return solarSystemId;
        }

        public async void OpenMarketWindow(int typeId)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token)) return;

            var url = $"https://esi.evetech.net/latest/ui/openwindow/marketdetails/";
            await url.WithOAuthBearerToken(token).SetQueryParam("type_id", typeId).PostAsync(null);
        }

        public async void SetWaypoints(int systemId, bool clearOtherWaypoints)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token)) return;

            var url = $"https://esi.evetech.net/latest/ui/autopilot/waypoint/";
            await url.WithOAuthBearerToken(token).SetQueryParams(new {
                add_to_beginning = false,
                clear_other_waypoints = clearOtherWaypoints,
                destination_id = systemId
            }).PostAsync(null);
        }

        private class AuthResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }
        }
    }
}
