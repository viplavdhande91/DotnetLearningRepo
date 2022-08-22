
using Microsoft.AspNetCore.Mvc;
using dotnet_project.Models;
using System.Collections.Generic;
using System.Linq;
using dotnet_project.Services;
namespace dotnet_project.Controllers
{


    [ApiController]
    [Route("[controller]")]
  
  
    public class CharacterController: ControllerBase
    {

        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService) //DEPENDENCY INJECTION
        {
            this._characterService =characterService;
        }
       
     
     
     [HttpGet("getall")]
     public IActionResult Get()
     {

        return Ok(_characterService.GetAllCharacters());
     }

     [HttpGet("{id}")]

      public IActionResult GetSingle(int id)
     {

        return Ok(_characterService.GetCharacterById(id));
     }

      
    
    [HttpPost]
        public IActionResult AddCharacter(Character newCharacter)
     {
         
         return Ok(_characterService.AddCharacter(newCharacter));
     }




    }
}