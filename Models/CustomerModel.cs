
using System;
using System.ComponentModel.DataAnnotations;


namespace VNNSIS.Models
{
    public class CustomerModel
    {
        [StringLength(22)]
        [Key]
        public string FirstName { get; set;}
        
        [StringLength(22)]
        public string LastName { get; set; }

        [StringLength(3)]
        public string Location { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}