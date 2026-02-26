using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts;

/// <summary>
/// Represents business logic for manipulating
/// Person entity 
/// </summary>
public interface IPersonsSorterService
{
    /// <summary>
    /// Returns sorted list of persons
    /// </summary>
    /// <param name="allPerson">Represents list of persons to sort</param>
    /// <param name="sortBy">Name of the property (key), based on which
    /// the persons should be sorted</param>
    /// <param name="sortOrder">ASC or DESC</param>
    /// <returns>Returns sorted persons as PersonResponse list</returns>
    Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPerson, string sortBy, SortOrderOptions sortOrder);
}