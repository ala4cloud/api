using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using alacloud_api.Models;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Azure.Security.KeyVault.Secrets;

namespace alacloudAPI.Controllers;



public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CosmosClient _client;
    private readonly Database _database;
    private readonly Container _container;
    private readonly SecretClient _secretClient;


    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;

        string keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
        var kvUri = "https://" + keyVaultName + ".vault.azure.net";
        var _secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        var database_name = _secretClient.GetSecret("cosmosDB-name-dev").Value.Value;
        var connection_string = _secretClient.GetSecret("cosmosDB-ConnectionString-dev").Value.Value;

        var _client = new CosmosClient(connection_string);

        _database = _client.GetDatabase(database_name);
        _container = _database.GetContainer("skills");


    }


    public async Task<List<SkillsModel>> GetSkillsAsync()
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c");
        using var feed = _container.GetItemQueryIterator<SkillsModel>(
            queryDefinition,
            requestOptions: new QueryRequestOptions
            {
                //PartitionKey = new PartitionKey("software")
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
