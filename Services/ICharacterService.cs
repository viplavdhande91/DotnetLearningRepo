using dotnet_project.Models;
using System.Collections.Generic;


namespace dotnet_project.Services
{
    public interface ICharacterService
    {  
        List<Character> GetAllCharacters();
        Character GetCharacterById(int id);

        List<Character> AddCharacter(Character newCharacter);

        
    } 
   
}