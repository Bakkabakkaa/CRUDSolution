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

namespace CRUDTests;

public class PersonsServiceTests
{
    private readonly IPersonsService _personsService;
    private readonly ICountriesService _countriesService;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFixture _fixture;

    public PersonsServiceTests(ITestOutputHelper testOutputHelper)
    {
        List<Person> personsInitialData = new List<Person>() { };
        List<Country> countriesInitialData = new List<Country>() { };

        DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
            new DbContextOptionsBuilder<ApplicationDbContext>().Options);

        ApplicationDbContext dbContext = dbContextMock.Object;
        dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);
        dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
        
        _countriesService = new CountriesService(null);
        _personsService = new PersonsService(null);
        
        _testOutputHelper = testOutputHelper;
        _fixture = new Fixture();
    }

    #region AddPerson

    [Fact]
    //When we supply null value as PersonAddRequest,
    //it should throw ArgumentNullException
    public async Task AddPerson_NullPerson()
    {
        //Arrange
        PersonAddRequest? personAddRequest = null;
        
        //Act
        Func<Task> action = async () => await _personsService.AddPerson(personAddRequest);

        await action.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    //When we supply null value as PersonName,
    //it should throw ArgumentException
    public async Task AddPerson_PersonNameIsNull()
    {
        // Arrange
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, null as string)
            .Create();
        
        // Assert
        Func<Task> action = async () => await _personsService.AddPerson(personAddRequest);

        await action.Should().ThrowAsync<ArgumentException>();

    }
    
    [Fact]
    //When we supply proper person details, it
    //should insert the person int the persons list;
    //and it should return an object of
    //PersonResponse, which includes with newly
    //generated person id
    public async Task AddPerson_ProperPersonDetails()
    {
        //Arrange
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone@example.com")
            .Create(); 
            
        //Act
        PersonResponse person_response_from_add = await _personsService.AddPerson(personAddRequest);
        List<PersonResponse> person_list = await _personsService.GetAllPersons();
        
        // Assert
        // Assert.True(person_response_from_add.PersonID != Guid.Empty);
        // Assert.Contains(person_response_from_add, person_list);

        person_response_from_add.Should().NotBe(Guid.Empty);
        person_list.Should().Contain(person_response_from_add);
    }
    
    #endregion

    #region GetPersonByPersonID

    [Fact]
    // If we supply as PersonID, it should return null as PersonResponse
    public async Task GetPersonByPersonID_NullPersonID()
    {
        // Arrange
        Guid? personID = null;
        
        // Act
        PersonResponse? person_response_from_get = await _personsService.GetPersonByPersonID(personID);
        
        // Assert 
        // Assert.Null(person_response_from_get);

        person_response_from_get.Should().BeNull();
    }

    [Fact]
    // If we supply a valid person id, it should return the valid
    // person details as PersonResponse object
    public async Task GetPersonByPersonID_WithPersonID()
    {
        // Arrange
        CountryAddRequest country_request = _fixture.Create<CountryAddRequest>();
        CountryResponse country_response = await _countriesService.AddCountry(country_request);

        // Act
        PersonAddRequest person_request = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone@example.com")
            .Create(); 

        PersonResponse person_response_from_add = await _personsService.AddPerson(person_request);
        PersonResponse? person_response_from_get = await _personsService.GetPersonByPersonID(person_response_from_add.PersonID);

        // Assert
        // Assert.Equal(person_response_from_add, person_response_from_get);
        person_response_from_get.Should().Be(person_response_from_add);
    }
    #endregion

    #region GetAllPersons

    [Fact]
    // THe GetAllPersons() should return an empty list by default
    public async Task GetAllPersons_EmptyList()
    {
        // Act
        List<PersonResponse> persons_from_get = await _personsService.GetAllPersons();
        
        // Assert
        // Assert.Empty(persons_from_get);
        persons_from_get.Should().BeEmpty();
    }

    [Fact]
    // First, we will add few persons; and then when we can call
    // GetAllPersons(), it should return the same persons that were
    // added
    public async Task GetAllPersons_AddFewPersons()
    {
        // Arrange
        CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
        CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();

        CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone1@example.com")
            .Create(); 
        
        PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone2@example.com")
            .Create(); 
        
        PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone3@example.com")
            .Create(); 

        List<PersonAddRequest> person_requests = new List<PersonAddRequest>()
        {
            person_request_1, person_request_2, person_request_3
        };

        List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

        foreach (var person_request in person_requests)
        {
            PersonResponse person_response = await _personsService.AddPerson(person_request);
            
            person_response_list_from_add.Add(person_response);
        }
        
        // Print person_response_list_fron_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (var person_response_from_add in  person_response_list_from_add)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }
        
        // Act
        List<PersonResponse> person_list_from_get = await _personsService.GetAllPersons();
        
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
        
        person_list_from_get.Should().BeEquivalentTo(person_response_list_from_add);
    }

    #endregion

    #region GetFilteredPersons

    [Fact]
    // If the search text is empty and search by is "PersonName", it
    // should return all persons
    public async Task GetFilteredPersons_EmptySearchText()
    {
        // Arrange
        CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
        CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();

        CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone1@example.com")
            .Create(); 
        
        PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone2@example.com")
            .Create(); 
        
        PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone3@example.com")
            .Create(); 

        List<PersonAddRequest> person_requests = new List<PersonAddRequest>()
        {
            person_request_1, person_request_2, person_request_3
        };

        List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

        foreach (var person_request in person_requests)
        {
            PersonResponse person_response = await _personsService.AddPerson(person_request);
            
            person_response_list_from_add.Add(person_response);
        }
        
        // Print person_response_list_fron_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (var person_response_from_add in  person_response_list_from_add)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }
        
        // Act
        List<PersonResponse> person_list_from_search = await 
            _personsService.GetFilteredPersons(nameof(Person.PersonName), "");
        
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

        person_list_from_search.Should().BeEquivalentTo(person_response_list_from_add);
    }
    
    [Fact]
    // First we will add few persons; and then we will search based on
    // person name with some search string. It should return the matching persons
    public async Task GetFilteredPersons_SearchByPersonName()
    {
        // Arrange
        CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
        CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();

        CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Masha")
            .With(temp => temp.Email, "someone1@example.com")
            .With(temp => temp.CountryID, country_response_1.CountryID)
            .Create(); 
        
        PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Mary")
            .With(temp => temp.Email, "someone2@example.com")
            .With(temp => temp.CountryID, country_response_2.CountryID)
            .Create(); 
        
        PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Scott")
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.CountryID, country_response_2.CountryID)
            .Create(); 

        List<PersonAddRequest> person_requests = new List<PersonAddRequest>()
        {
            person_request_1, person_request_2, person_request_3
        };

        List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

        foreach (var person_request in person_requests)
        {
            PersonResponse person_response = await _personsService.AddPerson(person_request);
            
            person_response_list_from_add.Add(person_response);
        }
        
        // Print person_response_list_fron_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (var person_response_from_add in  person_response_list_from_add)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }
        
        // Act
        List<PersonResponse> person_list_from_search = await 
            _personsService.GetFilteredPersons(nameof(Person.PersonName), "ma");
        
        // Print person_response_list_fron_get
        _testOutputHelper.WriteLine("Actual:");
        foreach (var person_response_from_get in  person_list_from_search)
        {
            _testOutputHelper.WriteLine(person_response_from_get.ToString());
        }
        
        // Assert
        //foreach (var person_response_from_add in person_response_list_from_add)
        //{
        //    if (person_response_from_add.PersonName != null)
        //    {
        //        if (person_response_from_add.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
        //        {
        //            Assert.Contains(person_response_from_add, person_list_from_search);
        //        }
        //    } 
        //}

        person_list_from_search.Should().OnlyContain(temp => temp.PersonName
            .Contains("ma", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region GetSortedPersons

    [Fact]
    // When we sort based on PersonName in DESC, it should return
    // persons list in descending on PersonName
    public async Task GetSortedPersons_DESC()
    {
        // Arrange
        CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
        CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();

        CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Smith")
            .With(temp => temp.Email, "someone1@example.com")
            .With(temp => temp.CountryID, country_response_1.CountryID)
            .Create(); 
        
        PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Mary")
            .With(temp => temp.Email, "someone2@example.com")
            .With(temp => temp.CountryID, country_response_2.CountryID)
            .Create(); 
        
        PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Harsha")
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.CountryID, country_response_2.CountryID)
            .Create(); 

        List<PersonAddRequest> person_requests = new List<PersonAddRequest>()
        {
            person_request_1, person_request_2, person_request_3
        };

        List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

        foreach (var person_request in person_requests)
        {
            PersonResponse person_response = await _personsService.AddPerson(person_request);
            
            person_response_list_from_add.Add(person_response);
        }
        
        // Print person_response_list_fron_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (var person_response_from_add in  person_response_list_from_add)
        {
            _testOutputHelper.WriteLine(person_response_from_add.ToString());
        }

        List<PersonResponse> allPersons = await _personsService.GetAllPersons();
        // Act
        List<PersonResponse> person_list_from_sort = await 
            _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);
        
        // Print person_response_list_fron_sort
        _testOutputHelper.WriteLine("Actual:");
        foreach (var person_response_from_sort in  person_list_from_sort)
        {
            _testOutputHelper.WriteLine(person_response_from_sort.ToString());
        }

        // person_response_list_from_add = person_response_list_from_add.OrderByDescending(temp => temp.PersonName).ToList();
        
        // Assert
        // for (int i = 0; i < person_response_list_from_add.Count; i++)
        // {
        //     Assert.Equal(person_response_list_from_add[i], person_list_from_sort[i]);
        // }

        // person_list_from_sort.Should().BeEquivalentTo(person_response_list_from_add);

        person_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
    }

    #endregion

    #region UpdatePerson

    [Fact]
    // When we supply as PersonUpdateRequest, it should throw
    // ArgumentNullException
    public async Task UpdatePerson_NullPerson()
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
            await _personsService.UpdatePerson(person_update_request);
        };

        await action.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    // When we supply invalid person id, it should throw ArgumentException
    public async Task UpdatePerson_InvalidPersonID()
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
            await _personsService.UpdatePerson(person_update_request);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    // When the PersonName is null, it should throw ArgumentException
    public async Task UpdatePerson_PersonNameIsNull()
    {
        // Arrange
        CountryAddRequest country_add_request = _fixture.Create<CountryAddRequest>();
        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);
        
        PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Harsha")
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.CountryID, country_response_from_add.CountryID)
            .Create(); 
        
        PersonResponse person_response_from_add = await _personsService.AddPerson(person_add_request);
        PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
        person_update_request.PersonName = null;

        // Assert
        // await Assert.ThrowsAsync<ArgumentException>(async () =>
        // {
        //     // Act
        //     await _personsService.UpdatePerson(person_update_request);
        // });

        Func<Task> action = async () =>
        {
            await _personsService.UpdatePerson(person_update_request);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }
    
    [Fact]
    // First, add a new person and try to update the person name and email
    public async Task UpdatePerson_PersonFullDetailsUpdation()
    {
        // Arrange
        CountryAddRequest country_add_request = _fixture.Create<CountryAddRequest>();
        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);
        
        PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Harsha")
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.CountryID, country_response_from_add.CountryID)
            .Create(); 
        
        PersonResponse person_response_from_add = await _personsService.AddPerson(person_add_request);
        PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
        person_update_request.PersonName = "William";
        person_update_request.Email = "william@example.com";
        
        // Act
        PersonResponse person_response_from_update = await _personsService.UpdatePerson(person_update_request);
        PersonResponse? person_response_from_get = await 
            _personsService.GetPersonByPersonID(person_response_from_update.PersonID);
        
        // Assert
        // Assert.Equal(person_response_from_get, person_response_from_update);

        person_response_from_update.Should().Be(person_response_from_get);
    }

    #endregion

    #region DeletePerson

    [Fact]
    // If you supply a valid PersonID, it should return true
    public async Task DeletePerson_ValidPersonID()
    {
        // Arrange
        CountryAddRequest country_add_request = _fixture.Create<CountryAddRequest>();
        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);
        
        PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Harsha")
            .With(temp => temp.Email, "someone3@example.com")
            .With(temp => temp.CountryID, country_response_from_add.CountryID)
            .Create(); 
        
        PersonResponse person_response_from_add = await _personsService.AddPerson(person_add_request);
        
        // Act
        bool isDeleted = await _personsService.DeletePerson(person_response_from_add.PersonID);
        
        // Assert
        // Assert.True(isDeleted);
       
        isDeleted.Should().BeTrue();
    }
    
    [Fact]
    // If you supply an invalid PersonID, it should return false
    public async Task DeletePerson_InvalidPersonID()
    {
        // Act
        bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());
        
        // Assert
        // Assert.False(isDeleted);

        isDeleted.Should().BeFalse();
    }
    
    #endregion
}