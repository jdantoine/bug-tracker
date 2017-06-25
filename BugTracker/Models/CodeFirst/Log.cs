using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    /// <summary>
    /// When creating a log object, you must provide values for WhoModified, DateModified, oldValue, newValue, and role/property. Ticket and Project can have log object
    /// </summary>
    public class Log
    {
        public int Id { get; set; }
        public int? TicketId { get; set; }
        public int? ProjectId { get; set; }
        public string ModifiedById { get; set; }
        public DateTimeOffset Modified { get; set; }
        public string Property { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual Project Project { get; set; }
        public virtual ApplicationUser ModifiedBy { get; set; }
    }
}