using Microsoft.EntityFrameworkCore;

namespace Entities;

public class PersonsDbContext :DbContext
{
    public DbSet<Country> Countries { get; set; }
    public DbSet<Person> Persons { get; set; }

    public PersonsDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Country>().ToTable("Countries");
        modelBuilder.Entity<Person>().ToTable("Persons");
        
        // Seed to Countries
        string countriesJson = System.IO.File.ReadAllText("countries.json");

        List<Country>? countries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesJson);

        foreach (var country in countries)
        {
            modelBuilder.Entity<Country>().HasData(country);
        }
        
        // Seed to Persons
        string personJson = System.IO.File.ReadAllText("persons.json");

        List<Person>? persons = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personJson);

        foreach (var person in persons)
        {
            modelBuilder.Entity<Person>().HasData(person);
        }
    }
}