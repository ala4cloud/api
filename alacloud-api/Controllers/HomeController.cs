using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using alacloud_api.Models;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace alacloudAPI.Controllers;



public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CosmosClient _client;
    private readonly Database _database;
    private readonly Container _container;


    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;

        var connection_string = "AccountEndpoint=https://tfex-cosmosdb-account-khw2hur8yl.documents.azure.com:443/;AccountKey=wtuLIMeECbbUXFKmD2U3oAFFQz5U6KIXORn7cCqLQrofugprBWrhjbVcqfhcP8pO53EWSHvEkQ0ZACDb4LAa7g==;";
        var _client = new CosmosClient(connection_string);


        _database = _client.GetDatabase("tfex-cosmos-sql-dbkhw2hur8yl");
        _container = _database.GetContainer("skills");


    }


    DefaultAzureCredential credential = new();


    public async Task<List<SkillsModel>> GetSkillsAsync()
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c");
        using var feed = _container.GetItemQueryIterator<SkillsModel>(
            queryDefinition,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey("software")
            });

        List<SkillsModel> results = new();
        while (feed.HasMoreResults)
        {
            results.AddRange(await feed.ReadNextAsync());
        }

        return results;
    }

    public async Task<IActionResult> Index()
    {

        var resposne = await GetSkillsAsync();
        return new JsonResult(resposne);
        //return Ok(resposne);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
