using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using User = McAzureFunctionController.Models.User;
using Microsoft.Azure.Cosmos.Linq;

namespace McAzureFunctionController.Permissions;

public static class UserExtensions
{
    private static async Task<Database> GetDatabase(this CosmosClient client) =>
        (await client.CreateDatabaseIfNotExistsAsync(DB_NAME)).Database;
    private static async Task<Container> GetContainer(this CosmosClient client) =>
        (await (await client.GetDatabase()).CreateContainerIfNotExistsAsync(COLLECTION_USERS, PARTITION_KEY_PATH)).Container;

    private static string DB_NAME = "Immisorator-com";
    private static string COLLECTION_USERS = "Users";
    private static string PARTITION_KEY_PATH = "/" + nameof(User.provider);
    private static PartitionKey PARTITION_KEY = PartitionKey.None;
    public static async Task<User> GetOrAddUser(this CosmosClient documentClient, ClaimsPrincipal claimsPrincipal)
    {
        // Either UPN if running in azure. or AuthLevel (Admin, Function, Etc.) if running locally.
        var upnClaim = claimsPrincipal.FindFirst(ClaimTypes.Upn) ?? claimsPrincipal.FindFirst("http://schemas.microsoft.com/2017/07/functions/claims/authlevel");

        var userContainer = await documentClient.GetContainer();

        var iterator = userContainer.GetItemLinqQueryable<User>()
            .Where(p => p.Username == upnClaim.Value && p.provider == upnClaim.Issuer)
            .ToFeedIterator();

        if (iterator.HasMoreResults)
        {
            foreach (User result in await iterator.ReadNextAsync())
            {
                return result;
            }
        }

        User newUser = new User
        {
            Username = upnClaim.Value,
            id = Guid.NewGuid(),
            provider = upnClaim.Issuer,
        };

        var responseUser = await userContainer.CreateItemAsync(newUser, partitionKey: new PartitionKey(newUser.provider));
        return responseUser.Resource;
    }
}
