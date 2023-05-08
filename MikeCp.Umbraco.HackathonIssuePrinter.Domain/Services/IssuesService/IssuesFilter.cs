namespace MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;

public class IssuesFilter
{
    public IEnumerable<string> Labels{ get; set; } = new List<string>();
    public DateTime CreatedSince { get; set; } = DateTime.Now;
}
