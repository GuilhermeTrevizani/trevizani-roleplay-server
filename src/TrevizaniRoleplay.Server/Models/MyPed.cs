using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;

namespace TrevizaniRoleplay.Server.Models;

public class MyPed
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid? SmugglerId { get; set; }
    public Guid? BodyId { get; set; }
    public Vector3 Position { get; private set; } = default!;
    public uint Dimension { get; private set; }
    public uint Model { get; private set; }
    public float Heading { get; private set; }
    public string? PersonalizationJson { get; private set; }
    public string? OutfitJson { get; private set; }

    public void Setup(MyPlayer player)
    {
        player.Emit("Ped:Setup", Id.ToString(), Model, Heading, Position, Dimension, PersonalizationJson, OutfitJson);
    }

    public MyPed SetupAllClients(uint model, float heading, Vector3 position, uint dimension,
        string? personalizationJson, string? outfitJson)
    {
        Model = model;
        Heading = heading;
        Position = position;
        Dimension = dimension;
        PersonalizationJson = personalizationJson;
        OutfitJson = outfitJson;

        NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Ped:Setup", Id.ToString(), Model, Heading, Position, Dimension, PersonalizationJson, OutfitJson);

        if (!Global.Peds.Contains(this))
            Global.Peds.Add(this);

        return this;
    }

    public void Remove(MyPlayer player)
    {
        player.Emit("Ped:Remove", Id.ToString());
    }

    public void RemoveAllClients()
    {
        NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Ped:Remove", Id.ToString());
        Global.Peds.Remove(this);
    }
}