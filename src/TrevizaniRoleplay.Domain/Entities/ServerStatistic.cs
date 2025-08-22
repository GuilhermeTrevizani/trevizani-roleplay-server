namespace TrevizaniRoleplay.Domain.Entities;

public class ServerStatistic : BaseEntity
{
    private ServerStatistic()
    {
    }

    public ServerStatistic(int totalPlayersCount, int spawnedPlayersCount)
    {
        TotalPlayersCount = totalPlayersCount;
        SpawnedPlayersCount = spawnedPlayersCount;
    }

    public int TotalPlayersCount { get; private set; }
    public int SpawnedPlayersCount { get; private set; }
}