using System;
using System.Collections.Generic;

namespace McAzureFunctionController.Models;

public abstract class ServerInstance
{
    public static string ServerType { get; set; } = "Unknown";
}
public class MinecraftServerInstance: ServerInstance
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public Guid? ServerHostId { get; set; }
    public Guid? OwnerId { get; set; }
    public string ServerType { get; set; } = Server_Type;
    public static new string Server_Type { get; set; } = "Minecraft_Server";
}

public class MinecraftServerHost
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public Guid? OwnerId { get; set; }
    public string HostType { get; set; } = "Minecraft_Server";
}

public class User
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string provider { get; set; }
    public string Username { get; set; }
}

public class MinecraftServerStatus
{
    public enum Statuses
    {
        Offline,
        Online,
        Error
    }
    public static string Server_Type = "Minecraft_Server";
    public string ServerType { get; set; } = Server_Type;
    public Guid id { get; set; } = Guid.NewGuid();
    
    public Guid ServerId { get; set; }
    
    /// <summary>
    /// Represents the online status of the server
    /// Valid options would be Online, Offline, Error
    /// </summary>
    public Statuses Status { get; set; }

    public List<string> AllPlayers { get; set; }
    public List<string> OnlinePlayers { get; set; }

}