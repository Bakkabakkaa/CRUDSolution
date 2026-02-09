using System.ComponentModel.DataAnnotations;

namespace Entities;

/// <summary>
/// Domain Model for Country
/// </summary>
public class Country
{
    [Key]
    public Guid CountryID { get; set; }
    
    [StringLength(30)]
    public string? CountryName { get; set; }
}