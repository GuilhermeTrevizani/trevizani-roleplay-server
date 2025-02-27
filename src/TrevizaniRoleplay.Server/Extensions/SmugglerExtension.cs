using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Server.Extensions;

public static class SmugglerExtension
{
    public static void CreateIdentifier(this Smuggler smuggler)
    {
        RemoveIdentifier(smuggler);

        Functions.RunOnMainThread(() =>
        {
            var ped = Functions.CreatePed(Functions.Hash(smuggler.Model),
                new(smuggler.PosX, smuggler.PosY, smuggler.PosZ),
                new(smuggler.RotR, smuggler.RotP, smuggler.RotY),
                smuggler.Dimension);
            ped.SmugglerId = smuggler.Id;

            var colShape = Functions.CreateColShapeCylinder(new(smuggler.PosX, smuggler.PosY, smuggler.PosZ - 0.95f), 1, 1.5f, smuggler.Dimension);
            colShape.Description = $"[CONTRABANDISTA] {{#FFFFFF}}Esse contrabandista está pagando ${smuggler.Value:N0} por peça de veículo. Use /contrabando para vender.";
            colShape.SmugglerId = smuggler.Id;
        });
    }

    public static void RemoveIdentifier(this Smuggler smuggler)
    {

        Functions.RunOnMainThread(() =>
        {
            var ped = Global.Peds.FirstOrDefault(x => x.SmugglerId == smuggler.Id);
            ped?.RemoveAllClients();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.SmugglerId == smuggler.Id);
            colShape?.Delete();
        });
    }

    public static bool IsSpawned(this Smuggler smuggler)
    {
        return Global.Peds.FirstOrDefault(x => x.SmugglerId == smuggler.Id) is not null;
    }

    public static IEnumerable<string> GetAllowedCharacters(this Smuggler smuggler)
    {
        return Functions.Deserialize<IEnumerable<string>>(smuggler.AllowedCharactersJSON);
    }
}