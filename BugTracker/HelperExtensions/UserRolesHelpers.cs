using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class UserRolesHelpers
    {
        //Create a usermanager object that contain users
        private static UserManager<ApplicationUser> manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        private static ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Any member that call this method will provide you with a userID, and a role name (ex. Developer). If this user Id is attached to that role (ex. Developer), return true
        /// </summary>
        public static bool UserIsInRole(this string userId, string roleName)
        {
            return manager.IsInRole(userId, roleName);
        }


        /// <summary>
        /// Any member that call this method will provide you with a user ID, and based on this ID it return all the roles (Ex. Submitter, Developer,...) this user is currently in
        /// </summary>
        public static IList<string> ListUserRoles(this string userId)
        {           
            return manager.GetRoles(userId).ToList();
        }

        /// <summary>
        ///  Any member that call this method will provide you with a roleName (Ex "Developer"). This method will go to Role DB, find the rolename, and list all users attached to it. Then return that list
        /// </summary>
        public static IList<ApplicationUser> UsersInRole(this string roleName)
        {
            var role = db.Roles.FirstOrDefault(r => r.Name == roleName);
            var userList = new List<ApplicationUser>();
           
            foreach (var user in db.Users)
                if (UserIsInRole(user.Id, roleName))
                    userList.Add(user);

            return userList;
        }

        //public static IList<ApplicationUser> UsersNotInRole(this string roleName)
        //{
        //    //CONVERT TO LINQ
        //    var role = db.Roles.FirstOrDefault(r => r.Name == roleName);
        //    var users = manager.Users.ToList();
        //    var userList = (IList<ApplicationUser>)role.Users;

        //    foreach (var user in userList)
        //        users.Remove(user);

        //    return users;
        //}

        /// <summary>
        /// any member that call this method will provide a user Id, and a role name (ex. Submitter). This method will add the user to that role. If done successfully return true
        /// </summary>
        public static bool AddUserToRole(this string userId, string roleName)
        {           
            var result = manager.AddToRole(userId, roleName);
            return result.Succeeded;
        }

        /// <summary>
        /// any member that call this method will provide a user Id, and a role name (ex. Submitter). This method will remove the user from that role. If done successfully return true
        /// </summary>
        public static bool RemoveUserFromRole(this string userId, string roleName)
        {
            var result = manager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        public static ApplicationUser GetProjectManager(this string userId)
        {
            var user = db.Users.Find(userId);
            return user;
        }
    }
}