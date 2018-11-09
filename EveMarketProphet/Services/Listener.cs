using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using Flurl;

namespace EveMarketProphet.Services
{
    public class Listener
    {
        public static Listener Instance { get; } = new Listener();

        private HttpListener listener;
        public bool active;

        private Listener()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(Auth.CallbackURL);
        }

        public void Stop()
        {
            listener.Stop();
            active = false;
        }

        public async void Async()
        {
            if (active) return;
            active = true;

            listener.Start();

            var authlink = Auth.Instance.CreateAuthLink();
            Process.Start(authlink);

            HttpListenerContext context = null;

            try
            {
                context = await listener.GetContextAsync();
            }
            catch (HttpListenerException ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }

            if (context == null) return;

            var request = context.Request;
            var response = context.Response;
            string responseString = "<HTML><BODY><p><b>EveMarketProphet<b></p>Authorization request successful</BODY></HTML>";

            var bytes = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            response.OutputStream.Close();
            response.Close();

            var url = new Url(request.Url.ToString());
            var code = url.QueryParams["code"].ToString();
            var state = url.QueryParams["state"].ToString();

            listener.Stop();

            Auth.Instance.RequestToken(code, state);
            active = false;
        }
    }
}
