using dotnet_project.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_project.Services
{
    public class CharacterService :ICharacterService
    {
        
     private static List<Character> characters = new List<Character>{

       new Character(),

       new Character {Id =1,Name = "Sam"}

     };

     public List<Character>  GetAllCharacters()
     {

        return characters;
     }

      public Character  GetCharacterById(int id)
     {

        return characters.FirstOrDefault(c=>c.Id == id);
     }

      
    
        public List<Character>  AddCharacter(Character newCharacter)
     {
         
         characters.Add(newCharacter);
        return characters;
     }



    }
}