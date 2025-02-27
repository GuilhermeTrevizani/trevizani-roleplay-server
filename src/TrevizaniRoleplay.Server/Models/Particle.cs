using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;

namespace TrevizaniRoleplay.Server.Models;

public class Particle
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Asset { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Vector3 Position { get; private set; } = default!;
    public uint Dimension { get; private set; }

    public void Setup(MyPlayer player)
    {
        player.Emit("Particle:Setup", Id.ToString(), Asset, Name, Position);
    }

    public Particle SetupAllClients(string asset, string name, Vector3 position, uint dimension)
    {
        Asset = asset;
        Name = name;
        Position = position;
        Dimension = dimension;

        foreach (var player in Global.SpawnedPlayers.Where(x => x.GetDimension() == Dimension))
            player.Emit("Particle:Setup", Id.ToString(), Asset, Name, Position);

        if (!Global.Particles.Contains(this))
            Global.Particles.Add(this);

        return this;
    }

    public void Remove(MyPlayer player)
    {
        player.Emit("Particle:Remove", Id.ToString());
    }

    public void RemoveAllClients()
    {
        NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Particle:Remove", Id.ToString());
        Global.Particles.Remove(this);
    }
}