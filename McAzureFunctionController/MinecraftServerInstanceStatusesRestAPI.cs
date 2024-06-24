using System;
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
public class MinecraftServerInstanceStatusesRestAPI
{
    // Write method to get the status of a server
    [FunctionName(nameof(GetServerStatus))]
    [OpenApiOperation(operationId: nameof(GetServerStatus))]
    [OpenApiParameter(name: "serverId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The server id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MinecraftServerStatus), Description = "The OK response")]
    public async Task<IActionResult> GetServerStatus(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ServerStatus/{serverId}")] HttpRequest req,
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
            var status = await dbClient.GetServerStatus(serverId);
            return new OkObjectResult(status);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            var errorResponse = new ObjectResult(e.Message);
            errorResponse.StatusCode = 500;
            return errorResponse;
        }
    }

    // Write method to update the status of a server
    [FunctionName(nameof(UpdateServerStatus))]
    [OpenApiOperation(operationId: nameof(UpdateServerStatus))]
    [OpenApiParameter(name: "serverId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The server id")]
    [OpenApiRequestBody("application/json", typeof(MinecraftServerStatus))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MinecraftServerStatus), Description = "The OK response")]
    public async Task<IActionResult> UpdateServerStatus(
        [HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "ServerStatus/{serverId}")] MinecraftServerStatus serverStatus,
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
            serverStatus.ServerId = serverId;
            var currentStatus = await dbClient.GetServerStatus(serverId);
            if(currentStatus != null)
            {
                serverStatus.id = currentStatus.id;
            }
            else
            {
                serverStatus.id = Guid.NewGuid();
            }
            var status = await dbClient.AddOrUpdateServerStatus(serverStatus);
            return new OkObjectResult(status);
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

