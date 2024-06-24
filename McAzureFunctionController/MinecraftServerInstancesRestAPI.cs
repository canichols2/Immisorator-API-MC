using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using McAzureFunctionController.DB_Extensions;
using McAzureFunctionController.Models;
using McAzureFunctionController.Permissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;

namespace McAzureFunctionController;

public class MinecraftServerInstancesRestAPI
{


    [FunctionName(nameof(AddNewServerInstance))]
    [OpenApiOperation(operationId: nameof(AddNewServerInstance))]
    [OpenApiRequestBody("application/json", typeof(MinecraftServerInstance))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MinecraftServerInstance), Description = "The OK response")]
    public async Task<IActionResult> AddNewServerInstance(
        [HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "ServerInstance")] MinecraftServerInstance newServerInstance,
        [CosmosDB(Connection = "ConnectionStrings:Cosmos")] CosmosClient dbClient,
        ClaimsPrincipal claimsPrincipal)
    {
        try
        {
            var currentUser = await dbClient.GetOrAddUser(claimsPrincipal);
            // TODO: add validation if server exists, don't change/update the server unless it's owned by the user
            newServerInstance.OwnerId = currentUser.id;
            var newServer = await dbClient.AddOrUpdateServerInstance(newServerInstance);
            return new OkObjectResult(newServer);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            var errorResponse = new ObjectResult(e.Message);
            errorResponse.StatusCode = 500;
            return errorResponse;
        }
    }

    [FunctionName(nameof(GetServerInstances))]
    [OpenApiOperation(operationId: nameof(GetServerInstances))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<MinecraftServerInstance>), Description = "The OK response")]
    public async Task<IActionResult> GetServerInstances(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ServerInstance")] HttpRequest req,
        [CosmosDB(Connection = "ConnectionStrings:Cosmos", PartitionKey = "default")] CosmosClient dbClient,
        ClaimsPrincipal claimsPrincipal
        )
    {
        //return new OkObjectResult("something");
        try
        {
            var currentUser = await dbClient.GetOrAddUser(claimsPrincipal);
            IAsyncEnumerable<MinecraftServerInstance> serverInstanceAsyncEnumerable = dbClient.GetOwnedServers(currentUser);
            var servers = await serverInstanceAsyncEnumerable.ToListAsync();

            return new OkObjectResult(servers);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            var errorResponse = new ObjectResult(e.Message);
            errorResponse.StatusCode = 500;
            return errorResponse;
        }
    }

    // Write method to delete a server instance
    [FunctionName(nameof(DeleteServerInstance))]
    [OpenApiOperation(operationId: nameof(DeleteServerInstance))]
    [OpenApiParameter(name: "serverId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The server id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MinecraftServerInstance), Description = "The OK response")]
    public async Task<IActionResult> DeleteServerInstance(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "ServerInstance/{serverId}")] HttpRequest req,
        [CosmosDB(Connection = "ConnectionStrings:Cosmos", PartitionKey = "default")] CosmosClient dbClient,
        Guid serverId,
        ClaimsPrincipal claimsPrincipal
        )
    {
        try
        {
            var currentUser = await dbClient.GetOrAddUser(claimsPrincipal);
            var server = await dbClient.GetServerInstance(serverId);
            if (server.OwnerId != currentUser.id)
            {
                return new UnauthorizedResult();
            }
            await dbClient.DeleteServerInstance(serverId);
            return new OkObjectResult(server);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            var errorResponse = new ObjectResult(e.Message);
            errorResponse.StatusCode = 500;
            return errorResponse;
        }
    }

    // write a method to get a single server by id
    [FunctionName(nameof(GetServerInstance))]
    [OpenApiOperation(operationId: nameof(GetServerInstance))]
    [OpenApiParameter(name: "serverId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The server id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MinecraftServerInstance), Description = "The OK response")]
    public async Task<IActionResult> GetServerInstance(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ServerInstance/{serverId}")] HttpRequest req,
        [CosmosDB(Connection = "ConnectionStrings:Cosmos", PartitionKey = "default")] CosmosClient dbClient,
        Guid serverId,
        ClaimsPrincipal claimsPrincipal
        )
    {
        try
        {
            var currentUser = await dbClient.GetOrAddUser(claimsPrincipal);
            var server = await dbClient.GetServerInstance(serverId);
            if (server.OwnerId != currentUser.id)
            {
                return new UnauthorizedResult();
            }
            return new OkObjectResult(server);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            var errorResponse = new ObjectResult(e.Message);
            errorResponse.StatusCode = 500;
            return errorResponse;
        }
    }
}

