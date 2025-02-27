using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;

namespace TrevizaniRoleplay.Server.Models;

public class AudioSpot
{
    public Guid Id { get; } = Guid.NewGuid();
    public Vector3 Position { get; set; }
    public string Source { get; set; } = string.Empty;
    public uint Dimension { get; set; }
    public float Volume { get; set; } = 1;
    public uint? VehicleId { get; set; }
    public bool Loop { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? ItemId { get; set; }
    public float Range { get; set; }
    public uint? PlayerId { get; set; }
    public DateTime RegisterDate { get; } = DateTime.Now;

    public void Setup(MyPlayer player)
    {
        player.Emit("Audio:Setup", Id.ToString(), Position, Source, Dimension, Volume, Loop, Range, VehicleId, PlayerId);
    }

    public void SetupAllClients()
    {
        NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Audio:Setup", Id.ToString(), Position, Source, Dimension, Volume, Loop, Range, VehicleId, PlayerId);

        if (!Global.AudioSpots.Contains(this))
            Global.AudioSpots.Add(this);
    }

    public void Remove(MyPlayer player)
    {
        player.Emit("Audio:Remove", Id.ToString());
    }

    public void RemoveAllClients()
    {
        NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Audio:Remove", Id.ToString());
        Global.AudioSpots.Remove(this);
    }
}