using McAzureFunctionController.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace McAzureFunctionController.DB_Extensions;


public static class ServerExtensions
{
    private static async Task<Database> GetDatabase(this CosmosClient client)
    {
        DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(DB_NAME);
        return database.Database;
    }
    private static async Task<Container> GetContainer(this CosmosClient client)
    {
        var DB = await client.GetDatabase();
        ContainerResponse container = await DB.CreateContainerIfNotExistsAsync(COLLECTION_SERVERS, PARTITION_KEY_PATH);
        return container.Container;
    }

    private static string DB_NAME = "Immisorator-com";
    private static string COLLECTION_SERVERS = "ServerInstances";
    private static string PARTITION_KEY_PATH = "/"+nameof(MinecraftServerInstance.ServerType);
    private static PartitionKey PARTITION_KEY = new PartitionKey(MinecraftServerInstance.Server_Type);

    public static async Task<MinecraftServerInstance> AddOrUpdateServerInstance(this CosmosClient documentClient, MinecraftServerInstance minecraftServerInstance) =>
        await (await documentClient.GetContainer()).UpsertItemAsync(minecraftServerInstance);

    public static async Task<MinecraftServerInstance> GetServerInstance(this CosmosClient documentClient, Guid serverId)
    {
        Container serverContainer = await documentClient.GetContainer();
        ItemResponse<MinecraftServerInstance> x = await serverContainer.ReadItemAsync<MinecraftServerInstance>(serverId.ToString(), PARTITION_KEY);
        return x;
    }
    // write a method to get all servers
    public static async IAsyncEnumerable<MinecraftServerInstance> GetServerInstances(this CosmosClient documentClient)
    {
        Container serverContainer = await documentClient.GetContainer();
        var iterator = serverContainer.GetItemLinqQueryable<MinecraftServerInstance>()
            .ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var server in await iterator.ReadNextAsync())
            {
                yield return server;
            }
        }
    }
    // write a method to get all servers owned by a user
    public static async IAsyncEnumerable<MinecraftServerInstance> GetOwnedServers(this CosmosClient documentClient, Models.User user)
    {
        var serverContainer = await documentClient.GetContainer();
        var iterator = serverContainer.GetItemLinqQueryable<MinecraftServerInstance>()
            .Where(mc => mc.OwnerId == user.id)
            .ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var server in await iterator.ReadNextAsync())
            {
                yield return server;
            }
        }
    }
    // write a method to delete a server
    public static async Task DeleteServerInstance(this CosmosClient documentClient, Guid serverId)
    {
        Container serverContainer = await documentClient.GetContainer();
        await serverContainer.DeleteItemAsync<MinecraftServerInstance>(serverId.ToString(), PARTITION_KEY);
    }
}
