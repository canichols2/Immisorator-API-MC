using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
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

public class MinecraftServerHostsRestAPI
{

    [FunctionName(nameof(UpsertServerHost))]
    [OpenApiOperation(operationId: nameof(UpsertServerHost))]
    public async Task<IActionResult> UpsertServerHost(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ServerHost")] MinecraftServerHost serverHostRequest,
        [CosmosDB(Connection = "ConnectionStrings:Cosmos", PartitionKey = "default")] CosmosClient dbClient,
        ClaimsPrincipal claimsPrincipal
        )
    {
        try
        {
            var currentUser = await dbClient.GetOrAddUser(claimsPrincipal);
            serverHostRequest.OwnerId = currentUser.id;
            var serverHost = await dbClient.AddOrUpdateServerHost(serverHostRequest);
            return new OkObjectResult(serverHost);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            var errorResponse = new ObjectResult(e.Message);
            errorResponse.StatusCode = 500;
            return errorResponse;
        }
    }

    [FunctionName(nameof(GetServerHosts))]
    [OpenApiOperation(operationId: nameof(GetServerHosts))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<MinecraftServerHost>), Description = "The OK response")]
    public async Task<IActionResult> GetServerHosts(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ServerHost")] HttpRequest req,
        [CosmosDB(Connection = "ConnectionStrings:Cosmos")] CosmosClient dbClient,
        ClaimsPrincipal claimsPrincipal
        )
    {
        try
        {
            var currentUser = await dbClient.GetOrAddUser(claimsPrincipal);
            var hosts = dbClient.GetOwnedServerHosts(currentUser);
            return new OkObjectResult(hosts);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            var errorResponse = new ObjectResult(e.Message);
            errorResponse.StatusCode = 500;
            return errorResponse;
        }
    }

    // write a method to get a single server host by id
    [FunctionName(nameof(GetServerHost))]
    [OpenApiOperation(operationId: nameof(GetServerHost))]
    [OpenApiParameter(name: "serverId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The server id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MinecraftServerHost), Description = "The OK response")]
    public async Task<IActionResult> GetServerHost(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ServerHost/{serverId}")] HttpRequest req,
        [CosmosDB(Connection = "ConnectionStrings:Cosmos", PartitionKey = "default")] CosmosClient dbClient,
        Guid serverId,
        ClaimsPrincipal claimsPrincipal
        )
    {
        try
        {
            var currentUser = await dbClient.GetOrAddUser(claimsPrincipal);
            var server = await dbClient.GetServerHost(serverId);
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


    // Write method to delete a server host
    [FunctionName(nameof(DeleteServerHost))]
    [OpenApiOperation(operationId: nameof(DeleteServerHost))]
    [OpenApiParameter(name: "serverId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The server id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MinecraftServerHost), Description = "The OK response")]
    public async Task<IActionResult> DeleteServerHost(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "ServerHost/{serverId}")] HttpRequest req,
        [CosmosDB(Connection = "ConnectionStrings:Cosmos", PartitionKey = "default")] CosmosClient dbClient,
        Guid serverId,
        ClaimsPrincipal claimsPrincipal
        )
    {
        try
        {
            var currentUser = await dbClient.GetOrAddUser(claimsPrincipal);
            var server = await dbClient.GetServerHost(serverId);
            if (server.OwnerId != currentUser.id)
            {
                return new UnauthorizedResult();
            }
            await dbClient.DeleteServerHost(serverId);
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

