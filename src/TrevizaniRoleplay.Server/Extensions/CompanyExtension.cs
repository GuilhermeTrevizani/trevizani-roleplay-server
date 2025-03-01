using Discord;
using Discord.WebSocket;
using GTANetworkAPI;
using System.Drawing;
using TrevizaniRoleplay.Infra.Data;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Extensions;

public static class CompanyExtension
{
    public static bool GetIsOpen(this Company company)
    {
        return Global.MyBlips.FirstOrDefault(x => x.CompanyId == company.Id) is not null;
    }

    public static void CreateIdentifier(this Company company)
    {
        RemoveIdentifier(company);

        Functions.RunOnMainThread(() =>
        {
            if (!company.CharacterId.HasValue && company.WeekRentValue > 0)
            {
                var pos = new Vector3(company.PosX, company.PosY, company.PosZ - 0.95f);

                var marker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, pos, Constants.MARKER_SCALE, Global.MainRgba, 0);
                marker.CompanyId = company.Id;

                var colShape = Functions.CreateColShapeCylinder(pos, 1, 3, 0);
                colShape.Description = $"[{company.Name}] {{#FFFFFF}}Use /alugarempresa para alugar por ${company.WeekRentValue:N0} semanalmente.";
                colShape.CompanyId = company.Id;
            }

            if (company.Type == CompanyType.ConvenienceStore)
                ToggleOpen(company);
        });
    }

    public static void RemoveIdentifier(this Company company)
    {
        Functions.RunOnMainThread(() =>
        {
            var marker = Global.Markers.FirstOrDefault(x => x.CompanyId == company.Id);
            marker?.Delete();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.CompanyId == company.Id);
            colShape?.Delete();

            var blip = Global.MyBlips.FirstOrDefault(x => x.CompanyId == company.Id);
            blip?.Delete();
        });
    }

    public static async Task RemoveOwner(this Company company, DatabaseContext context)
    {
        company.ResetOwner();
        context.Companies.Update(company);
        await context.SaveChangesAsync();

        if (company.Characters!.Count != 0)
        {
            context.CompaniesCharacters.RemoveRange(company.Characters);
            await context.SaveChangesAsync();
            company.Characters.Clear();
        }

        CreateIdentifier(company);
    }

    public static void ToggleOpen(this Company company)
    {
        Functions.RunOnMainThread(() =>
        {
            if (company.Type == CompanyType.ConvenienceStore)
            {
                var hasOpen = Global.Companies.Any(x => x.Type == CompanyType.ConvenienceStore && x.EmployeeOnDuty.HasValue);
                foreach (var targetCompany in Global.Companies.Where(x => x.Type == CompanyType.ConvenienceStore))
                {
                    var targetBlip = Global.MyBlips.FirstOrDefault(x => x.CompanyId == targetCompany.Id);
                    targetBlip?.Delete();

                    if (!hasOpen || targetCompany.EmployeeOnDuty.HasValue)
                    {
                        targetBlip = Functions.CreateBlip(new(targetCompany.PosX, targetCompany.PosY, targetCompany.PosZ), targetCompany.BlipType,
                            Convert.ToByte(targetCompany.EmployeeOnDuty.HasValue ? targetCompany.BlipColor : 4), targetCompany.Name, 0.8f, true);
                        targetBlip.CompanyId = targetCompany.Id;
                    }
                }
                return;
            }

            var blip = Global.MyBlips.FirstOrDefault(x => x.CompanyId == company.Id);
            if (blip is null)
            {
                blip = Functions.CreateBlip(new(company.PosX, company.PosY, company.PosZ), company.BlipType, company.BlipColor, company.Name, 0.8f, true);
                blip.CompanyId = company.Id;
            }
            else
            {
                blip?.Delete();
            }
        });
    }

    public static async Task Announce(this Company company, MyPlayer player, string message)
    {
        message = Functions.CheckFinalDot(message);
        foreach (var target in Global.SpawnedPlayers.Where(x => !x.User.AnnouncementToggle))
            target.SendMessage(Models.MessageType.None, $"[{company.Name}] {message}", Constants.ANNOUNCEMENT_COLOR);
        await Functions.SendServerMessage($"{player.Character.Name} ({player.SessionId}) ({player.User.Name}) enviou o anúncio da empresa.", UserStaff.JuniorServerAdmin, false);

        Global.Announcements.Add(new()
        {
            Type = AnnouncementType.Company,
            Sender = company.Name,
            Message = message,
            PositionX = company.PosX,
            PositionY = company.PosY,
        });

        await player.WriteLog(LogType.CompanyAdvertisement, $"{company.Id} | {message}", null);

        if (Global.DiscordClient is null
            || Global.DiscordClient.GetChannel(Global.CompanyAnnouncementDiscordChannel) is not SocketTextChannel channel)
            return;

        var cor = ColorTranslator.FromHtml($"#{company.Color}");
        var embedBuilder = new EmbedBuilder
        {
            Title = company.Name,
            Description = message,
            Color = new Discord.Color(cor.R, cor.G, cor.B),
        };
        embedBuilder.WithFooter($"Enviado em {DateTime.Now}.");

        await channel.SendMessageAsync(embed: embedBuilder.Build());
    }

    public static void BuyItems(this Company company, MyPlayer player)
    {
        player.Emit("CompanyBuyItem:Show", company.Id.ToString(), company.Name, Functions.Serialize(
            company.Items
            !.Select(x => new
            {
                x.Id,
                Global.ItemsTemplates.FirstOrDefault(y => y.Id == x.ItemTemplateId)!.Name,
                Price = Functions.IsOwnedByState(company.Type) ? x.CostPrice : x.SellPrice,
            })
            .OrderBy(x => x.Name))
            );
    }

    public static void SellItems(this Company company, MyPlayer player)
    {
        player.Emit("CompanySellItem:Show", company.Id.ToString(), company.Name, Functions.Serialize(
            company.Items
            !.Select(x => new
            {
                x.Id,
                Global.ItemsTemplates.FirstOrDefault(y => y.Id == x.ItemTemplateId)!.Name,
                Price = x.CostPrice,
            })
            .OrderBy(x => x.Name))
            );
    }
}