﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entites;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(i => i.Make)),
            "new" => query.Sort(x => x.Descending(i => i.CreatedAt)),
            _ => query.Sort(x => x.Ascending(i => i.AuctionEnd))
        };

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(i => i.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(i => i.AuctionEnd < DateTime.UtcNow.AddHours(6)
                && i.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(i => i.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query = query.Match(i => i.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query = query.Match(i => i.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber);

        query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}
