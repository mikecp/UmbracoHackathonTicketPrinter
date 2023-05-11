using System.ComponentModel.DataAnnotations;

namespace MikeCp.Umbraco.HackathonIssuePrinter.PrinterService;

public record IssueDocument
(
    [Required] string Id,
    [Required] string Title,
    string Details,
    [Required] string Source,
    [Required] string Author,  
    [Required] string Link
);

public interface IPrinterService
{
    string PrinterType { get; }

    void Print(IssueDocument issue);
}