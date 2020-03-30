using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogWalkerAPI.Models
{
    // C# representation of the Department table
    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int OwnersNeighborhoodId { get; set; }
        public string Phone { get; set; }
        public List<Dog> Dogs { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}