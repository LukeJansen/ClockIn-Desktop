using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Windows.Web.Http;
using Windows.Storage.Streams;

namespace ClockIn_Desktop.Classes
{
    public class Utility
    {
        public static List<Shift> LoadShifts(string url)
        {
            string jsonData;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                jsonData = reader.ReadToEnd();
            }

            List<Shift> shiftList = JsonConvert.DeserializeObject<List<Shift>>(jsonData);

            return shiftList;
        }

        public static async void UpdateShift(string url, string json)
        {
            try
            {
                // Construct the HttpClient and Uri. This endpoint is for test purposes only.
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(url);

                // Construct the JSON to post.
                HttpStringContent content = new HttpStringContent(json, UnicodeEncoding.Utf8, "application/json");

                // Post the JSON and wait for a response.
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                    uri,
                    content);

                // Make sure the post succeeded, and write out the response.
                httpResponseMessage.EnsureSuccessStatusCode();
                var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                Debug.WriteLine(httpResponseBody);
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine(ex);
            }
        }

    }
}
