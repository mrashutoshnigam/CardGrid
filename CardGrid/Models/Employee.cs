using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardGrid.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public System.DateTime HireDate { get; set; }
        public string Gender { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public string PhotoUrl
        {
            get
            {
                return @"\Photos\" + Id + ".jpg";
            }
        }
    }
}