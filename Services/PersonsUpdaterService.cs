using Entities;
using Exceptions;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;
using Serilog;

namespace Services;

public class PersonsUpdaterService : IPersonsUpdaterService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsUpdaterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;
    public PersonsUpdaterService(IPersonsRepository personsRepository, ILogger<PersonsUpdaterService> logger,
        IDiagnosticContext diagnosticContext)
    {
        _personsRepository = personsRepository;
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }
    
    public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
        if (personUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(Person));
        }
        
        // Validation
        ValidationHelper.ModelValidation(personUpdateRequest);
        
        // Get matching person object to update
        Person? matchingPerson = await _personsRepository.GetPersonByPersonID(personUpdateRequest.PersonID);
        if (matchingPerson == null)
        {
            throw new InvalidPersonIDException("Given person id doesn't exist");
        }
        
        // Update all details
        matchingPerson.PersonName = personUpdateRequest.PersonName;
        matchingPerson.Email = personUpdateRequest.Email;
        matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
        matchingPerson.Gender = personUpdateRequest.Gender.ToString();
        matchingPerson.CountryID = personUpdateRequest.CountryID;
        matchingPerson.Address = personUpdateRequest.Address;
        matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

        await _personsRepository.UpdatesPerson(matchingPerson);
        
        return matchingPerson.ToPersonResponse();
    }
}