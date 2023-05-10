using MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;
using MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.POS58D;

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

app.UseHttpsRedirection();

app.MapGet("/printIssuesTicket", async () =>
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
    .WithName("PrintIssuesTicket");


app.Run();
