using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts;
using Serilog;

namespace Services;

public class PersonsDeleterService : IPersonsDeleterService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsDeleterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;
    public PersonsDeleterService(IPersonsRepository personsRepository, ILogger<PersonsDeleterService> logger,
        IDiagnosticContext diagnosticContext)
    {
        _personsRepository = personsRepository;
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }

    public async Task<bool> DeletePerson(Guid? personID)
    {
        if (personID == null)
        {
            throw new ArgumentNullException(nameof(personID));
        }

        Person? person = await _personsRepository.GetPersonByPersonID(personID.Value);
        if (person != null)
        {
            
            await _personsRepository.DeletePersonByPersonID(personID.Value);
            return true;
        }
        else
        {
            return false;
        }
    }
}