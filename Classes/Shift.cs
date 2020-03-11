using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClockIn_Desktop
{
    public class Shift
    {
        public Shift()
        {
            this.Location = null;
            this.Role = null;
            this.Start = DateTime.Now;
            this.Finish = DateTime.Now;
            this.Users = new List<User>();
        }

        [JsonConstructor]
        public Shift(string id, string location, string role, string start, string finish, List<User> users)
        {
            this._ID = id;
            this.Location = location;
            this.Role = role;
            this.Start = DateTime.Parse(start, null, DateTimeStyles.RoundtripKind);
            this.Finish = DateTime.Parse(finish, null, DateTimeStyles.RoundtripKind);
            this.Users = users;
        }

        public string _ID { get; set; }
        public string Location { get; set; }
        public string Role { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        [JsonIgnore]
        public string DateString { get
            {
                return Start.ToString("d");
            }
        }
        [JsonIgnore]
        public string StartString { get
            {
                return Start.ToString("h:mm tt");
            } 
        }
        [JsonIgnore]
        public string FinishString
        {
            get
            {
                return Finish.ToString("h:mm tt");
            }
        }
        public List<User> Users { get; set; }
    }
}
