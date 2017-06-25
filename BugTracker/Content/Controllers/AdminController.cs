using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.HelperExtensions;
using Microsoft.AspNet.Identity;
using System.Collections;
using System.Data.Entity;

namespace BugTracker.Controllers
{
    [Authorize]
    [RequireHttps]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Users
        [Authorize(Roles = "Project Manager, Administrator")]
        public ActionResult Users()
        {
            var users = db.Users.Include(u=>u.Projects).ToList();
            return View(users);
        }

        //GET: Admin/Users
        [Authorize(Roles = "Project Manager, Administrator")]
        
        public PartialViewResult _UserInfo(string id)
        {
            UserInfoViewModel userInfo = new UserInfoViewModel();
            
            userInfo.User = db.Users.Find(id);
            userInfo.AssignedProjects = db.Projects.Where(p=>p.ProjectManagerId == id).ToList();
            userInfo.AssignedTickets = id.ListUserTickets();
           
            userInfo.SelectedRoles = id.ListUserRoles().ToList();
            
            userInfo.AllRoles = new MultiSelectList(db.Roles, "Name", "Name", userInfo.SelectedRoles);

            return PartialView(userInfo);
        }

        //POST: Admin/Users/AddRemoveRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult AddRemoveRole(string userId, List<string> SelectedRoles)
        {
            var user = db.Users.Find(userId);

            if (ModelState.IsValid)
            {
                var roles = db.Roles.ToList();
                //Select all the role names (not the IDs) from the AspNetRoles table
                foreach (var role in roles.Select(r => r.Name))
                {
                    try
                    {
                        if (SelectedRoles.Contains(role))
                            //From those list of roles
                            //Assign on to the user
                            user.Id.AddUserToRole(role);
                        else
                        if (user.Id.UserIsInRole(role))
                            //or remove role from user
                            user.Id.RemoveUserFromRole(role);
                        
                    }
                    catch (Exception)
                    {
                        ViewBag.ErrorMessage = "Please try again.";
                    }

                }

                //notify user of role change
                //var es = new EmailService();
                //var user = db.Users.Find(model.User.Id);
                //var msg = user.CreateUserRolesModifiedMessage();
                //es.Send(msg);

                ViewBag.SuccessMessage = "User roles have been successfully updated.";
                return RedirectToAction("Users");
            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again, or contact tech support.";
            //return RedirectToAction("Users");
            return RedirectToAction("Users");
        }


        //GET:Admin/Notifications
        public ActionResult Notifications()
        {
            var notifications = db.Notifications.ToList();
            var model = new List<NotificationsViewModel>();
            foreach (var notif in notifications)
            {
                var project = db.Projects.Find(notif.ProjectId);
                var ticket = db.Tickets.Find(notif.TicketId);
                var type = db.NotificationTypes.Find(notif.TypeId);
                var message = notif.Message.Replace("<br/>", " | "); ;

                model.Add(new NotificationsViewModel
                {
                    ProjectName = project != null ? project.Name : null,
                    TicketName = ticket != null ? ticket.Name : null,
                    Type = type.Name,
                    Recipients = notif.Recipients,
                    SendDate = notif.SendDate.FormatDateTimeOffset(),
                    Message = message
                });
            }

            return View(model);
        }

        ////GET: Admin/Roles/_AddRemoveUsers/roleName
        //[Authorize(Roles = "Administrator")]
        //public PartialViewResult _AddRemoveUsers(string roleName)
        //{
        //    var users = new List<string>();
        //    foreach (var user in db.Users)
        //        users.Add(user.FullName);

        //    var model = new AddRemoveUsersViewModel();
        //    model.RoleName = roleName;
        //    model.Users = new MultiSelectList(db.Users, "Id", "FullName", users);

        //    return PartialView(model);
        //}

        ////GET: Admin/Roles
        //[Authorize(Roles = "Administrator")]
        //public ActionResult Roles()
        //{
        //    var developers = db.Roles.FirstOrDefault(r => r.Name == "Developer").Name.UsersInRole().AsEnumerable();
        //    var administrators = db.Roles.FirstOrDefault(r => r.Name == "Administrator").Name.UsersInRole().AsEnumerable();
        //    var projectManagers = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Name.UsersInRole().AsEnumerable();
        //    var submitters = db.Roles.FirstOrDefault(r => r.Name == "Submitter").Name.UsersInRole().AsEnumerable();

        //    var model = new RolesIndexViewModel()
        //    {
        //        Submitters = submitters,
        //        Developers = developers,
        //        ProjectManagers = projectManagers,
        //        Administrators = administrators
        //    };

        //    return View(model);
        //}
    }
}