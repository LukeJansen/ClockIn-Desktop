using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.UI.Popups;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace ClockIn_Desktop.Classes
{
    public class Utility
    {
        public const string APIURL = "https://api.clockin.uk/";
        public const string AUTHURL = "https://auth.clockin.uk/";
        //public const string APIURL = "http://localhost:3000/";
        //public const string AUTHURL = "http://localhost:4000/";

        public async static Task<List<Shift>> LoadShifts(string url, Auth auth)
        {
            string jsonData;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Headers.Add("authorization", "Bearer " + auth.AccessToken);
                request.Timeout = 5000;

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    jsonData = await reader.ReadToEndAsync();
                }

                List<Shift> shiftList = JsonConvert.DeserializeObject<List<Shift>>(jsonData, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                });

                if (shiftList != null)
                {
                    return shiftList;
                }
                else
                {
                    await Utility.ShowDialog("Error", "Server did not respond with data. Please contact ClockIn.");
                    return new List<Shift>();
                }
            }
            catch (WebException e)
            {
                Dictionary<string, string> response;
                using (Stream stream = e.Response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    response = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
                }
                if (response["message"].Equals("jwt expired"))
                {
                    await auth.GenerateAccessToken();
                    Debug.WriteLine("JWT Expired Returning Null!" + DateTime.Now.ToString());
                    return new List<Shift>();
                }
                else
                {
                    await Utility.ShowDialog("Error", "Error ocurred, please report to ClockIn: " + response["message"]);
                    return new List<Shift>();
                }
            }
        }

        public async static Task<List<User>> LoadUsers(string url, Auth auth)
        {
            string jsonData;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Headers.Add("authorization", "Bearer " + auth.AccessToken);
                request.Timeout = 5000;

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    jsonData = await reader.ReadToEndAsync();
                }

                Debug.WriteLine(jsonData);

                List<User> userList = JsonConvert.DeserializeObject<List<User>>(jsonData, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                });

                if (userList != null)
                {
                    return userList;
                }
                else
                {
                    await Utility.ShowDialog("Error", "Server did not respond with data. Please contact ClockIn.");
                    return new List<User>();
                }
            }
            catch (WebException e)
            {
                Dictionary<string, string> response;
                using (Stream stream = e.Response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    response = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
                }
                if (response["message"].Equals("jwt expired"))
                {
                    await auth.GenerateAccessToken();
                    Debug.WriteLine("JWT Expired Returning Null!" + DateTime.Now.ToString());
                    return new List<User>();
                }
                else
                {
                    await Utility.ShowDialog("Error", "Error ocurred, please report to ClockIn: " + response["message"]);
                    return new List<User>();
                }
            }
        }

        public static async Task<Dictionary<String, String>> SendToApi(string url, string json, string method, string accessToken)
        {
            Debug.WriteLine(json);

            try
            {
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(url);

                HttpStringContent content = new HttpStringContent(json, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", accessToken);

                HttpResponseMessage message = new HttpResponseMessage();
                switch (method)
                {
                    case "GET":
                        message = await httpClient.GetAsync(uri);
                        break;
                    case "POST":
                        message = await httpClient.PostAsync(uri,content);
                        break;
                    case "DELETE":
                        message = await httpClient.DeleteAsync(uri);
                        break;
                }
                
                var httpResponseBody = await message.Content.ReadAsStringAsync();
                Debug.WriteLine(httpResponseBody);
                return JsonConvert.DeserializeObject<Dictionary<string,string>>(httpResponseBody);
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                var response = new Dictionary<string, string>();
                response["message"] = ex.Message;
                return response;
            }
        }

        public static async Task<ContentDialogResult> ShowDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                CloseButtonText = "Ok"
            };

            return await dialog.ShowAsync();
        }

        public static async Task<bool> ShowPassword(string password)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "User Password",
                Content = $"Generated password for user is: {password}",
                PrimaryButtonText = "Copy Password",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                DataPackage package = new DataPackage();
                package.SetText(password);
                Clipboard.SetContent(package);
            }

            return true;
        }

    }
}
