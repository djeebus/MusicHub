using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace MusicHub.GooglePlay
{
    public class GoogleMusicClient
    {
        private Dictionary<string, string> _authCreds;
        private System.Net.CookieContainer _authCookies = new CookieContainer();
        public bool IsLoggedOn
        {
            get
            {
                return _authCreds != null;
            }
        }

        public void LogOn(string username, string password)
        {
            if (this.IsLoggedOn)
                throw new Exception("Already logged in");

            AuthenticateToGoogle(username, password);

            var token = AuthenticateToMusicManager();

            GetCookies(token);
        }

        private void GetCookies(string token)
        {
            var data = new NameValueCollection
            {
                { "auth", token },
                { "service", "sj" },
                { "continue", "https://play.google.com/music/listen?u=0&hl=en" },
                { "source", "jumper" },
            };

            var url = string.Format("{0}?{1}",
                "https://www.google.com/accounts/TokenAuth",
                UrlEncodeFormData(data));

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = _authCookies;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
            }
        }

        private string AuthenticateToMusicManager()
        {
            var data = new NameValueCollection {
                { "SID", _authCreds["SID"] },
                { "LSID", _authCreds["LSID"] },
                { "service", "gaia" },
            };

            var request = (HttpWebRequest)WebRequest.Create(
                "https://www.google.com/accounts/IssueAuthToken");

            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            request.Method = "POST";
            request.UserAgent = "Music Manager (1, 0, 24, 7712 - Windows)";

            return MakePost(request, data);
        }

        private string MakePost(HttpWebRequest request, NameValueCollection data)
        {
            var urlEncodedFormData = UrlEncodeFormData(data);

            var payload = Encoding.UTF8.GetBytes(urlEncodedFormData);

            request.ContentLength = payload.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(payload, 0, payload.Length);
            }

            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd().TrimEnd();
                }
            }
        }

        private static string UrlEncodeFormData(NameValueCollection data)
        {
            var urlEncodedFormData = string.Join("&",
                from key in data.AllKeys
                select string.Format("{0}={1}",
                    System.Web.HttpUtility.UrlEncode(key),
                    System.Web.HttpUtility.UrlEncode(data[key])));
            return urlEncodedFormData;
        }

        private void AuthenticateToGoogle(string username, string password)
        {
            var data = new NameValueCollection
            {
                { "Email", username },
                { "Passwd", password },
                { "accountType", "GOOGLE" },
                { "service", "sj" },
            };

            byte[] result;
            try
            {
                using (var client = new WebClient())
                {
                    result = client.UploadValues(
                        new Uri("https://www.google.com/accounts/ClientLogin"),
                        data);
                }
            }
            catch (WebException webEx)
            {
                var httpResponse = webEx.Response as HttpWebResponse;
                if (httpResponse == null)
                    throw new ArgumentNullException("httpResponse", webEx);

                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.Forbidden:
                        // response would equal Error=BadAuthentication
                        throw new InvalidCredentialsException();

                    default:
                        throw;
                }
            }

            string text = Encoding.UTF8.GetString(result);

            _authCreds = CreateDictionary(text);
        }

        private static Dictionary<string, string> CreateDictionary(string text)
        {
            var parts = from p in text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        let s = p.Split('=')
                        select new
                        {
                            key = s[0],
                            value = s[1]
                        };

            return parts.ToDictionary(
                kvp => kvp.key,
                kvp => kvp.value);
        }

        public Song[] GetLibrary(ref string continuationToken)
        {
            object jsonRequest;
            if (string.IsNullOrWhiteSpace(continuationToken))
                jsonRequest = new object();
            else
                jsonRequest = new { continuationToken = continuationToken };

            var loadResponse = GetJsonFromWebCall<LoadAllTracksResponse>(
                "loadalltracks",
                jsonRequest);

            continuationToken = loadResponse.ContinuationToken;

            return loadResponse.Songs;
        }

        private T GetJsonFromWebCall<T>(string callName, object payload)        
        {
            var token = GetXtCookie();

            var s = string.Format("{0}{2}?u=0&xt={1}",
                "https://play.google.com/music/services/",
                token,
                callName);

            return GetJsonFromWebCall<T>(
                new Uri(s),
                payload);
        }

        private T GetJsonFromWebCall<T>(Uri url, object payload)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.8.1.6) Gecko/20061201 Firefox/2.0.0.6 (Ubuntu-feisty)";
            request.CookieContainer = _authCookies;

            if (payload != null)
            {
                string body = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                body = System.Web.HttpUtility.UrlEncode(body);
                body = "json=" + body;

                var data = Encoding.UTF8.GetBytes(body);

                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

                using (var writer = request.GetRequestStream())
                    writer.Write(data, 0, data.Length);
            }

            string json;

            using (var response = request.GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream()))
                {
                    json = stream.ReadToEnd();
                }
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        private string GetXtCookie()
        {
            var result = _authCookies.GetCookies(
                new Uri("https://play.google.com/music/listen/"));
            if (result == null)
                throw new ArgumentNullException("result");

            var cookie = result["xt"];
            if (cookie == null)
                throw new ArgumentNullException("xt");

            return cookie.Value;
        }

        public Uri GetSongUrl(string songId)
        {

            var url = string.Format(
                "https://play.google.com/music/play?u=0&pt=e&songid={0}", 
                HttpUtility.UrlEncode(songId));

            var response = GetJsonFromWebCall<SongUrl>(new Uri(url), null);

            return new Uri(response.Url);
        }
    }
}
