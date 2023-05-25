using dotnet_project.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace dotnet_project.Data
{
 

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public  DbSet<Person> Persons_ { get; set; }



 
    }
}
