using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Carwash.Models.ViewModel
{
    public class AppointmentUserViewModel
    {
        public int appointment_id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime date { get; set; }
        public System.TimeSpan time { get; set; }
        public string site { get; set; }
        public int phone { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string service { get; set; }
    }
}