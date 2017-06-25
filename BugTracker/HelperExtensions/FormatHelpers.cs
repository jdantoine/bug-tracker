using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class FormatHelpers
    {
        /// <summary>
        /// Any member that  call this method will provide a date. This method will convert the date to string and format it to MM/dd/yyyy.
        /// </summary>
        public static string FormatDateTimeOffsetCondensed(this DateTimeOffset? date)
        {
            string datestring;

            if (date != null)
                datestring = date.Value.ToString("MM/dd/yyyy");
            else
                datestring = "No date provided";

            return datestring;
        }

        public static string FormatDateTimeOffsetCondensed(this DateTimeOffset date)
        {
            string datestring;

            if (date != null)
                datestring = date.DateTime.ToString("MM/dd/yyyy");
            else
                datestring = "No date provided";

            return datestring;
        }

        /// <summary>
        /// Any member that  call this method will provide a date. This method will convert the date to string and format it to ddd, MMMM dd, yyyy (3L: 1st 3 ----- 4Letter: full day/mo/yr .
        /// </summary>
        public static string FormatDateTimeOffset(this DateTimeOffset? date)
        {
            string datestring;

            if (date != null)
                datestring = date.Value.ToString("ddd, MMMM dd, yyyy");
            else
                datestring = "No date provided";

            return datestring;
        }

        /// <summary>
        /// Any member that  call this method will provide a date. This method will convert the date to string and format it to ddd, MMMM dd, yyyy (3L: 1st 3 ----- 4Letter: full day/mo/yr .
        /// </summary>
        public static string FormatDateTimeOffset(this DateTimeOffset date)
        {
            string datestring;

            if (date != null)
                datestring = date.DateTime.ToString("ddd, MMMM dd, yyyy");
            else
                datestring = "No date provided";

            return datestring;
        }
    }
}