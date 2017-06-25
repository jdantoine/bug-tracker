using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class NotificationHelpers
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Any member that call this method will provide a list of ApplicationUsers. This method will convert those users into string.
        /// </summary>
        public static string ConvertUsersToNamesString(this ICollection<ApplicationUser> users)
        {
            string nameString = "";
            foreach (var user in users)
                nameString = nameString + user.FullName + "...";

            return nameString.Remove(nameString.Length-3);
        }

        /// <summary>
        /// Any member that call this method will provide a ticket's id, a notification type-object (ex. created, reassigned), list of /users/receipients, a message. This method will convert the users/receipient to string, institiate a notification object (which include a date,ticketId,message,receipients,notificationType)
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="type"></param>
        /// <param name="recipients"></param>
        /// <param name="msgBody"></param>
        /// <returns></returns>
        public static Notification CreateTicketNotification(this int ticketId, NotificationType type, List<ApplicationUser> recipients, string msgBody)
        {
            var users = recipients.ConvertUsersToNamesString();

            Notification notification = new Notification()
            {
                TicketId = ticketId,
                TypeId = type.Id,
                Recipients = users,
                SendDate = DateTimeOffset.Now,
                Message = msgBody
            };

            return notification;
        }

        //public static Notification CreateTicketNotification(this int ticketId, string type, List<ApplicationUser> recipients, string msgBody)
        //{
        //    var typeId = db.NotificationTypes.FirstOrDefault(n => n.Name == type).Id;
        //    var users = new List<string>();
        //    foreach (var user in recipients)
        //        users.Add(user.FullName);

        //    Notification notification = new Notification()
        //    {
        //        TicketId = ticketId,
        //        TypeId = typeId,
        //        Recipients = users,
        //        SendDate = DateTimeOffset.Now,
        //        Message = msgBody
        //    };

        //    return notification;          
        //}

        //public static Notification CreateProjectNotification(this int projectId, string type, IEnumerable<ApplicationUser> recipients, string msgBody)
        //{
        //    var typeId = db.NotificationTypes.FirstOrDefault(n => n.Name == type).Id;
        //    var users = new List<string>();
        //    foreach (var user in recipients)
        //        users.Add(user.FullName);

        //    Notification notification = new Notification()
        //    {
        //        ProjectId = projectId,
        //        TypeId = typeId,
        //        Recipients = users,
        //        SendDate = DateTimeOffset.Now,
        //        Message = msgBody
        //    };

        //    return notification;
        //}

        /// <summary>
        /// Any member that call this method will provide a project's id, a notification type-object (ex. created, reassigned), list of /users/receipients, a message. This method will convert the users/receipient to string, institiate a notification object (which include a date,projectId,message,receipients,notificationType)
        /// </summary>
        public static Notification CreateProjectNotification(this int projectId, NotificationType type, List<ApplicationUser> recipients, string msgBody)
        {
            var users = recipients.ConvertUsersToNamesString();

            Notification notification = new Notification()
            {
                ProjectId = projectId,
                TypeId = type.Id,
                SendDate = DateTimeOffset.Now,
                Message = msgBody,
                Recipients = users
            };

            return notification;
        }
    }
}