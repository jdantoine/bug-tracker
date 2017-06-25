﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using BugTracker.HelperExtensions;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Dictionary<int, NotificationType> types = new Dictionary<int, NotificationType>()
        {
           { 1, new NotificationType {Id=1, Name="Ticket Submitted" } },
            { 2, new NotificationType {Id=2, Name="Ticket Assigned" } },
            { 3, new NotificationType {Id=3, Name="Ticket Resolved" } },
            { 4, new NotificationType {Id=4, Name="Reminder:Update Tickets" } },
            { 5, new NotificationType {Id=5, Name="Ticket Modified" } },
            { 6, new NotificationType {Id=6, Name="Ticket Reassigned" } },
            { 7, new NotificationType {Id=7, Name="Project Reassigned" } },
            { 8, new NotificationType {Id=8, Name="Project Assigned" } },
            { 9, new NotificationType {Id=9, Name="New Project Manager" } },
            {10, new NotificationType {Id=10, Name="Project Deadline Changed" } }
        };

        // GET: Tickets
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            IEnumerable<Ticket> tickets = db.Tickets.Include(t => t.AssignedTo).Include(t => t.Project).OrderBy(t => t.Priority.Id).ToList();

            return View(tickets);
        }

        //GET: Tickets/UserTickets
        //try route prefix for Tickets/{user.FirstName}
        [Authorize(Roles = "Project Manager, Developer, Submitter")]
        public ActionResult UserTickets()
        {
            
            var userId = User.Identity.GetUserId();
            
            IEnumerable<Project> projects;
            IEnumerable<Ticket> assignedTickets;
            IEnumerable<Ticket> submittedTickets;

            if (User.IsInRole("Developer"))
                
                assignedTickets = userId.ListUserTickets().OrderBy(t => t.Priority.Id);
            else
                assignedTickets = null;
           
            projects = userId.ListUserProjects().OrderByDescending(p => p.Deadline);
            
            submittedTickets = db.Tickets.Where(t => t.SubmitterId == userId).Where(t => t.Status.Name != "Resolved").OrderBy(t => t.Submitted).ToList();
            
            var model = new UserTicketsViewModel()
            {
                AssignedProjects = projects,
                AssignedTickets = assignedTickets,
                SubmittedTickets = submittedTickets
            };

            return View(model);
        }

        // GET: Tickets/Details/5
        [NoDirectAccess]
        public ActionResult Details(int? id)
        {
            Ticket ticket = db.Tickets.Include(t => t.Comments).Include(t => t.Logs).FirstOrDefault(t => t.Id == id);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ticket == null)
                return HttpNotFound();

            ViewBag.FileErrorMessage = TempData["FileErrorMessage"];

            return View(ticket);
        }

        //GET: Tickets/ChooseProject
        public ActionResult ChooseProject(string returnUrl)
        {
            //get current user
            var user = db.Users.Find(User.Identity.GetUserId());
            var projects = new List<Project>();

            if (User.IsInRole("Administrator"))
                //if current user is admin, has access to all projects
                projects = db.Projects.Where(p => p.IsResolved != true).ToList();
            else
                //if not admin, loop tru the list of users assigned to this project
                foreach (var project in db.Projects.ToList())
                    //if current user is in it
                    if (project.Users.Contains(user))
                        //choose the project
                        projects.Add(project);
            var model = new ChooseProjectViewModel()
            {
                Projects = new SelectList(projects, "Id", "Name"),
                returnUrl = returnUrl
            };

            return View(model);
        }
        //POST: Tickets/ChooseProject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChooseProject(ChooseProjectViewModel model)
        {
            //the project (which included an Id) selected by the user is stored in a var
            var project = db.Projects.Find(model.SelectedProjectId);

            //user is redirected to [GET] Create method, which expect a project ID...if you're not admin
            return RedirectToAction("Create", new { id = project.Id });
        }

        // GET: Tickets/Create
        public ActionResult Create(int? id)
        {
            var project = db.Projects.Find(id);
            var userId = User.Identity.GetUserId();
            string projectName;
            var projectId = project.Id;
            var developers = "Developer".UsersInRole().ToList();
            var projectDevelopers = new List<ApplicationUser>();

            //select project developers
            if (project != null)
            {
                foreach (var dev in developers)
                    if (project.Users.Any(u => u.Id == dev.Id))
                        projectDevelopers.Add(dev);
                projectName = project.Name;
            }
            else
                projectName = "";

            var model = new CreateEditTicketViewModel()
            {
                Ticket = new Ticket(),
                ProjectName = projectName,
                ProjectId = project.Id,
                Developers = new SelectList(projectDevelopers, "Id", "FullName"),
                Priorities = new SelectList(db.Priorities.OrderBy(p => p.Id), "Id", "Name"),
                Statuses = new SelectList(db.Statuses, "Id", "Name"),
                Phases = new SelectList(db.TicketPhases, "Id", "Name"),
                Actions = new SelectList(db.TicketActions, "Id", "Name")
            };
            return View(model);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ProjectId,SubmitterId,AssignedToId,PriorityId,PhaseId,StatusId,ActionId,Name,Submitted,Description")] Ticket ticket, int ProjectId)
        {
            var userId = User.Identity.GetUserId();
            var project = db.Projects.Find(ProjectId);

            if (ModelState.IsValid)
            {
                ticket.SubmitterId = userId;
                ticket.Submitted = DateTimeOffset.Now;
                ticket.ProjectId = ProjectId;
                var es = new EmailService();

                if (ticket.AssignedToId != null)
                    ticket.Status = db.Statuses.FirstOrDefault(s => s.Name == "Assigned");
                else
                    ticket.Status = db.Statuses.FirstOrDefault(s => s.Name == "Unassigned");

                db.Tickets.Add(ticket);
                db.SaveChanges();

                if (!User.IsInRole("Administrator"))
                {
                    var admins = "Administrator".UsersInRole().ToList();
                    var msgList = ticket.CreateTicketSubmittedMessage(admins);
                    var msg = msgList.First().Body;
                    foreach (var message in msgList)
                        es.Send(message);

                    var notif = ticket.Id.CreateTicketNotification(types[2], admins, msg);
                }

                if (ticket.AssignedToId!= null)
                {
                    var developer = db.Users.Find(ticket.AssignedToId);
                    var msg = ticket.CreateAssignedToTicketMessage(project, developer);
                    es.Send(msg);

                    //log notification
                    var notif = ticket.Id.CreateTicketNotification(types[2], new List<ApplicationUser> { developer }, msg.Body);
                    db.Notifications.Add(notif);
                    db.SaveChanges();
                }

                    return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        [NoDirectAccess]
        public ActionResult Edit(int? id)
        {
            Ticket ticket = db.Tickets.Find(id);
            var userId = User.Identity.GetUserId();
            var project = ticket.Project;
            string projectName;
            //var projects = new List<Project>();
            var developers = "Developer".UsersInRole();
            var projectDevelopers = new List<ApplicationUser>();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ticket == null)
                return HttpNotFound();

            //if (User.IsInRole("Administrator"))
            //    projects = db.Projects.Where(p => p.IsResolved != true).ToList();
            //else
            //    projects = userId.ListUserProjects().Where(p => p.IsResolved != true).ToList();

            //select project developers
            if (project != null)
            {
                foreach (var dev in developers.ToList())
                    if (project.Users.Any(u => u.Id == dev.Id))
                        projectDevelopers.Add(dev);
                projectName = project.Name;
            }
            else
                projectName = "";

            var model = new CreateEditTicketViewModel()
            {
                Ticket = ticket,
                ProjectName = projectName,
                //Projects = new SelectList(projects, "Id", "Name"),
                Developers = new SelectList(projectDevelopers, "Id", "FullName", ticket.AssignedTo),
                Priorities = new SelectList(db.Priorities.OrderBy(p => p.Id), "Id", "Name", ticket.Priority),
                Statuses = new SelectList(db.Statuses, "Id", "Name", ticket.Status),
                Phases = new SelectList(db.TicketPhases, "Id", "Name", ticket.Phase),
                Actions = new SelectList(db.TicketActions, "Id", "Name", ticket.Action)
            };

            return View(model);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public ActionResult Edit([Bind(Include = "Id,ProjectId,AssignedToId,PriorityId,PhaseId,StatusId,ActionId,Name,LastModified,Submitted, SubmitterId,Closed,Description")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
                var project = db.Projects.Find(ticket.ProjectId);
                var manager = db.Users.Find(project.ProjectManagerId);
                var userId = User.Identity.GetUserId();
                var es = new EmailService();

                var unassigned = db.Statuses.FirstOrDefault(s => s.Name == "Unassigned");
                if (ticket.StatusId == unassigned.Id && ticket.AssignedToId != null)
                    ticket.StatusId = db.Statuses.FirstOrDefault(t => t.Name == "Assigned").Id;

                var resolvedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Resolved");
                if (ticket.StatusId == resolvedStatus.Id)
                    ticket.Closed = DateTimeOffset.Now;

                ticket.LastModified = DateTimeOffset.Now;
                db.Entry(ticket).State = EntityState.Modified;

                var newLogs = oldTicket.CreateTicketChangeLogs(ticket, userId);
                db.Logs.AddRange(newLogs);
                

                //check for resolved status
                if (ticket.StatusId == resolvedStatus.Id)
                {
                    var recipientList = new List<ApplicationUser>();
                    var admins = "Administrator".UsersInRole();
                    var submitter = db.Users.Find(ticket.SubmitterId);

                    recipientList.Add(manager);
                    if (submitter != manager)
                        recipientList.Add(submitter);
                    recipientList.Union(admins);

                    var msgList = ticket.CreateTicketResolvedMessage(recipientList);
                    var msg = msgList.First().Body;
                    foreach (var message in msgList)
                        es.Send(message);

                    var notification = ticket.Id.CreateTicketNotification(types[3], recipientList, msg);
                    db.Notifications.Add(notification);
                }

                //check for assigned developer
                if (oldTicket.AssignedToId != ticket.AssignedToId)
                {
                    var oldDev = db.Users.Find(oldTicket.AssignedToId);
                    var newDev = db.Users.Find(ticket.AssignedToId);

                    if (oldDev != null)
                    {
                        var msg = ticket.CreateTicketReassignedMessage(project, oldDev);
                        es.Send(msg);
                        var notifOld = ticket.Id.CreateTicketNotification(types[6], new List<ApplicationUser> { oldDev }, msg.Body);
                        db.Notifications.Add(notifOld);
                    }

                    var msg2 = ticket.CreateAssignedToTicketMessage(project, newDev);
                    es.Send(msg2);
                    var notifNew = ticket.Id.CreateTicketNotification(types[2], new List<ApplicationUser> { newDev }, msg2.Body);
                    db.Notifications.Add(notifNew);
                }

                if (oldTicket != ticket && userId != manager.Id)
                {
                    var msg = ticket.CreateTicketModifiedMessage(manager);
                    es.Send(msg);

                    ticket.Id.CreateTicketNotification(types[5], new List<ApplicationUser> { manager }, msg.Body);
                }
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            return View(ticket);
        }

        //GET: Tickets/RequestReassignment/5
        [Authorize(Roles = "Developer")]
        public ActionResult RequestReassignment(int id)
        {
            var ticket = db.Tickets.Find(id);

            return View(ticket);
        }

        //POST: Tickets/RequestReassignment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Developer")]
        public ActionResult RequestReassignment(int id, string request)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var ticket = db.Tickets.Find(id);
            var es = new EmailService();
            var msg = ticket.CreateReassignmentRequestedMessage(user, request);
            es.Send(msg);

            return RedirectToAction("Details", "Tickets", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
