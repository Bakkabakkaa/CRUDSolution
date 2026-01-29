using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class PersonsService : IPersonsService
{
    private readonly List<Person> _persons;
    private readonly ICountriesService _countriesService;

    public PersonsService()
    {
        _persons = new List<Person>();
        _countriesService = new CountriesService();
    }

    private PersonResponse ConvertPersonToPersonResponse(Person person)
    {
        PersonResponse personResponse = person.ToPersonResponse();
        personResponse.Country = _countriesService.GetCountryByCountryID(person.CountryID)?.CountryName;

        return personResponse;
    }
    public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
    {
        // Check if PersonAddRequest is not null
        if (personAddRequest == null)
        {
            throw new ArgumentNullException(nameof(personAddRequest));
        }
        
        // Validate PersonName
        if (string.IsNullOrEmpty(personAddRequest.PersonName))
        {
            throw new ArgumentException("Personname can't be blank");
        }
        
        // Convert personAddRequest into Person type
        Person person = personAddRequest.ToPerson();
        
        // Generate PersonID
        person.PersonID = Guid.NewGuid();
        
        // Add person object to persons list 
        _persons.Add(person);

        // Convert the Person object into PersonResponse type
        return ConvertPersonToPersonResponse(person);
        
    }

    public List<PersonResponse> GetAllPersons()
    {
        throw new NotImplementedException();
    }
}