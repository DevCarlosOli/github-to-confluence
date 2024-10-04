using GithubMarkdownToConfluenceAPI.Models;
using GithubMarkdownToConfluenceAPI.Repositories;
using GithubMarkdownToConfluenceAPI.Repositories.Interfaces;
using GithubMarkdownToConfluenceAPI.Services;
using GithubMarkdownToConfluenceAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.Configure<ConfluenceConfig>(builder.Configuration.GetSection("ConfluenceConfig"));
builder.Services.Configure<GitHubConfig>(builder.Configuration.GetSection("GitHubSettings"));
builder.Services.AddScoped<IConfluenceRepository, ConfluenceRepository>();
builder.Services.AddScoped<IMarkdownService, MarkdownService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
