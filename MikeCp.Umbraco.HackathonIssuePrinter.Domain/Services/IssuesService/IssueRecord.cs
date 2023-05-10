using System.ComponentModel.DataAnnotations;

namespace MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;

public record IssueRecord(
    [Required] int Id, 
    [Required] int Number,
    [Required] string Repository,
    [Required] string Url,
    [Required] string Title,
    [Required] string User,
    string[] Labels, 
    string State, 
    string Body, 
    [Required] DateTime CreatedOn, 
    DateTime UpdatedOn)
{
}

