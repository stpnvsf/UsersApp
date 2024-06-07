using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using Application;

namespace DatabaseLayer
{
    public sealed class InitDbRepository : IInitDb
    {
        private readonly IConfiguration _configuration;

        public InitDbRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task Init()
        {
            //https://stackoverflow.com/questions/29190081/create-database-and-table-in-a-single-sql-script#comment46594682_29190121
            await CreateDatabase();
            await CreateTables();
        }

        private async Task CreateDatabase()
        {
            var query = """
                IF NOT EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'UserDatabase') AND type in (N'U'))
                BEGIN
                    CREATE DATABASE UserDatabase;
                END
                """;

            await using (var db = new SqlConnection(_configuration.GetConnectionString("InitDatabase")))
            {
                await db.ExecuteAsync(query);
            }
        }

        private async Task CreateTables()
        {
            var query = """
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'User') AND type in (N'U'))
                BEGIN
                	CREATE TABLE [User] (
                	    Guid int identity,
                	    Login varchar(200),
                	    Password varchar(200),
                	    Name varchar(200),
                	    Gender int,
                	    Birthday datetime,
                	    Admin bit,
                	    CreatedOn datetime,
                	    CreatedBy varchar(200),
                	    ModifiedOn datetime,
                	    ModifiedBy varchar(200),
                	    RevokedOn datetime,
                	    RevokedBy varchar(200)
                		);
                
                
                    INSERT INTO [User](Login, Password, Name, Admin)
                    	VALUES 
                    		('admin', '0000', 'admin', 1 );                    
                END;
                """;
                //var query = """
                //IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'User') AND type in (N'U'))
                //    BEGIN
                //		CREATE TABLE User(
                //        Guid int identity,
                //        Login varchar(200),
                //        Password varchar(200),
                //        Name varchar(200),
                //        Gender int,
                //        Birthday datetime,
                //        Admin bit,
                //        CreatedOn datetime,
                //        CreatedBy varchar(200),
                //        ModifiedOn datetime,
                //        ModifiedBy varchar(200),
                //        RevokedOn datetime,
                //        RevokedBy varchar(200),
                //	);
                
                //    INSERT INTO User(Login, Password, Name, Admin)
                //    	VALUES 
                //    		('admin', '0000', 'admin', 1 );                    
                //END
                //""";

            await using (var db = new SqlConnection(_configuration.GetConnectionString("Database")))
            {
                await db.ExecuteAsync(query);
            }
        }
    }
}
