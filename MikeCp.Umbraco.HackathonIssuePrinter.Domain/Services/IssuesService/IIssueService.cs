namespace MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;

public interface IIssueService
{
    Task<IEnumerable<IssueRecord>> GetIssues(IssuesFilter filter);
}