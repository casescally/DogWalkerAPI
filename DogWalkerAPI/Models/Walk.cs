using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepartmentsEmployeesAPI.Models
{
    // C# representation of the Employee table
    public class Walks
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public int WalkerId { get; set; }
        public int DogId { get; set; }
    }
}