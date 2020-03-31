using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogWalkerAPI.Models
{
    // C# representation of the Employee table
    public class Walker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NeighborhoodId { get; set; }
        public List<Walk> Walks { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}