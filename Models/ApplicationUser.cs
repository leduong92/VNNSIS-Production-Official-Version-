
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VNNSIS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(22)]
        public string FirstName { get; set;}
        
        [StringLength(22)]
        public string LastName { get; set; }

        [StringLength(3)]
        public string Location { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}