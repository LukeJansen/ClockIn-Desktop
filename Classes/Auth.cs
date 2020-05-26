using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace ClockIn_Desktop.Classes
{
    public class Auth
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        private PasswordVault vault;


        public Auth()
        {
            vault = new PasswordVault();
            var credentialList = vault.RetrieveAll();

            Debug.WriteLine(credentialList.Count);

            if (credentialList.Count == 1)
            {
                PasswordCredential credential = credentialList[0];
                credential.RetrievePassword();
                UserID = credential.UserName;
                RefreshToken = credential.Password;
            }
        }

        public async Task<bool> GenerateAccessToken()
        {
            if (RefreshToken != null)
            {
                string json = $"{{ \"RefreshToken\":\"{RefreshToken}\",\"UserID\":\"{UserID}\"}}";
                var response = await Utility.SendToApi(Utility.AUTHURL + "token/refresh", json, "POST", "");
                if (response["message"].Equals("Access Token Generated"))
                {
                    AccessToken = response["AccessToken"];
                    return true;
                }
                else if (response["message"].Equals("Refresh Token Not Valid"))
                {
                    vault.Remove(new PasswordCredential("ClockIn", UserID, RefreshToken));
                    RefreshToken = null;
                    AccessToken = null;
                    UserID = null;

                    return false;
                }
                else
                {
                    Debug.WriteLine(response["message"]);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public async void LogOut()
        {
            vault.Remove(new PasswordCredential("ClockIn", UserID, RefreshToken));
            await Utility.SendToApi(Utility.AUTHURL + "logout", $"{{ \"UserID\": \"{UserID}\",\"RefreshToken\":\"{RefreshToken}\" }}", "POST", "");
            RefreshToken = null;
            AccessToken = null;
            UserID = null;
        }
    }
}
