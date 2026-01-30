using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit;

namespace CRUDTests;

public class PersonsServiceTests
{
    private readonly IPersonsService _personsService;
    private readonly ICountriesService _countriesService;

    public PersonsServiceTests()
    {
        _personsService = new PersonsService();
        _countriesService = new CountriesService();
    }

    #region AddPerson

    [Fact]
    //When we supply null value as PersonAddRequest,
    //it should throw ArgumentNullException
    public void AddPerson_NullPerson()
    {
        //Arrange
        PersonAddRequest? personAddRequest = null;
        
        //Act
        Assert.Throws<ArgumentNullException>(() => _personsService.AddPerson(personAddRequest));
    }
    
    [Fact]
    //When we supply null value as PersonName,
    //it should throw ArgumentException
    public void AddPerson_PersonNameIsNull()
    {
        //Arrange
        PersonAddRequest? personAddRequest = new PersonAddRequest()
        {
            PersonName = null
        };
        
        //Act
        Assert.Throws<ArgumentException>(() => _personsService.AddPerson(personAddRequest));
    }
    
    [Fact]
    //When we supply proper person details, it
    //should insert the person int the persons list;
    //and it should return an object of
    //PersonResponse, which includes with newly
    //generated person id
    public void AddPerson_ProperPersonDetails()
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
        PersonResponse person_response_from_add = _personsService.AddPerson(personAddRequest);
        List<PersonResponse> person_list = _personsService.GetAllPersons();
        
        //Assert
        Assert.True(person_response_from_add.PersonID != Guid.Empty);
        Assert.Contains(person_response_from_add, person_list);
    }
    
    #endregion

    #region GetPersonByPersonID

    [Fact]
    // If we supply as PersonID, it should return null as PersonResponse
    public void GetPersonByPersonID_NullPersonID()
    {
        // Arrange
        Guid? personID = null;
        
        // Act
        PersonResponse? person_response_from_get = _personsService.GetPersonByPersonID(personID);
        
        // Assert 
        Assert.Null(person_response_from_get);
    }

    [Fact]
    // If we supply a valid person id, it should return the valid
    // person details as PersonResponse object
    public void GetPersonByPersonID_WithPersonID()
    {
        // Arrange
        CountryAddRequest country_request = new CountryAddRequest()
        {
            CountryName = "Canada"
        };
        CountryResponse country_response = _countriesService.AddCountry(country_request);

        // Act
        PersonAddRequest person_request = new PersonAddRequest()
        {
            PersonName = "Person", Email = "person@example.com",
            Address = "sample address", CountryID = country_response.CountryID,
            Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"),
            ReceiveNewsLetters = true
        };

        PersonResponse person_response_from_add = _personsService.AddPerson(person_request);
        PersonResponse? person_response_from_get = _personsService.GetPersonByPersonID(person_response_from_add.PersonID);

        // Assert
        Assert.Equal(person_response_from_add, person_response_from_get);
    }
    #endregion
}