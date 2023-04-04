using Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

var parentDirectory = Directory.GetParent(Directory.GetCurrentDirectory());
var envFilePath = Path.Combine(parentDirectory!.FullName, ".env");

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvFile(envFilePath)
    .AddEnvironmentVariables();

IConfiguration configuration = configurationBuilder.Build();

string? connectionString = configuration["DB_CONNECTION_STRING"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Unable to find DB_CONNECTION_STRING in .env file or EnvironmentVariables.");
}

var optionsBuilder = new DbContextOptionsBuilder<SimpleDbContext>();

// Set CommandTimeout, e.g., 180 seconds (3 minutes)
optionsBuilder.UseSqlServer(connectionString, options => options.CommandTimeout(180));

using var ctx =  new SimpleDbContext(optionsBuilder.Options);

ctx.Authors.Add(new Author
{
    Name = "Master Yoda",
    Books = new List<Book>()
    {
        new Book
        {
            Title = "Way of the Jedi",
        }
    }
});
ctx.Authors.Add(new Author
{
    Name = "General Kenobi",
    Books = new List<Book>()
    {
        new Book
        {
            Title = "How to Train An Apprentice",
        }
    }
});
ctx.Authors.Add(new Author
{
    Name = "Anakin Skywalker",
    Books = new List<Book>()
    {
        new Book
        {
            Title = "The Importance of the High Ground",
        }
    }
});
ctx.Authors.Add(new Author
{
    Name = "Emperor Palpatine",
    Books = new List<Book>()
    {
        new Book
        {
            Title = "Secrets of the Sith",
        },
        new Book
        {
            Title = "How to Overthrow a Republic",
        }
    }
});

ctx.SaveChanges();
