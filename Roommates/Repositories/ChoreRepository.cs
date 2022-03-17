using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roommates.Models;
using Microsoft.Data.SqlClient;

namespace Roommates.Repositories
{
    internal class ChoreRepository : BaseRepository
    {
        public ChoreRepository(string connectionString) : base(connectionString) { }

        public List<Chore> GetAll()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM CHORE";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Chore> chores = new List<Chore>();

                        while (reader.Read())
                        {
                            int idColumnPosition = reader.GetOrdinal("Id");

                            int idValue = reader.GetInt32(idColumnPosition);

                            int nameColumnPosition = reader.GetOrdinal("Name");
                            string nameValue = reader.GetString(nameColumnPosition);

                            Chore chore = new Chore()
                            {
                                Id = idValue,
                                Name = nameValue
                            };

                            chores.Add(chore);
                        }
                        return chores;
                    }
                }
            }
        }

        public Chore GetById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Name FROM Chore WHERE ID = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Chore chore = null;

                        if (reader.Read())
                        {
                            chore = new Chore()
                            {
                                Id = id,
                                Name = reader.GetString(reader.GetOrdinal("Name")),

                            };
                        }
                        return chore;
                    }
                }
            }

        }

        public void Insert(Chore chore)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Chore (Name)
OUTPUT INSERTED.Id
VALUES (@name)";
                    cmd.Parameters.AddWithValue("@name", chore.Name);
                    int id = (int)cmd.ExecuteScalar();

                    chore.Id = id;
                }
            }
        }

        public List<Chore> GetUnassignedChores()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Chore
                        LEFT JOIN RoommateChore ON ChoreId = Chore.Id
                        WHERE RoommateChore.Id IS NULL;";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Chore> unassignedChores = new List<Chore>();

                        while (reader.Read())
                        {
                            Chore chore = new Chore()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            };

                            unassignedChores.Add(chore);
                        }

                        return unassignedChores;
                    }
                }
            }
        }

        public List<Chore> GetAssignedChores()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Chore
                        LEFT JOIN RoommateChore ON ChoreId = Chore.Id
                        WHERE RoommateChore.Id IS NOT NULL;";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Chore> assignedChores = new List<Chore>();

                        while (reader.Read())
                        {
                            Chore chore = new Chore()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            };

                            assignedChores.Add(chore);
                        }

                        return assignedChores;
                    }
                }
            }
        }

        public void AssignChore(int roommateId, int choreId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO RoommateChore (RoommateId, ChoreId)
VALUES (@roommateId, @choreId)";
                    cmd.Parameters.AddWithValue("@roommateId", roommateId);
                    cmd.Parameters.AddWithValue("@choreId", choreId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(Chore chore)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Chore
                                    SET Name = @name
                                    WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", chore.Id);
                    cmd.Parameters.AddWithValue("@name", chore.Name);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM RoommateChore WHERE ChoreId = @id;
DELETE FROM Chore WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ChoreCount> GetChoreCounts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Roommate.FirstName, COUNT(RoommateChore.Id) AS Count
                        FROM Roommate
                        LEFT JOIN RoommateChore ON Roommate.Id = RoommateChore.RoommateId
                        GROUP BY Roommate.Id, Roommate.FirstName";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var choreCounts = new List<ChoreCount>();

                        while (reader.Read())
                        {
                            var choreCount = new ChoreCount()
                            {
                                Name = reader.GetString(reader.GetOrdinal("FirstName")),
                                Count = reader.GetInt32(reader.GetOrdinal("Count"))

                            };

                            choreCounts.Add(choreCount);
                        }

                        return choreCounts;
                    }
                }
            }
        }

        public string GetChoreRoommateName(int choreId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT FirstName FROM RoommateChore 
JOIN Roommate ON RoommateChore.RoommateId = Roommate.Id
WHERE RoommateChore.ChoreId = @id";
                    cmd.Parameters.AddWithValue("@id", choreId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        string name = null;

                        if (reader.Read())
                        {
                            name = reader.GetString(reader.GetOrdinal("FirstName"));
                        }
                        return name;
                    }
                }
            }
        }

        public void ReassignChore(int roommateId, int choreId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM RoommateChore WHERE ChoreId = @chore1Id;";
                    cmd.Parameters.AddWithValue("@chore1Id", choreId);

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"INSERT INTO RoommateChore (RoommateId, ChoreId)
VALUES (@roommateId, @choreId)";
                    cmd.Parameters.AddWithValue("@roommateId", roommateId);
                    cmd.Parameters.AddWithValue("@choreId", choreId);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
