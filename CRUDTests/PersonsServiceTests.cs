using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit;
using Xunit.Abstractions;

namespace CRUDTests;

public class PersonsServiceTests
{
    private readonly IPersonsService _personsService;
    private readonly ICountriesService _countriesService;
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonsServiceTests(ITestOutputHelper testOutputHelper)
    {
        _countriesService = new CountriesService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options));
        _personsService = new PersonsService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options), _countriesService);
        _testOutputHelper = testOutputHelper;
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
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _personsService.AddPerson(personAddRequest));
    }
    
    [Fact]
    //When we supply null value as PersonName,
    //it should throw ArgumentException
    public async Task AddPerson_PersonNameIsNull()
    {
        //Arrange
        PersonAddRequest? personAddRequest = new PersonAddRequest()
        {
            PersonName = null
        };
        
        //Act
        await Assert.ThrowsAsync<ArgumentException>(async () => await _personsService.AddPerson(personAddRequest));
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
        PersonAddRequest? personAddRequest = new PersonAddRequest()
        {
            PersonName = "Person", Email = "person@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"),
            ReceiveNewsLetters = true
        };
        
        //Act
        PersonResponse person_response_from_add = await _personsService.AddPerson(personAddRequest);
        List<PersonResponse> person_list = await _personsService.GetAllPersons();
        
        //Assert
        Assert.True(person_response_from_add.PersonID != Guid.Empty);
        Assert.Contains(person_response_from_add, person_list);
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
        Assert.Null(person_response_from_get);
    }

    [Fact]
    // If we supply a valid person id, it should return the valid
    // person details as PersonResponse object
    public async Task GetPersonByPersonID_WithPersonID()
    {
        // Arrange
        CountryAddRequest country_request = new CountryAddRequest()
        {
            CountryName = "Canada"
        };
        CountryResponse country_response = await _countriesService.AddCountry(country_request);

        // Act
        PersonAddRequest person_request = new PersonAddRequest()
        {
            PersonName = "Person", Email = "person@example.com",
            Address = "sample address", CountryID = country_response.CountryID,
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"),
            ReceiveNewsLetters = true
        };

        PersonResponse person_response_from_add = await _personsService.AddPerson(person_request);
        PersonResponse? person_response_from_get = await _personsService.GetPersonByPersonID(person_response_from_add.PersonID);

        // Assert
        Assert.Equal(person_response_from_add, person_response_from_get);
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
        Assert.Empty(persons_from_get);
    }

    [Fact]
    // First, we will add few persons; and then when we can call
    // GetAllPersons(), it should return the same persons that were
    // added
    public async Task GetAllPersons_AddFewPersons()
    {
        // Arrange
        CountryAddRequest country_request_1 = new CountryAddRequest() { CountryName = "USA" };
        CountryAddRequest country_request_2 = new CountryAddRequest() { CountryName = "India" };

        CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = new PersonAddRequest()
        {
            PersonName = "Smith", Email = "smith@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"),
            ReceiveNewsLetters = true
        };
        
        PersonAddRequest person_request_2 = new PersonAddRequest()
        {
            PersonName = "Mary", Email = "mary@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Female, DateOfBirth = DateTime.Parse("2003-01-01"),
            ReceiveNewsLetters = true
        };
        
        PersonAddRequest person_request_3 = new PersonAddRequest()
        {
            PersonName = "Rahman", Email = "rahman@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("1999-01-01"),
            ReceiveNewsLetters = true
        };

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
        foreach (var person_response_from_add in person_response_list_from_add)
        {
            Assert.Contains(person_response_from_add, person_list_from_get);
        }
    }

    #endregion

    #region GetFilteredPersons

    [Fact]
    // If the search text is empty and search by is "PersonName", it
    // should return all persons
    public async Task GetFilteredPersons_EmptySearchText()
    {
        // Arrange
        CountryAddRequest country_request_1 = new CountryAddRequest() { CountryName = "USA" };
        CountryAddRequest country_request_2 = new CountryAddRequest() { CountryName = "India" };

        CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = new PersonAddRequest()
        {
            PersonName = "Smith", Email = "smith@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"),
            ReceiveNewsLetters = true
        };
        
        PersonAddRequest person_request_2 = new PersonAddRequest()
        {
            PersonName = "Mary", Email = "mary@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Female, DateOfBirth = DateTime.Parse("2003-01-01"),
            ReceiveNewsLetters = true
        };
        
        PersonAddRequest person_request_3 = new PersonAddRequest()
        {
            PersonName = "Rahman", Email = "rahman@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("1999-01-01"),
            ReceiveNewsLetters = true
        };

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
        foreach (var person_response_from_add in person_response_list_from_add)
        {
            Assert.Contains(person_response_from_add, person_list_from_search);
        }
    }
    
    [Fact]
    // First we will add few persons; and then we will search based on
    // person name with some search string. It should return the matching persons
    public async Task GetFilteredPersons_SearchByPersonName()
    {
        // Arrange
        CountryAddRequest country_request_1 = new CountryAddRequest() { CountryName = "USA" };
        CountryAddRequest country_request_2 = new CountryAddRequest() { CountryName = "India" };

        CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = new PersonAddRequest()
        {
            PersonName = "Smith", Email = "smith@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"),
            ReceiveNewsLetters = true
        };
        
        PersonAddRequest person_request_2 = new PersonAddRequest()
        {
            PersonName = "Mary", Email = "mary@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Female, DateOfBirth = DateTime.Parse("2003-01-01"),
            ReceiveNewsLetters = true
        };
        
        PersonAddRequest person_request_3 = new PersonAddRequest()
        {
            PersonName = "Rahman", Email = "rahman@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("1999-01-01"),
            ReceiveNewsLetters = true
        };

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
        foreach (var person_response_from_add in person_response_list_from_add)
        {
            if (person_response_from_add.PersonName != null)
            {
                if (person_response_from_add.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.Contains(person_response_from_add, person_list_from_search);
                }
            }
        }
    }

    #endregion

    #region GetSortedPersons

    [Fact]
    // When we sort based on PersonName in DESC, it should return
    // persons list in descending on PersonName
    public async Task GetSortedPersons_DESC()
    {
        // Arrange
        CountryAddRequest country_request_1 = new CountryAddRequest() { CountryName = "USA" };
        CountryAddRequest country_request_2 = new CountryAddRequest() { CountryName = "India" };

        CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = new PersonAddRequest()
        {
            PersonName = "Smith", Email = "smith@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"),
            ReceiveNewsLetters = true
        };
        
        PersonAddRequest person_request_2 = new PersonAddRequest()
        {
            PersonName = "Mary", Email = "mary@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Female, DateOfBirth = DateTime.Parse("2003-01-01"),
            ReceiveNewsLetters = true
        };
        
        PersonAddRequest person_request_3 = new PersonAddRequest()
        {
            PersonName = "Rahman", Email = "rahman@example.com",
            Address = "sample address", CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("1999-01-01"),
            ReceiveNewsLetters = true
        };

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

        person_response_list_from_add = person_response_list_from_add.OrderByDescending(temp => temp.PersonName).ToList();
        
        // Assert
        for (int i = 0; i < person_response_list_from_add.Count; i++)
        {
            Assert.Equal(person_response_list_from_add[i], person_list_from_sort[i]);
        }
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
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            // Act
            await _personsService.UpdatePerson(person_update_request);
        });
    }
    
    [Fact]
    // When we supply invalid person id, it should throw ArgumentException
    public async Task UpdatePerson_InvalidPersonID()
    {
        // Arrange
        PersonUpdateRequest? person_update_request = new PersonUpdateRequest()
        {
            PersonID = new Guid()
        };
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            // Act
            await _personsService.UpdatePerson(person_update_request);
        });
    }

    [Fact]
    // When the PersonName is null, it should throw ArgumentException
    public async Task UpdatePerson_PersonNameIsNull()
    {
        // Arrange
        CountryAddRequest country_add_request = new CountryAddRequest()
        {
            CountryName = "USA"
        };
        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);
        PersonAddRequest person_add_request = new PersonAddRequest()
        {
            PersonName = "Smith", CountryID = country_response_from_add.CountryID,
            Email = "smith@example.com", Address = "address..", Gender = GenderOptions.Male
        };
        PersonResponse person_response_from_add = await _personsService.AddPerson(person_add_request);
        PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
        person_update_request.PersonName = null;

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            // Act
            await _personsService.UpdatePerson(person_update_request);
        });
    }
    
    [Fact]
    // First, add a new person and try to update the person name and email
    public async Task UpdatePerson_PersonFullDetailsUpdation()
    {
        // Arrange
        CountryAddRequest country_add_request = new CountryAddRequest()
        {
            CountryName = "USA"
        };
        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);
        PersonAddRequest person_add_request = new PersonAddRequest()
        {
            PersonName = "Smith", CountryID = country_response_from_add.CountryID,
            Address = "Abc road", DateOfBirth = DateTime.Parse("2000-01-01"),
            Email = "abc@example.com", Gender = GenderOptions.Male,
            ReceiveNewsLetters = true
        };
        PersonResponse person_response_from_add = await _personsService.AddPerson(person_add_request);
        PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
        person_update_request.PersonName = "William";
        person_update_request.Email = "william@example.com";
        
        // Act
        PersonResponse person_response_from_update = await _personsService.UpdatePerson(person_update_request);
        PersonResponse? person_response_from_get = await 
            _personsService.GetPersonByPersonID(person_response_from_update.PersonID);
        
        // Assert
        Assert.Equal(person_response_from_get, person_response_from_update);

    }

    #endregion

    #region DeletePerson

    [Fact]
    // If you supply a valid PersonID, it should return true
    public async Task DeletePerson_ValidPersonID()
    {
        // Arrange
        CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "USA" };
        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

        PersonAddRequest person_add_request = new PersonAddRequest()
        {
            PersonName = "Jones", Address = "address", CountryID = country_response_from_add.CountryID,
            DateOfBirth = Convert.ToDateTime("2010-01-01"), Email = "jones@example.com",
            Gender = GenderOptions.Male, ReceiveNewsLetters = true
        };
        PersonResponse person_response_from_add = await _personsService.AddPerson(person_add_request);
        
        // Act
        bool isDeleted = await _personsService.DeletePerson(person_response_from_add.PersonID);
        
        // Assert
        Assert.True(isDeleted);
    }
    
    [Fact]
    // If you supply an invalid PersonID, it should return false
    public async Task DeletePerson_InvalidPersonID()
    {
        // Act
        bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());
        
        // Assert
        Assert.False(isDeleted);
    }
    
    #endregion
}