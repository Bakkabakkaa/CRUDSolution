using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;
using Serilog;

namespace Services;

public class PersonsAdderService : IPersonsAdderService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsAdderService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;
    public PersonsAdderService(IPersonsRepository personsRepository, ILogger<PersonsAdderService> logger,
        IDiagnosticContext diagnosticContext)
    {
        _personsRepository = personsRepository;
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }
    
    public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
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
        await _personsRepository.AddPerson(person);
        // _personsRepository.sp_InsertPerson(person);

        // Convert the Person object into PersonResponse type
        return person.ToPersonResponse();
    }
}