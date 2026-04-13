using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class YearcardDTO
{
    public Guid? Id { get; set; }
    public int CardId { get; set; }

    [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
    public DateTime ValidTo { get; set; }
    public List<ValidityIntervalDTO> ValidityIntervals { get; set; } = new List<ValidityIntervalDTO>();

    [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\+\d{1,3}-(\d+(-\d+)*)$", ErrorMessage = "Phone number must follow +<country code>-<number>, digits and hyphens only")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    public string? UserName { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsValidForDiscount { get; set; }
    public string ExposedIdentification { get; set; } = string.Empty;
}