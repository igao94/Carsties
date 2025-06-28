using MongoDB.Entities;
using SearchService.Entites;

namespace SearchService.Services;

public class AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
{
    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(i => i.UpdatedAt))
            .Project(i => i.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        var response = await httpClient
            .GetFromJsonAsync<List<Item>>(config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated);

        return response ?? [];
    }
}
