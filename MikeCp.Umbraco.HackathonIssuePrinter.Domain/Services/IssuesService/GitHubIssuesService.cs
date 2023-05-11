using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Flurl;
using Flurl.Http;

namespace MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;

public class GitHubIssuesService : IIssueService
{
    protected const string GitHubRoot = "https://api.github.com";

    protected const string pat_token =
        "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";

    protected const string ghp_token = "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";

    protected const string token = "11AANQAAY0x6TvBUDHrXby_DJLu1lh3hLVZ8lDtRgAY82uoY9uyHb5j1FexLtqEtszZF4OKCIF0Jv7ur7E";

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

    public Task<IssueRecord?> GetIssue(int issueId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<IssueRecord>> GetIssues(IssuesFilter filter)
    {

        using var gitApiClient = new HttpClient();
        gitApiClient.BaseAddress = new Uri(GitHubRoot);
        var token = pat_token;

        gitApiClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
        gitApiClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        gitApiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);

      /*  var testResponse = await testClient.GetStringAsync("/repos/Umbraco/Umbraco-CMS/issues?labels=community%2Fup-for-grabs");
       
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(GitHubRoot);
        httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", pat_token);
       */
        var issues = new List<IssueRecordDto>();
        var link = "/repos/Umbraco/Umbraco-CMS/issues";

        using var client = new FlurlClient(gitApiClient);
   /*    var blob = await $"{GitHubRoot}{link}"
            .WithClient(client)
            .SetQueryParam("labels", string.Join(',', filter.Labels), NullValueHandling.Ignore)
            //    .SetQueryParam("since", filter.CreatedSince.ToString("O"), NullValueHandling.Remove)
            .GetJsonAsync<IEnumerable<IssueRecordDto>>();


/*

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"token {pat_token}");
        //httpClient.BaseAddress = new Uri(GitHubRoot);
        var response = await httpClient
            .GetAsync("https://api.github.com/repos/mikecp/Umbraco-CMS/issues?labels=community%2Fup-for-grabs");

        using var client = new FlurlClient(GitHubRoot);

        var blob1 = await client
            .WithOAuthBearerToken(pat_token)
            .Request("repos", "Umbraco", "Umbraco-CMS", "issues")
            .SetQueryParam("labels", string.Join(',', filter.Labels), NullValueHandling.Ignore)
            .GetJsonAsync<IEnumerable<IssueRecordDto>>();

        var blob = await link
            .WithClient(client)
            .SetQueryParam("labels", string.Join(',', filter.Labels), NullValueHandling.Ignore)
            //    .SetQueryParam("since", filter.CreatedSince.ToString("O"), NullValueHandling.Remove)
            .GetJsonAsync<IEnumerable<IssueRecordDto>>();

        */
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
