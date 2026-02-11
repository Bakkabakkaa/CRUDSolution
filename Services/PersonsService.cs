using System.ComponentModel.DataAnnotations;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private readonly PersonsDbContext _db;
    private readonly ICountriesService _countriesService;

    public PersonsService(PersonsDbContext personsDbContext, ICountriesService countriesService)
    {
        _db = personsDbContext;
        _countriesService = countriesService;
    }
    
    public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
    {
        // Check if PersonAddRequest is not null
        if (personAddRequest == null)
        {
            throw new ArgumentNullException(nameof(personAddRequest));
        }
        
        // Model validations
        ValidationHelper.ModelValidation(personAddRequest);
        
        // Convert personAddRequest into Person type
        Person person = personAddRequest.ToPerson();
        
        // Generate PersonID
        person.PersonID = Guid.NewGuid();
        
        // Add person object to persons list 
        _db.Persons.Add(person);
        _db.SaveChanges();
        // _db.sp_InsertPerson(person);

        // Convert the Person object into PersonResponse type
        return ConvertPersonToPersonResponse(person);
        
    }

    public List<PersonResponse> GetAllPersons()
    {
        return _db.Persons.ToList().Select(temp => ConvertPersonToPersonResponse(temp)).ToList();
        // return _db.sp_GetAllPersons().Select(temp => ConvertPersonToPersonResponse(temp)).ToList();
    }

    public PersonResponse? GetPersonByPersonID(Guid? personID)
    {
        if (personID == null)
        {
            return null;
        }

        Person? person = _db.Persons.FirstOrDefault(temp => temp.PersonID == personID);

        if (person == null)
        {
            return null;
        }

        return ConvertPersonToPersonResponse(person);
    }

    public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
    {
        List<PersonResponse> allPersons = GetAllPersons();
        List<PersonResponse> matchingPersons = allPersons;

        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
        {
            return matchingPersons;
        }

        switch (searchBy)
        {
            case nameof(PersonResponse.PersonName):
                matchingPersons = allPersons.Where(temp => string.IsNullOrEmpty(temp.PersonName) || temp.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            case nameof(PersonResponse.Email):
                matchingPersons = allPersons.
                    Where(temp => string.IsNullOrEmpty(temp.Email) || temp.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;

            case nameof(PersonResponse.DateOfBirth):
                matchingPersons = allPersons.
                    Where(temp => temp.DateOfBirth != null || temp.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            case nameof(PersonResponse.Gender):
                matchingPersons = allPersons.
                    Where(temp => string.IsNullOrEmpty(temp.Gender) || temp.Gender.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            case nameof(PersonResponse.CountryID):
                matchingPersons = allPersons.
                    Where(temp => string.IsNullOrEmpty(temp.Country) || temp.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            case nameof(PersonResponse.Address):
                matchingPersons = allPersons.
                    Where(temp => string.IsNullOrEmpty(temp.Address) || temp.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            default:
                matchingPersons = allPersons;
                break;
        }

        return matchingPersons;
    }

    public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPerson, string sortBy, SortOrderOptions sortOrder)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return allPerson;
        }

        List<PersonResponse> sortedPerson = (sortBy, sortOrder)
            switch
            {
                (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),

                _ => allPerson
            };

        return sortedPerson;
    }

    public PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
        if (personUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(Person));
        }
        
        // Validation
        ValidationHelper.ModelValidation(personUpdateRequest);
        
        // Get matching person object to update
        Person? matchingPerson = _db.Persons.FirstOrDefault(temp => temp.PersonID == personUpdateRequest.PersonID);
        if (matchingPerson == null)
        {
            throw new AggregateException("Given person id doesn't exist");
        }
        
        // Update all details
        matchingPerson.PersonName = personUpdateRequest.PersonName;
        matchingPerson.Email = personUpdateRequest.Email;
        matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
        matchingPerson.Gender = personUpdateRequest.Gender.ToString();
        matchingPerson.CountryID = personUpdateRequest.CountryID;
        matchingPerson.Address = personUpdateRequest.Address;
        matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

        _db.SaveChanges();
        
        return ConvertPersonToPersonResponse(matchingPerson);
    }

    public bool DeletePerson(Guid personID)
    {
        if (personID == null)
        {
            throw new ArgumentNullException(nameof(personID));
        }

        Person? person = _db.Persons.FirstOrDefault(temp => temp.PersonID == personID);
        if (person != null)
        {
            
            _db.Persons.Remove(_db.Persons.First(temp => temp.PersonID == personID));
            _db.SaveChanges();
            return true;
        }
        else
        {
            return false;
        }
    }

    private PersonResponse ConvertPersonToPersonResponse(Person person)
    {
        PersonResponse personResponse = person.ToPersonResponse();
        personResponse.Country = _countriesService.GetCountryByCountryID(person.CountryID)?.CountryName;

        return personResponse;
    }
}