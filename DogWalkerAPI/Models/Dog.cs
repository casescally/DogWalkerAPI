using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogWalkerAPI.Models
{
    // C# representation of the Employee table
    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }

        //This is to hold the actual foreign key integer
        public string Breed { get; set; }

        // This property is for storing the C# object representing the department
        public string Notes { get; set; }
        public Owner Owner { get; set; }
    }
}