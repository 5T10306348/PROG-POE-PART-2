using System.ComponentModel.DataAnnotations;

namespace CMCS.Models;
public class ClaimModel
{
    [Required]
    public string UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Hours worked must be greater than 0.")]
    public double HoursWorked { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0.")]
    public double HourlyRate { get; set; }

    public string ExtraNotes { get; set; }
}
