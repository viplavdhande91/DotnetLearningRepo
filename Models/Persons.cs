using System;
using System.Collections.Generic;

namespace dotnet_project.Models
{
    public partial class Persons
    {
        public int? PersonId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
    }
}
