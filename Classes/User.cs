using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockIn_Desktop
{
    public class User
    {
        public enum UserType
        {
            USER,
            ADMIN
        }

        public User()
        {
            this.FirstName = null;
            this.LastName = null;
            this.Email = null;
            this.Phone = null;
            this.DOB = DateTime.Now;
            this.Type = UserType.USER;
        }

        [JsonConstructor]
        public User(string id, string FirstName, string LastName, string Email, string Phone, string DOB, int Type)
        {
            this._ID = id;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.Email = Email;
            this.Phone = Phone;
            this.DOB = DateTime.Parse(DOB, null, DateTimeStyles.RoundtripKind);
            this.Type = (UserType)Type;
        }

        public string _ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DOB { get; set; }
        public UserType Type { get; set; }
        [JsonIgnore]
        public string DOBString
        {
            get
            {
                return DOB.ToString("d");
            }
        }
    }
}
