using MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;
using MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.POS58D;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapGet("/printIssuesTickets", async () =>
    {
        var issuesService = new GitHubIssuesService();
        var issuesFilter = new IssuesFilter
        {
            CreatedSince = new DateTime(2023, 05, 01),
            Labels = new[] { "community/up-for-grabs" }
        };

        var issues =  (await issuesService.GetIssues(issuesFilter));

        if (issues?.Count() > 0)
        {
            using (var printer = new POS58DPrinterService())
            {
                var issue = issues.First();

               /* issues?.ToList().ForEach(
                    issue =>
                    {*/
                        printer.Print(new
                                        (issue.Number.ToString(),
                                        issue.Title,
                                        string.Empty,
                                        issue.Repository,
                                        issue.User,
                                        issue.Url
                                        ));
         /*           }
                    );*/
            }
        }

    })
    .WithName("PrintIssuesTickets");

app.MapPost("/printGitHubLabeledIssueTicket", (GitHubIssuesService.LabeledIssueDto payload) =>
{
    var labelsToProcess = new[] { "good first issue", "bug" };

    // Make sure we can handle before printing
    if ("labeled".Equals(payload.Action, StringComparison.OrdinalIgnoreCase) &&
        labelsToProcess.Contains(payload.Label?.Name.ToLowerInvariant()) &&
        "open".Equals(payload.Issue.State, StringComparison.OrdinalIgnoreCase))
    {
        // We proceed to print
        var issue = GitHubIssuesService.DtoToRecord(payload.Issue);
   /*     using (var printer = new POS58DPrinterService())
        {
            printer.Print(new
                             (issue.Number.ToString(),
                             issue.Title,
                             string.Empty,
                             issue.Repository,
                             issue.User,
                             issue.Url
                             ));
        }*/
    }
})
    .WithName("PrintGitHubLabeledIssueTicket");

app.Run();
