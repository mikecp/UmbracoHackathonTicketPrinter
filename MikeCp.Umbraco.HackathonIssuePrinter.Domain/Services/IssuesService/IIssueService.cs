namespace MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;

public interface IIssueService
{
    Task<IssueRecord?> GetIssue(int issueId);
    Task<IEnumerable<IssueRecord>> GetIssues(IssuesFilter filter);
}