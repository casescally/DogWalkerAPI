using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using DogWalkerAPI.Models;
using Microsoft.AspNetCore.Http;

namespace DogWalkerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DogController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DogController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"Select d.Id, d.Name, d.OwnerId, d.Breed, d.Notes, o.Name as OwnerName, o.Address, o.OwnersNeighborhoodId, o.Phone, n.Name as NeighborhoodName

                        FROM Dog d

                        Left Join Owner o

                        On d.OwnerId = o.Id

                        LEFT Join Neighborhood n

                        ON o.OwnersNeighborhoodId = n.Id";

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Dog> dogs = new List<Dog>();

                    while (reader.Read())
                    {
                        Dog dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            Owner = new Owner()
                            {

                                Name = reader.GetString(reader.GetOrdinal("OwnerName")),

                                Address = reader.GetString(reader.GetOrdinal("Address")),

                                OwnersNeighborhoodId = reader.GetInt32(reader.GetOrdinal("OwnersNeighborhoodId")),

                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            }
                        };

                        dogs.Add(dog);
                    }
                    reader.Close();

                    return Ok(dogs);
                }
            }
        }

        [HttpGet("{id}", Name = "GetDog")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"Select d.Id, d.Name, d.OwnerId, d.Breed, d.Notes, o.Name as OwnerName, o.Id, o.Address, o.OwnersNeighborhoodId, o.Phone, n.Name as NeighborhoodName

                        FROM Dog d

                        Left Join [Owner] o

                        On d.OwnerId = o.Id

                        LEFT Join Neighborhood n

                        ON o.OwnersNeighborhoodId = n.Id

                        WHERE d.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dog dog = null;

                    if (reader.Read())
                    {
                        dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            Owner = new Owner()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("OwnerName")),

                                Address = reader.GetString(reader.GetOrdinal("Address")),

                                OwnersNeighborhoodId = reader.GetInt32(reader.GetOrdinal("OwnersNeighborhoodId")),

                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            }


                        };
                    }
                    reader.Close();

                    return Ok(dog);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Dog dog)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Dog (Name, OwnerId, Breed, Notes)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name, @OwnerId, @Breed, @Notes)";
                    cmd.Parameters.Add(new SqlParameter("@Name", dog.Name));
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", dog.OwnerId));
                    cmd.Parameters.Add(new SqlParameter("@Breed", dog.Breed));
                    cmd.Parameters.Add(new SqlParameter("@Notes", dog.Notes));

                    int newId = (int)cmd.ExecuteScalar();
                    dog.Id = newId;
                    return CreatedAtRoute("GetDog", new { id = newId }, dog);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Dog dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Dog
                                            SET Name = @Name, OwnerId=@OwnerId, Breed=@Breed, Notes=@notes
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@Name", dog.Name));
                        cmd.Parameters.Add(new SqlParameter("@OwnerId", dog.OwnerId));                        
                        cmd.Parameters.Add(new SqlParameter("@Breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@Notes", dog.Notes));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Dog WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DogExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM Dog
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}