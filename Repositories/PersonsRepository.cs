using System.Linq.Expressions;
using Entities;
using RepositoryContracts;

namespace Repositories;

public class PersonsRepository : IPersonsRepository
{
    public Task<Person> AddPerson(Person person)
    {
        throw new NotImplementedException();
    }

    public Task<List<Person>> GetAllPerson()
    {
        throw new NotImplementedException();
    }

    public Task<Person> GetPersonByPersonID(Guid personID)
    {
        throw new NotImplementedException();
    }

    public Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePersonByPersonID(Guid personID)
    {
        throw new NotImplementedException();
    }

    public Task<Person> UpdatesPerson(Person person)
    {
        throw new NotImplementedException();
    }
}