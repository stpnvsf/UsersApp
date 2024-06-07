using System.Data;
using System.Data.SqlClient;
using Dapper;
using Application;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace DatabaseLayer
{
    public sealed class Repository : IRepository
    {

        private readonly string _connectionString;

        public Repository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Database");
        }

        public async Task<int> AddAsync(UserDTO user, string createdBy)
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                try
                {
                    var userParameters = new
                    {
                        Login = user.Login,
                        Password = user.Password,
                        Name = user.Name,
                        Gender = user.Gender,
                        Birthday = user.Birthday,
                        Admin = user.Admin,
                        CreatedOn = user.CreatedOn,
                        CreatedBy = user.CreatedBy,
                    };

                    var userInsert = """
                         INSERT INTO [User] (
                            Login, 
                            Password, 
                            Name, 
                            Gender, 
                            Birthday, 
                            Admin ,
                            CreatedOn, 
                            CreatedBy 
                         )
                         OUTPUT INSERTED.Guid
                         VALUES (
                            @Login,
                            @Password,
                            @Name,
                            @Gender,
                            @Birthday,
                            @Admin,
                            @CreatedOn,
                            @CreatedBy
                         )
                         """;
                    var userId = await db.QuerySingleAsync<int>(userInsert, userParameters);

                    return userId;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public async Task<UserDTO> GetByLoginAsync(string login)
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                try
                {
                    var user = """
                     SELECT *
                     FROM [User]
                     WHERE Login = @Login
                     """;
                    var userId = await db.QuerySingleAsync<UserDTO>(user, new { Login = login });

                    return userId;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<int> ChangeAsync(UserDTO user, string name)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var userParameters = new
                    {
                        Login = user.Login,
                        Password = user.Password,
                        Name = user.Name,
                        Gender = user.Gender,
                        Birthday = user.Birthday,
                        Admin = user.Admin,
                        ModifiedOn = DateTime.Now,
                        ModifiedBy = name,

                    };

                    string query = $"""
                                    UPDATE [User]
                                    SET 
                                    Login = @Login,
                                    Password = @Password,
                                    Name = @Name,
                                    Gender = @Gender,
                                    Birthday = @Birthday,
                                    Admin = @Admin,
                                    ModifiedOn = @ModifiedOn,
                                    ModifiedBy = @ModifiedBy
                                    WHERE Login = @Login
                                    """;
                    var result = await db.QuerySingleAsync(query, userParameters);

                    return result;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<int> Delete(string login)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    string query = $"""
                                    DELETE 
                                    FROM [User]
                                    WHERE Login = @Login
                                    """;
                    var result = await db.QuerySingleAsync(query, new { Login = login });

                    return result;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        public async Task<int> DeleteSoft(string login, string name)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var userParameters = new
                    {
                        Login = login,
                        RevokedBy = name,
                        RevokedOn = DateTime.Now
                    };

                    string query = $"""
                                    UPDATE [User]
                                    SET 
                                    RevokedOn = @RevokedOn,
                                    RevokedBy = @RevokedBy
                                    WHERE Login = @Login
                                    """;
                    var result = await db.QuerySingleAsync<int>(query, userParameters);

                    return result;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<List<UserDTO>> GetAllActiveUsersAsync()
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                try
                {
                    var user = """
                     SELECT *
                     FROM [User]
                     WHERE RevokedOn = 0 or RevokedOn is null
                     ORDER BY CreatedOn;
                     """;
                    var users = await db.QueryAsync<UserDTO>(user);

                    return (List<UserDTO>)users;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<List<UserDTO>> GetAllOlderThanAsync(int age)
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                try
                {
                    var user = """
                     SELECT *
                     FROM [User]
                     WHERE DATEDIFF(YEAR , Birthday , (SELECT CONVERT (date, CURRENT_TIMESTAMP) )) >= @Age;
                     """;
                    var users = await db.QueryAsync<UserDTO>(user, new { Age = age});

                    return (List<UserDTO>)users;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        public async Task<int> RecoverUserAsync(string login)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    string query = $"""
                                    UPDATE [User]
                                    SET 
                                    RevokedOn = null,
                                    RevokedBy = null
                                    WHERE Login = @Login
                                    """;
                    var result = await db.QuerySingleAsync(query, new { Login = login });

                    return result;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task ChangePasswordAsync(string login, string password, string name)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var userParameters = new
                    {
                        Login = login,
                        Password = password,
                        ModifiedOn = DateTime.Now,
                        ModifiedBy = name

                    };

                    string query = $"""
                                    UPDATE [User]
                                    SET 
                                    Password = @Password,
                                    ModifiedOn = @ModifiedOn,
                                    ModifiedBy = @ModifiedBy
                                    WHERE Login = @Login
                                    """;
                    var result = await db.QueryAsync(query, userParameters);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task ChangeLoginAsync(string login, string name)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var userParameters = new
                    {
                        Login = login,
                        ModifiedOn = DateTime.Now,
                        ModifiedBy = name

                    };

                    string query = $"""
                                    UPDATE [User]
                                    SET 
                                    Password = @Password,
                                    ModifiedOn = @ModifiedOn,
                                    ModifiedBy = @ModifiedBy
                                    WHERE Login = @Login
                                    """;
                    var result = await db.QueryAsync(query, userParameters);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task ChangeAsync(string login, string name, int gender, DateTime date, string nameAuth)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var userParameters = new
                    {
                        Login = login,
                        Name = name,
                        Gender = gender,
                        Date = date,
                        ModifiedOn = DateTime.Now,
                        ModifiedBy = nameAuth

                    };

                    string query = $"""
                                    UPDATE [User]
                                    SET 
                                    Name = @Name,
                                    Gender = @Gender,
                                    Birthday = @Date
                                    ModifiedOn = @ModifiedOn,
                                    ModifiedBy = @ModifiedBy
                                    WHERE Login = @Login
                                    """;
                    var result = await db.QueryAsync(query, userParameters);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

    }
}
