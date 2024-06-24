using McAzureFunctionController.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace McAzureFunctionController.DB_Extensions;

public static class ServerStatusExtensions
{

    private static async Task<Database> GetDatabase(this CosmosClient client) =>
        (await client.CreateDatabaseIfNotExistsAsync(DB_NAME)).Database;
    private static async Task<Container> GetContainer(this CosmosClient client) =>
        (await (await client.GetDatabase()).CreateContainerIfNotExistsAsync(COLLECTION_SERVER_STATUS, PARTITION_KEY_PATH)).Container;

    private static string DB_NAME = "Immisorator-com";
    private static string COLLECTION_SERVER_STATUS = "ServerStatuses";
    private static string PARTITION_KEY_PATH = "/"+nameof(MinecraftServerStatus.ServerType);
    private static PartitionKey PARTITION_KEY = new PartitionKey(MinecraftServerStatus.Server_Type);

    // write method to get server status
    public static async Task<MinecraftServerStatus> GetServerStatus(this CosmosClient documentClient, Guid serverId)
    {
        var serverContainer = await documentClient.GetContainer();
        
        var iterator = serverContainer.GetItemLinqQueryable<MinecraftServerStatus>()
            .Where(mc => mc.ServerId == serverId)
            .ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var server in await iterator.ReadNextAsync())
            {
                return server;
            }
        }
        return null;
    }

    public static async Task<MinecraftServerStatus> AddOrUpdateServerStatus(this CosmosClient documentClient, MinecraftServerStatus minecraftServerStatus)
    {
        var serverContainer = await documentClient.GetContainer();

        // TODO: Validate server exists, and user is owner (or allowed to update status)
        return await serverContainer.UpsertItemAsync(minecraftServerStatus);
    }
}
