using AutoFixture;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Moq;

namespace CRUDTests;

public class CountriesServiceTest
{
    private readonly ICountriesService _countriesService;
    private readonly IFixture _fixture;

    public CountriesServiceTest()
    {
        List<Country> countriesInitialData = new List<Country>() { };

        DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
            new DbContextOptionsBuilder<ApplicationDbContext>().Options);

        ApplicationDbContext dbContext = dbContextMock.Object;
        dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

        _countriesService = new CountriesService(null);
        _fixture = new Fixture();
    }
    
    #region AddCountry
    [Fact]
    //When CountryAddRequest is null, it should ArgumentNullException
    public async Task AddCountry_NullCountry()
    {
        //Arrange
        CountryAddRequest? request = null;
        //Assert
        // await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        // {
        //     //Act
        //     await _countriesService.AddCountry(request);
        // });

        Func<Task> action = async () =>
        {
            await _countriesService.AddCountry(request);
        };

        await action.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    //When the CountryName is null, it should throw ArgumentException
    public async Task AddCountry_CountryNameIsNull()
    {
        //Arrange
        CountryAddRequest? request = _fixture.Build<CountryAddRequest>()
            .With(temp => temp.CountryName, null as string)
            .Create();
        // Assert
        // await Assert.ThrowsAsync<ArgumentException>(async () =>
        // {
        //     //Act
        //     await _countriesService.AddCountry(request);
        // });

        Func<Task> action = async () =>
        {
            await _countriesService.AddCountry(request);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }
    
    [Fact]
    //When the CountryName is duplicate, it should throw ArgumentException
    public async Task AddCountry_DuplicateCountryName()
    {
        //Arrange
        CountryAddRequest? request1 = _fixture.Build<CountryAddRequest>()
            .With(temp => temp.CountryName, "USA")
            .Create();
        
        CountryAddRequest? request2 = _fixture.Build<CountryAddRequest>()
            .With(temp => temp.CountryName, "USA")
            .Create();
        //Assert
        // await Assert.ThrowsAsync<ArgumentException>(async () =>
        // {
        //     //Act
        //     await _countriesService.AddCountry(request1);
        //     await _countriesService.AddCountry(request2);
        // });

        Func<Task> action = async () =>
        {
            await _countriesService.AddCountry(request1);
            await _countriesService.AddCountry(request2);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }
    
    [Fact]
    //When you supply proper country name, it should insert (add) the country to the existing list of countries
    public async Task AddCountry_ProperCountryDetails()
    {
        //Arrange
        CountryAddRequest? request = _fixture.Create<CountryAddRequest>();
        
        //Act
        CountryResponse response = await _countriesService.AddCountry(request);
        List<CountryResponse> countries_from_GetAllCountries = await _countriesService.GetAllCountries();
        
        //Assert
        // Assert.True(response.CountryID != Guid.Empty);
        // Assert.Contains(response, countries_from_GetAllCountries);

        response.CountryID.Should().NotBe(Guid.Empty);
        countries_from_GetAllCountries.Should().Contain(response);
    }
    #endregion

    #region GetAllCountries
    
    [Fact]
    //The list of countries should be empty by default (before adding any countries)
    public async Task GetAllCountries_EmptyList()
    {
        //Acts
        List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();
        
        //Assert
        // Assert.Empty(actual_country_response_list);

        actual_country_response_list.Should().BeEmpty();
    }
    
    [Fact]
    //
    public async Task GetAllCountries_AddFewCountries()
    {
        //Arrange
        List<CountryAddRequest> country_request_list = new List<CountryAddRequest>()
        {
            _fixture.Create<CountryAddRequest>(),
            _fixture.Create<CountryAddRequest>()
        };
        
        //Act
        List<CountryResponse> country_list_from_add_country = new List<CountryResponse>();
        foreach (var country_request in country_request_list)
        {
            country_list_from_add_country.Add(await _countriesService.AddCountry(country_request));
        }

        List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();
        
        //read each from countries_list_from_add_country
        // foreach (var expected_country in country_list_from_add_country)
        // {
        //     Assert.Contains(expected_country, actualCountryResponseList);
        // }

        actualCountryResponseList.Should().BeEquivalentTo(country_list_from_add_country);
    }

    #endregion

    #region GetCountryByCountryID

    [Fact]
    //If we supply null as CountryID, it should return null as CountryResponse
    public async Task GetCountryByCountryID_NullCountryID()
    {
        //Arrange
        Guid? countryID = null;
        
        //Act
        CountryResponse? country_response_from_get_method = await _countriesService.GetCountryByCountryID(countryID);
        
        //Assert
        // Assert.Null(country_response_from_get_method);

        country_response_from_get_method.Should().BeNull();
    }

    [Fact]
    //If we supply a valid country id, it should return the matching country details as CountryResponse object
    public async Task GetCountryByCountryID_ValidCountryID()
    {
        //Arrange
        CountryAddRequest country_add_request = _fixture.Create<CountryAddRequest>();

        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);
        
        //Act
        CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryID(country_response_from_add.CountryID);
        
        //Assert
        // Assert.Equal(country_response_from_add, country_response_from_get);

        country_response_from_get.Should().Be(country_response_from_add);
    }
    #endregion
    
}