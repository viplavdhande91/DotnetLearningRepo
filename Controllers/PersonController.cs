using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet_project.Data;
using dotnet_project.Models;
using Microsoft.Data.SqlClient;

namespace dotnet_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly DataContext _context;

        public PersonController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Person
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPersons_()
        {
            return await _context.Persons_.ToListAsync();
        }

        //// GET: api/Person/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Person>> GetPerson(Guid id)
        //{
        //    var person = await _context.Persons_.FindAsync(id);

        //    if (person == null)
        //    {
        //        return NotFound();
        //    }

        //    return person;
        //}

        [HttpGet("SP/{personId}")] //STORED PROCEDURE WITH  PARAMETERS
        public async Task<ActionResult<List<Person>>> GetCharacters(Guid personId)
        {
            


            var result = await _context.Persons_.FromSqlRaw("EXEC SelectSpecificPerson {0}", personId.ToString()).ToListAsync();
             
            return Ok(result);

            
        


        }


        [HttpGet("SP")] //STORED PROCEDURE WITH NO PARAMETERS
        public async Task<ActionResult<List<Person>>> GetAllCharactersSP()
        {
            var result = await _context.Persons_.FromSqlRaw("SelectAllPersons").ToListAsync();

            return Ok(result);
        }

        // PUT: api/Person/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(Guid id, Person person)
        {
            if (id != person.PersonId)
            {
                return BadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Person
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            _context.Persons_.Add(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPerson", new { id = person.PersonId }, person);
        }

        // DELETE: api/Person/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(Guid id)
        {
            var person = await _context.Persons_.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _context.Persons_.Remove(person);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PersonExists(Guid id)
        {
            return _context.Persons_.Any(e => e.PersonId == id);
        }
    }
}
