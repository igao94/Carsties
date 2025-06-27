using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entites;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

await DB.InitAsync("SearchDb", MongoClientSettings
    .FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));

await DB.Index<Item>()
    .Key(i => i.Make, KeyType.Text)
    .Key(i => i.Model, KeyType.Text)
    .Key(i => i.Color, KeyType.Text)
    .CreateAsync();

app.Run();
