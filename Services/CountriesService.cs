using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    private readonly List<Country> _countries;

    public CountriesService(bool initialize = true)
    {
        _countries = new List<Country>();
        if (initialize)
        {
            _countries.AddRange(new List<Country>()
            {
                new Country()
                {
                    CountryID = Guid.Parse("3F0243A7-1B3A-4ACD-AD63-11151BFF251A"),
                    CountryName = "USA"
                },
                new Country()
                {
                    CountryID = Guid.Parse("DED50EEE-E1A4-46D4-B6E4-21AE22FFF791"),
                    CountryName = "Canada"
                },
                new Country()
                {
                    CountryID = Guid.Parse("B8D30CF2-AD9C-4598-86CE-C372787D6B02"),
                    CountryName = "UK"
                },
                new Country()
                {
                    CountryID = Guid.Parse("FFE0511A-DE54-46B3-AB4B-43FD7D753049"),
                    CountryName = "India"
                },
                new Country()
                {
                    CountryID = Guid.Parse("834F1F53-BC40-474E-826C-FAFC00F25467"),
                    CountryName = "Australia"
                }
            });
        }
    }
    public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
    {
        //Validation: countryAddRequest parameter can't be null
        if (countryAddRequest == null)
        {
            throw new ArgumentNullException(nameof(countryAddRequest));
        }
        
        //Validation CountryName can't be null
        if (countryAddRequest.CountryName == null)
        {
            throw new ArgumentException(nameof(countryAddRequest.CountryName));
        }
        
        //Validation: CountryName can't be duplicate
        if (_countries.Where(temp => temp.CountryName == countryAddRequest.CountryName).Count() > 0)
        {
            throw new ArgumentException("Given country name already exists");
        }
        //Convert Object from CountryAddRequest to Country typ
        Country country = countryAddRequest.ToCountry();
        
        //Generate CountryID
        country.CountryID = Guid.NewGuid();
        
        //Add country object into _countries
        _countries.Add(country);

        return country.ToCountryResponse();
    }

    public List<CountryResponse> GetAllCountries()
    {
        return _countries.Select(country => country.ToCountryResponse()).ToList();
    }

    public CountryResponse? GetCountryByCountryID(Guid? countryID)
    {
        if (countryID == null)
        {
            return null;
        }
        
        Country? country_response_from_list = _countries.FirstOrDefault(temp => temp.CountryID == countryID);

        if (country_response_from_list == null)
        {
            return null;
        }
        return country_response_from_list.ToCountryResponse();
    }
}