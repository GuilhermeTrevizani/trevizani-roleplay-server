using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Server.Factories;

namespace TrevizaniRoleplay.Server.Extensions;

public static class HelpRequestExtension
{
    public static async Task<MyPlayer?> Check(this HelpRequest helpRequest)
    {
        var player = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == helpRequest.CharacterSessionId && x.Character.Name == helpRequest.CharacterName);
        if (player is not null)
            return player;

        helpRequest.Answer(null);

        var context = Functions.GetDatabaseContext();
        context.HelpRequests.Update(helpRequest);
        await context.SaveChangesAsync();

        Global.HelpRequests.Remove(helpRequest);

        return null;
    }
}