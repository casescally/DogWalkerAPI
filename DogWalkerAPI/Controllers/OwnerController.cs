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
    public class OwnerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OwnerController(IConfiguration config)
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
        public async Task<IActionResult> Get(
            [FromQuery] int? neighborhoodId,
            [FromQuery] string include,
            [FromQuery] string q)
        {
            if (include != "neighborhoodName")
            {
                var owners = GetAllOwners(neighborhoodId, q);
                return Ok(owners);
            } else
            {
                var owners = 
            }
        }

        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT o.Id, o.[Name], o.[Address], o.OwnersNeighborhoodId, o.Phone, n.[Name] NeighborhoodName, d.Id DogId ,d.[Name] DogName, d.Breed, d.Notes
                        FROM [Owner] o
                        Left Join  Neighborhood n
                        On o.OwnersNeighborhoodId = n.Id
                        LEFT JOIN Dog d
                        On o.Id = d.OwnerId
                        Where o.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner owner = null;

                    while (reader.Read())

                    {
                        if (owner == null)
                        {
                            owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                OwnersNeighborhoodId = reader.GetInt32(reader.GetOrdinal("OwnersNeighborhoodId")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Neighborhood = new Neighborhood()
                                {

                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),

                                    Name = reader.GetString(reader.GetOrdinal("Name"))

                                },
                                Dogs = new List<Dog>()
                            };
                        }
                        owner.Dogs.Add(new Dog()
                        {

                            Id = reader.GetInt32(reader.GetOrdinal("DogId")),

                            OwnerId = reader.GetInt32(reader.GetOrdinal("Id")),

                            Name = reader.GetString(reader.GetOrdinal("DogName")),

                            Breed = reader.GetString(reader.GetOrdinal("Breed")),

                            Notes = reader.GetString(reader.GetOrdinal("Notes"))
                        });
                    }
                    reader.Close();

                    return Ok(owner);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Owner (Name, OwnerName, Address, OwnersNeighborhoodId, Phone)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name, @OwnerId, @Breed, @Notes)";
                    cmd.Parameters.Add(new SqlParameter("@Name", owner.Name));
                    cmd.Parameters.Add(new SqlParameter("@Address", owner.Address));
                    cmd.Parameters.Add(new SqlParameter("@OwnersNeighborhoodId", owner.OwnersNeighborhoodId));
                    cmd.Parameters.Add(new SqlParameter("@Phone", owner.Phone));

                    int newId = (int)cmd.ExecuteScalar();
                    owner.Id = newId;
                    return CreatedAtRoute("GetOwner", new { id = newId }, owner);
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
                if (!OwnerExists(id))
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
                        cmd.CommandText = @"DELETE FROM [Owner] WHERE Id = @id";
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
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool OwnerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM Owner
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}