using GTANetworkAPI;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffJobScript : Script
{
    [Command("empregos")]
    public static void CMD_empregos(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Jobs))
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        player.Emit("StaffJob:Show", GetJobsJson());
    }

    [RemoteEvent(nameof(StaffJobSave))]
    public async Task StaffJobSave(Player playerParam, string jsonString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Jobs))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var request = Functions.Deserialize<JobRequest>(jsonString);

            if (request.Salary <= 0)
            {
                player.SendNotification(NotificationType.Error, "Salário deve ser maior que 0.");
                return;
            }

            if (request.BlipType < 1 || request.BlipType > Constants.MAX_BLIP_TYPE)
            {
                player.SendNotification(NotificationType.Error, string.Format("Tipo do Blip deve ser entre 1 e {0}.", Constants.MAX_BLIP_TYPE));
                return;
            }

            if (request.BlipColor < 1 || request.BlipColor > 85)
            {
                player.SendNotification(NotificationType.Error, "Cor do Blip deve ser entre 1 e 85.");
                return;
            }

            request.BlipName ??= string.Empty;
            if (request.BlipName.Length < 1 || request.BlipName.Length > 100)
            {
                player.SendNotification(NotificationType.Error, "Nome do Blip deve ter entre 1 e 100 caracteres.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleRentModel) && !Functions.CheckIfVehicleExists(request.VehicleRentModel))
            {
                player.SendNotification(NotificationType.Error, $"Veículo {request.VehicleRentModel} não existe.");
                return;
            }

            if (request.VehicleRentValue < 0)
            {
                player.SendNotification(NotificationType.Error, "Valor Aluguel deve ser maior ou igual a 0.");
                return;
            }

            var job = Global.Jobs.FirstOrDefault(x => x.Id == request.Id);
            if (job is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            job.Update(request.PosX, request.PosY, request.PosZ, request.Salary,
                request.BlipType, request.BlipColor, request.BlipName,
                request.VehicleRentModel, request.VehicleRentValue,
                request.VehicleRentPosX, request.VehicleRentPosY, request.VehicleRentPosZ,
                request.VehicleRentRotR, request.VehicleRentRotP, request.VehicleRentRotY);

            var context = Functions.GetDatabaseContext();
            context.Jobs.Update(job);
            await context.SaveChangesAsync();

            job.CreateIdentifier();

            await player.WriteLog(LogType.Staff, $"Gravar Emprego | {Functions.Serialize(job)}", null);
            player.SendNotification(NotificationType.Success, "Emprego editado.");

            var json = GetJobsJson();
            foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Jobs)))
                target.Emit("StaffJob:Update", json);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetJobsJson()
    {
        return Functions.Serialize(Global.Jobs
            .Select(x => new
            {
                x.Id,
                Name = x.CharacterJob.GetDescription(),
                x.PosX,
                x.PosY,
                x.PosZ,
                x.Salary,
                x.BlipType,
                x.BlipColor,
                x.BlipName,
                x.VehicleRentModel,
                x.VehicleRentValue,
                x.VehicleRentPosX,
                x.VehicleRentPosY,
                x.VehicleRentPosZ,
                x.VehicleRentRotR,
                x.VehicleRentRotP,
                x.VehicleRentRotY,
            })
            .OrderBy(x => x.Name));
    }
}