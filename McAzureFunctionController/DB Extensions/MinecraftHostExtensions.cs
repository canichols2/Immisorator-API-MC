using System.Linq;
using McAzureFunctionController.Models;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using User = McAzureFunctionController.Models.User;
using Microsoft.Azure.Cosmos.Linq;
using System.Threading.Tasks;
using System;

namespace McAzureFunctionController.Permissions;


public static class MinecraftHostExtensions
{
    private static async Task<Database> GetDatabase(this CosmosClient client) =>
        (await client.CreateDatabaseIfNotExistsAsync(DB_NAME)).Database;
    private static async Task<Container> GetContainer(this CosmosClient client) =>
        (await (await client.GetDatabase()).CreateContainerIfNotExistsAsync(COLLECTION_SERVER_HOSTS, PARTITION_KEY_PATH)).Container;

    private static string DB_NAME = "Immisorator-com";
    private static string COLLECTION_SERVER_HOSTS = "ServerHosts";
    private static string PARTITION_KEY_PATH = "/" + "NO_VALUE";
    private static PartitionKey PARTITION_KEY = PartitionKey.None;
    public static async Task<MinecraftServerHost> AddOrUpdateServerHost(this CosmosClient documentClient, MinecraftServerHost minecraftServerHost)
    {
        var serverContainer = await documentClient.GetContainer();

        return await serverContainer.UpsertItemAsync(minecraftServerHost);
    }
    // write a method to get all servers
    public static async IAsyncEnumerable<MinecraftServerHost> GetServerHosts(this CosmosClient documentClient)
    {
        var serverContainer = await documentClient.GetContainer();
        var iterator = serverContainer.GetItemLinqQueryable<MinecraftServerHost>()
            .ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var server in await iterator.ReadNextAsync())
            {
                yield return server;
            }
        }
    }
    public static async IAsyncEnumerable<MinecraftServerHost> GetOwnedServerHosts(this CosmosClient documentClient, User user)
    {
        var serverContainer = await documentClient.GetContainer();
        var iterator = serverContainer.GetItemLinqQueryable<MinecraftServerHost>()
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
    public static async Task<MinecraftServerHost> GetServerHost(this CosmosClient documentClient, Guid serverId)
    {
        var serverContainer = await documentClient.GetContainer();

        var iterator = serverContainer.GetItemLinqQueryable<MinecraftServerHost>()
            .Where(s => s.id == serverId)
            .ToFeedIterator();

        if (iterator.HasMoreResults)
        {
            foreach (MinecraftServerHost result in await iterator.ReadNextAsync())
            {
                return result;
            }
        }

        return null;
    }
    // write a method to delete a server
    public static async Task DeleteServerHost(this CosmosClient documentClient, Guid serverId)
    {
        var serverContainer = await documentClient.GetContainer();
        await serverContainer.DeleteItemAsync<MinecraftServerHost>(serverId.ToString(), new PartitionKey(serverId.ToString()));
    }

}