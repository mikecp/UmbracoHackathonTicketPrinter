using MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;
using MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.POS58D;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

var githubConfig = config.GetSection("GitHub").Get<GitHubIssuesService.Configuration>();
var printerConfig = config.GetSection("Printer").Get<MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.PrinterConfiguration>();

app.MapGet("/printIssuesTickets", async (string? label, DateTime? createdSince) =>
    {
        var issuesService = new GitHubIssuesService(githubConfig);

        var issues =  (await issuesService.GetIssues(new IssuesFilter { Labels = new[] { label ?? string.Empty }, CreatedSince = createdSince ?? DateTime.Now })).ToList();

        if (issues?.Count() > 0)
        {
            using (var printer = new POS58DPrinterService(printerConfig))
            {
                issues?.ToList().ForEach(
                    issue =>
                    {
                        printer.Print(new
                                        (issue.Number.ToString(),
                                        issue.Title,
                                        string.Empty,
                                        issue.Repository,
                                        issue.User,
                                        issue.Url
                                        ));
                    }
                    );
            }
        }
    })
    .WithName("PrintIssuesTickets");

app.MapPost("/printGitHubLabeledIssueTicket", (GitHubIssuesService.LabeledIssueDto payload) =>
{
    var labelsToProcess = githubConfig.LabelsToProcess;

    // Make sure it is a trigger we want to handle before printing
    if ("labeled".Equals(payload.Action, StringComparison.OrdinalIgnoreCase) &&
        "open".Equals(payload.Issue.State, StringComparison.OrdinalIgnoreCase) &&
        labelsToProcess.Contains(payload.Label?.Name.ToLowerInvariant() ?? string.Empty)
        )
    {
        // We proceed to print
        var issue = GitHubIssuesService.DtoToRecord(payload.Issue);
        using (var printer = new POS58DPrinterService(printerConfig))
        {
            printer.Print(new
                             (issue.Number.ToString(),
                             issue.Title,
                             string.Empty,
                             issue.Repository,
                             issue.User,
                             issue.Url
                             ));
        }
    }
})
    .WithName("PrintGitHubLabeledIssueTicket");

app.Run();
