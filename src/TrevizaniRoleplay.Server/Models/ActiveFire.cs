using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Server.Models;

public class ActiveFire
{
    public Guid Id { get; set; }
    public System.Timers.Timer Timer { get; set; } = default!;
    public List<Span> Spans { get; set; } = [];
    public Fire Fire => Global.Fires.FirstOrDefault(x => x.Id == Id)!;

    public class Span()
    {
        public float Life { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public Particle ParticleFire { get; set; } = default!;
        public Particle ParticleSmoke { get; set; } = default!;

        public void Create(Vector3 position, uint dimension, float life)
        {
            Life = life;
            ParticleFire = new Particle().SetupAllClients("scr_trevor3", "scr_trev3_trailer_plume", position, dimension);
            ParticleSmoke = new Particle().SetupAllClients("scr_agencyheistb", "scr_env_agency3b_smoke", position, dimension);
        }

        public void Destroy()
        {
            ParticleFire?.RemoveAllClients();
            ParticleSmoke?.RemoveAllClients();
        }
    }

    public void Start(Fire fire)
    {
        Id = fire.Id;

        Timer = new System.Timers.Timer(TimeSpan.FromSeconds(fire.SecondsNewFireSpan));
        Timer.Elapsed += Timer_Elapsed;
        Timer.Start();
        Timer_Elapsed(null, null);

        Global.ActiveFires.Add(this);
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        var fire = Fire;
        if (fire is null)
            return;

        if (Spans.Count >= fire.MaxFireSpan)
            return;

        var lastFireSpan = Spans.OrderByDescending(x => x.RegisterDate).FirstOrDefault();
        var lastPosition = lastFireSpan?.ParticleFire?.Position ?? new(fire.PosX, fire.PosY, fire.PosZ);

        if (lastFireSpan is not null)
        {
            lastPosition.X += fire.PositionNewFireSpan * (new Random().NextDouble() >= 0.5 ? 1 : -1);
            lastPosition.Y += fire.PositionNewFireSpan * (new Random().NextDouble() >= 0.5 ? 1 : -1);
        }

        var span = new Span();
        span.Create(lastPosition, fire.Dimension, fire.FireSpanLife);
        Spans.Add(span);
    }

    public void Stop()
    {
        Timer?.Stop();
        Spans.ForEach(x => x.Destroy());
        Global.ActiveFires.Remove(this);
    }
}