using System.Linq.Expressions;
using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using Serilog;
using Serilog.Extensions.Hosting;

namespace CRUDTests;

public class PersonsServiceTest
{
    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsSorterService _personsSorterService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    
    private readonly IPersonsRepository _personsRepository;
    private readonly Mock<IPersonsRepository> _personRepositoryMock;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFixture _fixture;

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();

        _personRepositoryMock = new Mock<IPersonsRepository>();
        _personsRepository = _personRepositoryMock.Object;

        var diagnosticContextMock = new Mock<IDiagnosticContext>();
        
        var loggerMockGetterService = new Mock<ILogger<PersonsGetterService>>(); 
        var loggerMockAdderService = new Mock<ILogger<PersonsAdderService>>(); 
        var loggerMockDeleterService = new Mock<ILogger<PersonsDeleterService>>(); 
        var loggerMockSorterService = new Mock<ILogger<PersonsSorterService>>(); 
        var loggerMockUpdaterService = new Mock<ILogger<PersonsUpdaterService>>(); 
        
        
        _personsGetterService = new PersonsGetterService(_personsRepository, loggerMockGetterService.Object, diagnosticContextMock.Object);
        _personsAdderService = new PersonsAdderService(_personsRepository, loggerMockAdderService.Object, diagnosticContextMock.Object);
        _personsDeleterService = new PersonsDeleterService(_personsRepository, loggerMockDeleterService.Object, diagnosticContextMock.Object);
        _personsSorterService = new PersonsSorterService(_personsRepository, loggerMockSorterService.Object, diagnosticContextMock.Object);
        _personsUpdaterService = new PersonsUpdaterService(_personsRepository, loggerMockUpdaterService.Object, diagnosticContextMock.Object);
        
        _testOutputHelper = testOutputHelper;
    }

    #region AddPerson

    [Fact]
    //When we supply null value as PersonAddRequest,
    //it should throw ArgumentNullException
    public async Task AddPerson_NullPerson_ToBeArgumentNullException()
    {
        //Arrange
        PersonAddRequest? personAddRequest = null;
        
        //Act
        Func<Task> action = async () => await _personsAdderService.AddPerson(personAddRequest);

        await action.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    //When we supply null value as PersonName,
    //it should throw ArgumentException
    public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
    {
        // Arrange
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, null as string)
            .Create();

        Person person = personAddRequest.ToPerson();
        
        // When PersonRepository.AddPerson is called, it has to return the same "person" object
        _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
            .ReturnsAsync(person);
        
        // Assert
        Func<Task> action = async () => await _personsAdderService.AddPerson(personAddRequest);

        await action.Should().ThrowAsync<ArgumentException>();

    }
    
    [Fact]
    //When we supply proper person details, it
    //should insert the person int the persons list;
    //and it should return an object of
    //PersonResponse, which includes with newly
    //generated person id
    public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
    {
        //Arrange
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone@example.com")
            .Create();

        Person person = personAddRequest.ToPerson();
        PersonResponse person_response_expected = person.ToPersonResponse();
        
        // If we supply any argument value to the AddPerson method,
        // it should return the same return value
        _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
            .ReturnsAsync(person);
            
        //Act
        PersonResponse person_response_from_add = await _personsAdderService.AddPerson(personAddRequest);
        person_response_expected.PersonID = person_response_from_add.PersonID;
        
        // Assert
        
        person_response_from_add.Should().NotBe(Guid.Empty);
        person_response_from_add.Should().Be(person_response_expected);
    }
    
    #endregion

    #region GetPersonByPersonID

    [Fact]
    // If we supply as PersonID, it should return null as PersonResponse
    public async Task GetPersonByPersonID_NullPersonID_TeBeNull()
    {
        // Arrange
        Guid? personID = null;
        
        // Act
        PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(personID);
        
        // Assert 
        // Assert.Null(person_response_from_get);

        person_response_from_get.Should().BeNull();
    }

    [Fact]
    // If we supply a valid person id, it should return the valid
    // person details as PersonResponse object
    public async Task GetPersonByPersonID_WithPersonID_ToBeSuccessful()
    {
        // Arrange
        Person person = _fixture.Build<Person>()
            .With(temp => temp.Email, "someone@example.com")
            .With(temp => temp.Country, null as Country)
            .Create();
        PersonResponse person_response_expected = person.ToPersonResponse();

        _personRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
            .ReturnsAsync(person);

        // Act
        PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(person.PersonID);

        // Assert
        // Assert.Equal(person_response_from_add, person_response_from_get);
        person_response_from_get.Should().Be(person_response_expected);
    }
    #endregion

    #region GetAllPersons

    [Fact]
    // THe GetAllPersons() should return an empty list by default
    public async Task GetAllPersons_ToBeEmptyList()
    {
        // Arrange
        List<Person> persons = new List<Person>();
        _personRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);
        
        // Act
        List<PersonResponse> persons_from_get = await _personsGetterService.GetAllPersons();
        
        // Assert
        // Assert.Empty(persons_from_get);
        persons_from_get.Should().BeEmpty();
    }

    [Fact]
    // First, we will add few persons; and then when we can call
    // GetAllPersons(), it should return the same persons that were
    // added
    public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
    {
        // Arrange

        List<Person> persons = new List<Person>()
        {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
        };
        
        List<PersonResponse> person_response_list_expected = 
            persons.Select(temp => temp.ToPersonResponse()).ToList();

        // Print person_response_list_fron_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (var person_response_from_add in  person_response_list_expected)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }

        _personRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);
        
        // Act
        List<PersonResponse> person_list_from_get = await _personsGetterService.GetAllPersons();
        
        // Print person_response_list_fron_get
        _testOutputHelper.WriteLine("Actual:");
        foreach (var person_response_from_get in  person_list_from_get)
        {
            _testOutputHelper.WriteLine(person_response_from_get.ToString());
        }
        
        // Assert
        // foreach (var person_response_from_add in person_response_list_from_add)
        // {
        //     Assert.Contains(person_response_from_add, person_list_from_get);
        // }
        
        person_list_from_get.Should().BeEquivalentTo(person_response_list_expected);
    }

    #endregion

    #region GetFilteredPersons

    [Fact]
    // If the search text is empty and search by is "PersonName", it
    // should return all persons
    public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new List<Person>()
        {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
        };
        
        List<PersonResponse> person_response_list_expected = 
            persons.Select(temp => temp.ToPersonResponse()).ToList();
        
        // Print person_response_list_fron_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (var person_response_from_add in  person_response_list_expected)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }

        _personRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(persons);
        
        // Act
        List<PersonResponse> person_list_from_search = await 
            _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "");
        
        // Print person_response_list_fron_get
        _testOutputHelper.WriteLine("Actual:");
        foreach (var person_response_from_get in  person_list_from_search)
        {
            _testOutputHelper.WriteLine(person_response_from_get.ToString());
        }
        
        // Assert
        //foreach (var person_response_from_add in person_response_list_from_add)
        //{
        //    Assert.Contains(person_response_from_add, person_list_from_search);
        //}

        person_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
    }
    
    [Fact]
    // First we will add few persons; and then we will search based on
    // person name with some search string. It should return the matching persons
    public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new List<Person>()
        {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
        };
        
        List<PersonResponse> person_response_list_expected = 
            persons.Select(temp => temp.ToPersonResponse()).ToList();
        
        // Print person_response_list_fron_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (var person_response_from_add in  person_response_list_expected)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }

        _personRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(persons);
        
        // Act
        List<PersonResponse> person_list_from_search = await 
            _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "sa");
        
        // Print person_response_list_fron_get
        _testOutputHelper.WriteLine("Actual:");
        foreach (var person_response_from_get in  person_list_from_search)
        {
            _testOutputHelper.WriteLine(person_response_from_get.ToString());
        }
        
        // Assert
        //foreach (var person_response_from_add in person_response_list_from_add)
        //{
        //    Assert.Contains(person_response_from_add, person_list_from_search);
        //}

        person_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
    }

    #endregion

    #region GetSortedPersons

    [Fact]
    // When we sort based on PersonName in DESC, it should return
    // persons list in descending on PersonName
    public async Task GetSortedPersons_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new List<Person>()
        {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
        };
        
        List<PersonResponse> person_response_list_expected = 
            persons.Select(temp => temp.ToPersonResponse()).ToList();

        _personRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);
        
        
        // Print person_response_list_fron_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (var person_response_from_add in  person_response_list_expected)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }

        List<PersonResponse> allPersons = await _personsGetterService.GetAllPersons();
        // Act
        List<PersonResponse> person_list_from_sort = await 
            _personsSorterService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);
        
        // Print person_response_list_fron_sort
        _testOutputHelper.WriteLine("Actual:");
        foreach (var person_response_from_sort in  person_list_from_sort)
        {
            _testOutputHelper.WriteLine(person_response_from_sort.ToString());
        }

        // Assert
        
        person_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
    }

    #endregion

    #region UpdatePerson

    [Fact]
    // When we supply as PersonUpdateRequest, it should throw
    // ArgumentNullException
    public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
    {
        // Arrange
        PersonUpdateRequest? person_update_request = null;
        
        // Assert
        // await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        // {
        //     // Act
        //     await _personsService.UpdatePerson(person_update_request);
        // });

        Func<Task> action = async () =>
        {
            await _personsUpdaterService.UpdatePerson(person_update_request);
        };

        await action.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    // When we supply invalid person id, it should throw ArgumentException
    public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
    {
        // Arrange
        PersonUpdateRequest? person_update_request = _fixture.Build<PersonUpdateRequest>()
            .Create();
        
        // Assert
        // await Assert.ThrowsAsync<ArgumentException>(async () =>
        // {
        //     // Act
        //     await _personsService.UpdatePerson(person_update_request);
        // });

        Func<Task> action = async () =>
        {
            await _personsUpdaterService.UpdatePerson(person_update_request);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    // When the PersonName is null, it should throw ArgumentException
    public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
    {
        // Arrange
        
        Person person = _fixture.Build<Person>()
            .With(temp => temp.PersonName, null as string)
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.Country, null as Country)
            .With(temp => temp.Gender, "Male")
            .Create();

        PersonResponse person_response_expected = person.ToPersonResponse();
        PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

        // Assert
        // await Assert.ThrowsAsync<ArgumentException>(async () =>
        // {
        //     // Act
        //     await _personsService.UpdatePerson(person_update_request);
        // });

        Func<Task> action = async () =>
        {
            await _personsUpdaterService.UpdatePerson(person_update_request);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }
    
    [Fact]
    // First, add a new person and try to update the person name and email
    public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
    {
        // Arrange
        
        Person person = _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.Country, null as Country)
            .With(temp => temp.Gender, "Male")
            .Create();

        PersonResponse person_response_expected = person.ToPersonResponse();
        PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

        _personRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
            .ReturnsAsync(person);
        _personRepositoryMock.Setup(temp => temp.UpdatesPerson(It.IsAny<Person>()))
            .ReturnsAsync(person);
        
        // Act
        PersonResponse person_response_from_update = await _personsUpdaterService.UpdatePerson(person_update_request);
        
        // Assert
        // Assert.Equal(person_response_from_get, person_response_from_update);

        person_response_from_update.Should().Be(person_response_expected);
    }

    #endregion

    #region DeletePerson

    [Fact]
    // If you supply a valid PersonID, it should return true
    public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
    {
        // Arrange
        
        Person person = _fixture.Build<Person>()
            .With(temp => temp.PersonName, "Harsha")
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.Country, null as Country)
            .With(temp => temp.Gender, "Male")
            .Create();

        _personRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _personRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
            .ReturnsAsync(person);
        
        // Act
        bool isDeleted = await _personsDeleterService.DeletePerson(person.PersonID);
        
        // Assert
        // Assert.True(isDeleted);
       
        isDeleted.Should().BeTrue();
    }
    
    [Fact]
    // If you supply an invalid PersonID, it should return false
    public async Task DeletePerson_InvalidPersonID()
    {
        // Act
        bool isDeleted = await _personsDeleterService.DeletePerson(Guid.NewGuid());
        
        // Assert
        // Assert.False(isDeleted);

        isDeleted.Should().BeFalse();
    }
    
    #endregion
}