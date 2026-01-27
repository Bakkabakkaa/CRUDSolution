using ServiceContracts;

namespace CRUDTests;

public class CountriesServiceTest
{
    private readonly ICountriesService _countriesService;

    public CountriesServiceTest()
    {
        
    }
    [Fact]
    public void Test1()
    {
        //Arrange
        MyMath mm = new MyMath();
        var input1 = 10;
        var input2 = 5;
        var expected = 15;
        //Act
        var actual = mm.Add(input1, input2);

        //Assert
        Assert.Equal(expected, actual);
    }
}