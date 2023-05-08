using MikeCp.Umbraco.HackathonIssuePrinter.Domain.Services.IssuesService;

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

app.MapGet("/getIssuesTicket", () =>
    {
        var issuesService = new GitHubIssuesService();
        var issuesFilter = new IssuesFilter
        {
            //CreatedSince = new DateTime(2022, 10, 01),
            Labels = new[] { "community/up-for-grabs" }
        };

        var issues=  issuesService.GetIssues(issuesFilter);
    })
    .WithName("GetIssuesTicket");


app.Run();
