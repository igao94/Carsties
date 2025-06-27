using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entites;
using System.Text.Json;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDbAsync(WebApplication app)
    {
        await DB.InitAsync("SearchDb", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(i => i.Make, KeyType.Text)
            .Key(i => i.Model, KeyType.Text)
            .Key(i => i.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();

        if (count == 0)
        {
            Console.WriteLine("No data - will atempt to seed.");

            var itemsData = await File.ReadAllTextAsync("Data/auctions.json");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var items = JsonSerializer.Deserialize<List<Item>>(itemsData, options);

            if (items is not null)
            {
                await DB.SaveAsync(items);
            }
        }
    }
}
