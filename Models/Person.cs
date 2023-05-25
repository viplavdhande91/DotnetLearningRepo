using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace dotnet_project.Models
{
    public class Person
    {
        [Key]
        public Guid PersonId { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
    }
}
