using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private readonly ApplicationDbContext _db;
    private readonly ICountriesService _countriesService;
    public PersonsService(ApplicationDbContext applicationDbContext, ICountriesService countriesService)
    {
        _db = applicationDbContext;
        _countriesService = countriesService;
    }
    
    public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
    {
        // Check if PersonAddRequest is not null
        if (personAddRequest == null)
        {
            throw new ArgumentNullException(nameof(personAddRequest));
        }
        
        // Model validations
        ValidationHelper.ModelValidation(personAddRequest);
        
        // Convert personAddRequest into Person type
        Person person = personAddRequest.ToPerson();
        
        // Generate PersonID
        person.PersonID = Guid.NewGuid();
        
        // Add person object to persons list 
        _db.Persons.Add(person);
        await _db.SaveChangesAsync();
        // _db.sp_InsertPerson(person);

        // Convert the Person object into PersonResponse type
        return person.ToPersonResponse();
        
    }

    public async Task<List<PersonResponse>> GetAllPersons()
    {
        var persons = await _db.Persons.Include("Country").ToListAsync();
        
        return persons.Select(temp => temp.ToPersonResponse()).ToList();
        // return _db.sp_GetAllPersons().Select(temp => ConvertPersonToPersonResponse(temp)).ToList();
    }

    public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
    {
        if (personID == null)
        {
            return null;
        }

        Person? person = await _db.Persons.Include("Country").FirstOrDefaultAsync(temp => temp.PersonID == personID);

        if (person == null)
        {
            return null;
        }

        return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
    {
        List<PersonResponse> allPersons = await GetAllPersons();
        List<PersonResponse> matchingPersons = allPersons;

        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
        {
            return matchingPersons;
        }

        switch (searchBy)
        {
            case nameof(PersonResponse.PersonName):
                matchingPersons = allPersons.Where(temp => string.IsNullOrEmpty(temp.PersonName) || temp.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            case nameof(PersonResponse.Email):
                matchingPersons = allPersons.
                    Where(temp => string.IsNullOrEmpty(temp.Email) || temp.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;

            case nameof(PersonResponse.DateOfBirth):
                matchingPersons = allPersons.
                    Where(temp => temp.DateOfBirth != null || temp.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            case nameof(PersonResponse.Gender):
                matchingPersons = allPersons.
                    Where(temp => string.IsNullOrEmpty(temp.Gender) || temp.Gender.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            case nameof(PersonResponse.CountryID):
                matchingPersons = allPersons.
                    Where(temp => string.IsNullOrEmpty(temp.Country) || temp.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            case nameof(PersonResponse.Address):
                matchingPersons = allPersons.
                    Where(temp => string.IsNullOrEmpty(temp.Address) || temp.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                break;
            
            default:
                matchingPersons = allPersons;
                break;
        }

        return matchingPersons;
    }

    public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPerson, string sortBy, SortOrderOptions sortOrder)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return allPerson;
        }

        List<PersonResponse> sortedPerson = (sortBy, sortOrder)
            switch
            {
                (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) =>
                    allPerson.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) =>
                    allPerson.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),

                _ => allPerson
            };

        return sortedPerson;
    }

    public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
        if (personUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(Person));
        }
        
        // Validation
        ValidationHelper.ModelValidation(personUpdateRequest);
        
        // Get matching person object to update
        Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == personUpdateRequest.PersonID);
        if (matchingPerson == null)
        {
            throw new AggregateException("Given person id doesn't exist");
        }
        
        // Update all details
        matchingPerson.PersonName = personUpdateRequest.PersonName;
        matchingPerson.Email = personUpdateRequest.Email;
        matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
        matchingPerson.Gender = personUpdateRequest.Gender.ToString();
        matchingPerson.CountryID = personUpdateRequest.CountryID;
        matchingPerson.Address = personUpdateRequest.Address;
        matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

        await _db.SaveChangesAsync();
        
        return matchingPerson.ToPersonResponse();
    }

    public async Task<bool> DeletePerson(Guid personID)
    {
        if (personID == null)
        {
            throw new ArgumentNullException(nameof(personID));
        }

        Person? person = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == personID);
        if (person != null)
        {
            
            _db.Persons.Remove(_db.Persons.First(temp => temp.PersonID == personID));
            await _db.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<MemoryStream> GetPersonsCSV()
    {
        MemoryStream memoryStream = new MemoryStream();
        StreamWriter streamWriter = new StreamWriter(memoryStream);

        CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
        CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);
        
        csvWriter.WriteField(nameof(PersonResponse.PersonName));
        csvWriter.WriteField(nameof(PersonResponse.Email));
        csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
        csvWriter.WriteField(nameof(PersonResponse.Age));
        csvWriter.WriteField(nameof(PersonResponse.Gender));
        csvWriter.WriteField(nameof(PersonResponse.Country));
        csvWriter.WriteField(nameof(PersonResponse.Address));
        csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
        await csvWriter.NextRecordAsync();


        List<PersonResponse> persons = await _db.Persons.Include("Country").Select(temp => temp.ToPersonResponse()).ToListAsync();

        foreach (var person in persons)
        {
            csvWriter.WriteField(person.PersonName);
            csvWriter.WriteField(person.Email);
            if (person.DateOfBirth.HasValue)
                csvWriter.WriteField(person.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            else
                csvWriter.WriteField("");
            csvWriter.WriteField(person.Age);
            csvWriter.WriteField(person.Gender);
            csvWriter.WriteField(person.Country);
            csvWriter.WriteField(person.Address);
            csvWriter.WriteField(person.ReceiveNewsLetters);
            await csvWriter.NextRecordAsync();
            await csvWriter.FlushAsync();
        }
        
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<MemoryStream> GetPersonsExcel()
    {
        MemoryStream memoryStream = new MemoryStream();
        using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");
            worksheet.Cells["A1"].Value = "Person Name";
            worksheet.Cells["B1"].Value = "Email";
            worksheet.Cells["C1"].Value = "Date Of Birth";
            worksheet.Cells["D1"].Value = "Age";
            worksheet.Cells["E1"].Value = "Gender";
            worksheet.Cells["F1"].Value = "Country";
            worksheet.Cells["G1"].Value = "Address";
            worksheet.Cells["H1"].Value = "Receive News Letters";

            using (ExcelRange headerCells = worksheet.Cells["A1:H1"])
            {
                headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                headerCells.Style.Font.Bold = true;
            }

            int row = 2;
            List<PersonResponse> persons =
                await _db.Persons.Include("Country").Select(temp => temp.ToPersonResponse()).ToListAsync();

            foreach (var person in persons)
            {
                worksheet.Cells[row, 1].Value = person.PersonName;
                worksheet.Cells[row, 2].Value = person.Email;
                if (person.DateOfBirth.HasValue)
                {
                    worksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("yyyy-MM-dd");
                }
                worksheet.Cells[row, 4].Value = person.Age;
                worksheet.Cells[row, 5].Value = person.Gender;
                worksheet.Cells[row, 6].Value = person.Country;
                worksheet.Cells[row, 7].Value = person.Address;
                worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;

                row++;
            }

            worksheet.Cells[$"A1:H{row}"].AutoFitColumns();

            await excelPackage.SaveAsync();
        }

        memoryStream.Position = 0;
        return memoryStream;
    }
}