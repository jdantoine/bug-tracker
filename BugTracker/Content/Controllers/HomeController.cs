using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BugTracker.HelperExtensions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;

namespace BugTracker.Controllers
{
    
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles ="Administrator, Project Manager, Developer")]
        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var tickets = new List<Ticket>();
            var attachments = new List<Models.Attachment>();
            var comments = new List<Comment>();
            int projects = 0;        

            if (User.IsInRole("Administrator"))
            {
                tickets = db.Tickets.ToList();
                attachments = db.Attachments.Take(5).ToList();
                projects = db.Projects.Where(p => p.IsResolved != true).Count();
                foreach (var ticket in tickets)
                    foreach (var comment in ticket.Comments)
                        comments.Add(comment);
            }
            else if (User.IsInRole("Project Manager"))
            {
                tickets = db.Tickets.Where(t => t.Project.ProjectManagerId == userId).ToList();
                foreach (var ticket in tickets)
                    foreach (var attach in ticket.Attachments)
                        attachments.Add(attach);
                foreach (var ticket in tickets)
                    foreach (var comment in ticket.Comments)
                        comments.Add(comment);
                projects = userId.ListUserProjects().Count();
            }
            else if (User.IsInRole("Developer") && User.IsInRole("Project Manager"))
            {
                tickets = db.Tickets.Where(t=>t.Project.Users.Contains(db.Users.Find(userId))).ToList();
                foreach (var ticket in tickets)
                    foreach (var attach in ticket.Attachments)
                        attachments.Add(attach);
                foreach (var ticket in tickets)
                    foreach (var comment in ticket.Comments)
                        comments.Add(comment);
                projects = userId.ListUserProjects().Count();
            }
            else if (User.IsInRole("Developer"))
            {
                tickets = db.Tickets.Where(t => t.AssignedToId == userId).ToList();
                foreach (var ticket in tickets)
                    foreach (var attach in ticket.Attachments)
                        attachments.Add(attach);
                foreach (var ticket in tickets)
                    foreach (var comment in ticket.Comments)
                        comments.Add(comment);
                projects = userId.ListUserProjects().Count();
            }

            var model = new DashboardViewModel()
            {
                Tickets = tickets,
                Attachments = attachments,
                Comments = comments.Take(5),
                ProjectsAmt = projects
            };

            return View(model);
        }

        public ActionResult GetCharts()
        {
            var resolved = db.Statuses.FirstOrDefault(s => s.Name == "Resolved");

            var priorityDonut = (from priority in db.Priorities
                                 let tickets = db.Tickets.Where(t => t.PriorityId == priority.Id).ToList()
                                 let ticketCount = tickets.Count()
                                 where ticketCount > 0 
                                 select new
                                 {
                                     label = priority.Name,
                                     value = ticketCount
                                 }).ToArray();

            var actionDonut = (from action in db.TicketActions
                               let tickets = db.Tickets.Where(t => t.ActionId == action.Id).ToList()
                               let ticketCount = tickets.Count()
                               where ticketCount > 0
                               select new
                               {
                                   label = action.Name,
                                   value = ticketCount
                               }).ToArray();

            var phaseDonut = (from phase in db.TicketPhases
                              let tickets = db.Tickets.Where(t => t.PhaseId == phase.Id).ToList()
                              let ticketCount = tickets.Count()
                              where ticketCount > 0
                              select new
                              {
                                  label = phase.Name,
                                  value = ticketCount
                              }).ToArray();

            var allData = new
            {
                priorityDonut = priorityDonut,
                actionDonut = actionDonut,
                phaseDonut = phaseDonut
            };

            return Content(JsonConvert.SerializeObject(allData), "application/json");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var from = model.FromEmail;
                    var email = new MailMessage(from, ConfigurationManager.AppSettings["emailto"])
                    {
                        Subject = model.Subject,
                        Body = $"<strong>{model.FromName}</strong> left you a message: {model.Body}. The user's email is <strong>{model.FromEmail}</strong>",
                        IsBodyHtml = true
                    };

                    var svc = new PersonalEmail();
                    await svc.SendAsync(email);
                    return RedirectToAction("Sent", "Home");
                    //return File("~/ContactEmail1.html", "text/html");
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    await Task.FromResult(0);
                }
            }
            return View(model);
        }

        public ActionResult Sent()
        {
            return View();
        }

    }
}