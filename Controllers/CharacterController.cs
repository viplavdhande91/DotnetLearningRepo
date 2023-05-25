using dotnet_project.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace dotnet_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class CharacterController : ControllerBase
    {

        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService) //DEPENDENCY INJECTION
        {
            _characterService = characterService;
        }



        [HttpGet("getCount")]
        public IActionResult Get()
        {

            return Ok(_characterService.Get());
        }



    [HttpPost()]
            
     public IActionResult AddCharacter(int numberNew)
    {

        return Ok(_characterService.Add(numberNew));
    }




        
    
}

}