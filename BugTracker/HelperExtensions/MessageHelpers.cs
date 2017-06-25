﻿using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class MessageHelpers
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Any member that call this method will provide a ticket, and recipients/users. This method will create a message object (wich include a destination/recipients address) and attach it to the ticket and users.
        /// </summary>
        public static IEnumerable<IdentityMessage> CreateTicketSubmittedMessage(this Ticket ticket, IEnumerable<ApplicationUser> recipients)
        {
            var _ticket = db.Tickets.Find(ticket.Id);
            var msgList = new List<IdentityMessage>(); //list of message for all recipients

            foreach (var user in recipients)
            {
                var msg = new IdentityMessage();//create message object
                msg.Destination = user.Email;
                msg.Body = "A new ticket has been submitted by " + _ticket.Submitter.FullName + ". Please review the ticket details to determine validity, and to assign a Project Manager and Developer. <br/><br/>Ticket: " + _ticket.Name + "<br/> <br/>Submitted: " + _ticket.Submitted.FormatDateTimeOffset() + "<br/><br/>Description: " + _ticket.Description != null ? _ticket.Description : "";

                msgList.Add(msg);
            }


            return msgList;
        }

        /// <summary>
        /// Any member that call this method will provide a ticket, a project, and a user. This method will create a message object (wich include a destination/recipients address, and a body ~ will include PM's name) and attach it to the ticket and user.
        /// </summary>
        public static IdentityMessage CreateAssignedToTicketMessage(this Ticket ticket, Project project, ApplicationUser user)
        {
            var _ticket = db.Tickets.Find(ticket.Id);
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "A new ticket has been assigned to you by " + manager.FullName + ". Ticket details are below. <br/><br/> Project: " + project.Name + "<br/> Project Due Date: " + project.Deadline  + "<br/> Ticket Name: " + _ticket.Name != null ? _ticket.Name : "" + "<br/> Description: " + _ticket.Description != null ? _ticket.Description : "" + "<br/> Submitter: " + _ticket.Submitter.FullName + "<br/> Priority: " + _ticket.Priority.Name != null ? _ticket.Priority.Name : "" + "<br/> Action: " + _ticket.Action.Name + "<br/> Phase: " + _ticket.Phase.Name != null ? ticket.Phase.Name : "" + "<br/><br/>If you have questions or cannot complete this ticket, please contact " + manager.FirstName + " at " + manager.Email + ".";
            msg.Subject = "New ticket assignment on project " + project.Name;

            return msg;
        }

        /// <summary>
        /// Any member that call this method will provide a project, and a user. This method will create a message object (wich include a destination and message) and attach it to the project and a user.
        /// </summary>
        public static IdentityMessage CreateAssignedToProjectMessage(this Project project, ApplicationUser user)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
           msg.Destination = user.Email;
           msg.Body = "You have recently been assigned to a new project. Project details are below. <br/><br/> Project: " + project.Name + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions, please contact " + manager.FirstName + " at " + manager.Email + ".";
           msg.Subject = "New project assignment: " + project.Name;

            return msg;
        }

        /// <summary>
        /// Any member that call this method will provide a project, and a user. This method will create a message object (wich include a destination and message) and attach it to the project.
        /// </summary>
        public static IdentityMessage CreateRemovedFromProjectMessage(this Project project, ApplicationUser user)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
           msg.Destination = user.Email;
           msg.Body = "You have recently been removed from a project. Project details are below. <br/><br/> Project: " + project.Name + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions, please contact " + manager.FirstName + " at " + manager.Email + ".";
           msg.Subject = "Removed from Project: " + project.Name;

            return msg;
        }

        /// <summary>
        /// Any member that call this method will provide a project, and recipients/users. This method will create a message object (wich include a destination and message) and attach it to the project.
        /// </summary>
        public static IEnumerable<IdentityMessage> CreateProjectDeadlineChangedMessage(this Project project, IEnumerable<ApplicationUser> users)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msgList = new List<IdentityMessage>();
            foreach (var user in users)
            {
                var msg = new IdentityMessage();
                msg.Destination = user.Email;
                msg.Body = "The deadline for your project " + project.Name + " has been changed. The new deadline is " + project.Deadline.FormatDateTimeOffset() + ".";
                msg.Subject = "Deadline change for " + project.Name;

                msgList.Add(msg);
            }
            return msgList;
        }

        /// <summary>
        /// Any member that call this method will provide a project, and developers/users. This method will create a message object (wich include a destination and message) and attach it to the project.
        /// </summary>
        public static IEnumerable<IdentityMessage> CreateNewProjectManagerMessage(this Project project, IEnumerable<ApplicationUser> developers)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msgList = new List<IdentityMessage>();
            foreach(var developer in developers)
            {
                var msg = new IdentityMessage();
                msg.Destination = developer.Email;
                msg.Body = "A new project manager has recently been assigned to one of your projects. Details are below. <br/><br/> Project: " + project.Name + "<br/> New Project Manager: " + manager.FullName + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions, please contact " + manager.FirstName + " at " + manager.Email + ".";
                msg.Subject = "New Project Manager on project " + project.Name;

                msgList.Add(msg);
            }
            return msgList;
        }

        /// <summary>
        ///  Any member that call this method will provide aticket, a project, and a user. This method will create a message object (wich include a destination and message) and attach it to the ticket.
        /// </summary>
        public static IdentityMessage CreateTicketReassignedMessage(this Ticket ticket, Project project, ApplicationUser user)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "One of your tickets has been reassigned to a new developer.  Details are below. <br/><br/> Name: " + ticket.Name + "<br/> Project: " + project.Name + "<br/><br/> If you have questions about this reassignment, please contact the Project Manager, " + manager.FullName + " at " + manager.Email + ".";
            msg.Subject = "Ticket: " + ticket.Name + " has been reassigned.";

            return msg;
        }

        /// <summary>
        ///  Any member that call this method will provide a ticket, and a user. This method will create a message object (wich include a destination and message) and attach it to the ticket.
        /// </summary>
        public static IdentityMessage CreateTicketModifiedMessage(this Ticket ticket, ApplicationUser user)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == ticket.ProjectId);
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "The ticket " + ticket.Name + " in project " + project.Name + " has been modified. Please review the changelog to see if any action is necessary. View the ChangeLog <a href=\"https://jantoine-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id + "\">here.</a>";
            msg.Subject = "Ticket: " + ticket.Name + " has been modified.";

            return msg;
        }

        /// <summary>
        ///  Any member that call this method will provide a ticket, a user, and a request. This method will create a message object (wich include a destination, message, and requestExplanation) and attach it to the ticket.
        /// </summary>
        public static IdentityMessage CreateReassignmentRequestedMessage(this Ticket ticket, ApplicationUser user, string request)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == ticket.ProjectId);
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
            msg.Destination = manager.Email;
            msg.Body = "The developer " + user.FullName + " for ticket " + ticket.Name + " has requested that the ticket be reassigned. Please review the request below and either fulfill the request or contact the developer. If the developer indicates that the ticket is ready to advance to the next stage of development, review current work for errors before reassigning.<br/><br/> Request: " + request + "<br/><br/>View ticket details <a href=\"https://jantoine-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id + "\">here</a>.";
            msg.Subject = "Reassignment requested for ticket " + ticket.Name;

            return msg;
        }

        /// <summary>
        ///  Any member that call this method will provide a ticket, a user, and a request. This method will create a message object (wich include a destination and message) and attach it to the ticket. 
        /// </summary>
        public static IEnumerable<IdentityMessage> CreateTicketResolvedMessage(this Ticket ticket, IEnumerable<ApplicationUser> recipients)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == ticket.ProjectId);
            var manager = db.Users.Find(project.ProjectManagerId);
            var msgList = new List<IdentityMessage>();
            foreach (var recipient in recipients)
            {
                var msg = new IdentityMessage();
                msg.Destination = recipient.Email;
                msg.Body = "One of your tickets has been reassigned to a new developer.  Details are below. <br/><br/> Name: " + ticket.Name + "<br/> Project: " + project.Name + "<br/><br/> If you have questions about this reassignment, please contact the Project Manager, " + manager.FullName + " at " + manager.Email + ".";
                msg.Subject = "Ticket: " + ticket.Name + " has been reassigned.";

                msgList.Add(msg);
            }
            return msgList;
        }

        //public static IdentityMessage CreateTicketIsExplosiveMessage(this Ticket ticket, ApplicationUser developer, ApplicationUser projectManager)
        //{
        //    var msg = new IdentityMessage();
        //    return msg;
        //}

        //public static IdentityMessage CreateUserRolesModifiedMessage(this ApplicationUser user)
        //{
        //    var msg = new IdentityMessage();
        //    return msg;
        //}

    }
}