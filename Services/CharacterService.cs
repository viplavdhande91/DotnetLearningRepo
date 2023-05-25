using System.Collections.Generic;
using System.Linq;

namespace dotnet_project.Services
{
    public class CharacterService :  ICharacterService
    {
        public int _count { get; set; }

        // CLASS METHODS
        public int Get()
        {
            return _count;
        }


        public int Add(int count)
        {   
            
            _count = _count + count ;
            return _count;
        }



    }
}

