using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class EmailViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string FromName { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string FromEmail { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Body { get; set; }
    }
}