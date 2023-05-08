using System.ComponentModel.DataAnnotations;

namespace MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;

public record IssueRecord(
    [Required] int Id, 
    [Required] int Number, 
    [Required] string Url,
    [Required] string Title,
    string[] Labels, 
    string State, 
    string Body, 
    [Required] DateTime CreatedOn, 
    DateTime UpdatedOn)
{
}

