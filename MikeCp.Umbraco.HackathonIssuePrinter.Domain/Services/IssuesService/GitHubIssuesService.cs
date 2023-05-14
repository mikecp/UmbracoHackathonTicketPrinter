using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Flurl;
using Flurl.Http;

namespace MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;

public class GitHubIssuesService : IIssueService
{
    protected readonly string gitHubRoot = string.Empty;
    protected readonly string pat_token = string.Empty;

    public class Configuration
    {
        public string ApiRoot { get; set; } = string.Empty;
        public string[] LabelsToProcess { get; set; } = new string[0];
        public string PatToken { get; set; } = string.Empty;
    }

    public record IssueRecordDto(
     [Required] int Id,
     [Required] int Number,
     [Required] string Repository_Url,
     [Required] string Html_Url,
     [Required] string Title,
     [Required] UserDto User,
     LabelDto[] Labels,
     string State,
     string Body,
     [Required] DateTime Created_At,
     DateTime Updated_At)
    { }

    public record LabelDto(
        [Required] string Name
    )
    { }

    public record UserDto
    (
        [Required] string Login
    )
    { }

    public record LabeledIssueDto (
        [Required] string Action,
        [Required] IssueRecordDto Issue,
        LabelDto Label
    );

    public GitHubIssuesService(Configuration configuration)
    {
        gitHubRoot = configuration.ApiRoot;
        pat_token = configuration.PatToken ?? string.Empty;
    }

    public async Task<IEnumerable<IssueRecord>> GetIssues(IssuesFilter filter)
    {
        using var gitApiClient = new HttpClient();
        gitApiClient.BaseAddress = new Uri(gitHubRoot);

        gitApiClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
        gitApiClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        gitApiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", pat_token);

        var issues = new List<IssueRecordDto>();
        var link = "/repos/Umbraco/Umbraco-CMS/issues";

        using var client = new FlurlClient(gitApiClient);
        do
        {
            var issuesResponse = await link
                .WithClient(client)
                .SetQueryParam("labels", string.Join(',', filter.Labels), NullValueHandling.Ignore)
                .SetQueryParam("since", filter.CreatedSince.ToString("O"), NullValueHandling.Remove)
                .GetAsync();

            var nextIssues = await issuesResponse.ResponseMessage.Content.ReadFromJsonAsync<IEnumerable<IssueRecordDto>>();
            if (nextIssues != null)
            {
                issues.AddRange(nextIssues);
            }

            link = GetNextPageLink(issuesResponse.Headers.FirstOrDefault("link"));

        } while (!string.IsNullOrWhiteSpace(link));

        return issues.Select(DtoToRecord);
    }

    public static IssueRecord DtoToRecord(IssueRecordDto dtoRecord)
        => new(
            dtoRecord.Id,
            dtoRecord.Number,
            dtoRecord.Repository_Url.Substring(dtoRecord.Repository_Url.IndexOf("repos/") + "repos/".Length),
            dtoRecord.Html_Url,
            dtoRecord.Title,
            dtoRecord.User.Login,
            dtoRecord.Labels.Select(l => l.Name).ToArray(),
            dtoRecord.State,
            dtoRecord.Body,
            dtoRecord.Created_At,
            dtoRecord.Updated_At
        );

    private string? GetNextPageLink(string? linksInfo)
        => string.IsNullOrWhiteSpace(linksInfo)
            ? null
            : linksInfo
                .Split(',')
                .FirstOrDefault(l => l.Contains("rel=\"next\""))
                ?.Split(';')[0]
                .Trim(' ', '<', '>');
}
