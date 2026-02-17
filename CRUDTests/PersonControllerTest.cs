using AutoFixture;
using Moq;
using ServiceContracts;

namespace CRUDTests;

public class PersonControllerTest
{
    private readonly IPersonsService _personsService;
    private readonly ICountriesService _countriesService;
    private readonly Mock<ICountriesService> _countriesServiceMock;
    private readonly Mock<IPersonsService> _personsServiceMock;

    private readonly Fixture _fixture;

    public PersonControllerTest()
    {
        _fixture = new Fixture();
        
        _countriesServiceMock = new Mock<ICountriesService>();
        _personsServiceMock = new Mock<IPersonsService>();

        _countriesService = _countriesServiceMock.Object;
        _personsService = _personsServiceMock.Object;
    }
}

