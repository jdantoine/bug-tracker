﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class UserTicketsViewModel
    {
        public IEnumerable<Ticket> AssignedTickets;
        public IEnumerable<Project> AssignedProjects;
        public IEnumerable<Ticket> SubmittedTickets;
    }

    public class CreateEditTicketViewModel
    {
        public Ticket Ticket { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }

        public SelectList Projects { get; set; }
        public SelectList Developers { get; set; }
        public SelectList Priorities { get; set; }
        public SelectList Statuses { get; set; }
        public SelectList Phases { get; set; }
        public SelectList Actions { get; set; }
    }

    public class ChooseProjectViewModel
    {
        public SelectList Projects { get; set; }
        [Required]
        public int SelectedProjectId { get; set; }
        public string returnUrl { get; set; }
    }
}