using Entities;
using RepositoryContracts;

namespace Repositories;

public class CountriesRepository : ICountriesRepository
{
    public Task<Country> AddCountry(Country country)
    {
        throw new NotImplementedException();
    }

    public Task<List<Country>> GetAllCountries()
    {
        throw new NotImplementedException();
    }

    public Task<Country?> GetCountryByCountryID(Guid countryID)
    {
        throw new NotImplementedException();
    }

    public Task<Country?> GetCountryByCountryName(string countryName)
    {
        throw new NotImplementedException();
    }
}