using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffPropertyScript : Script
{
    [Command("int", "/int (tipo)")]
    public async Task CMD_int(MyPlayer player, byte type)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Properties))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!Enum.IsDefined(typeof(PropertyInterior), type))
        {
            player.SendMessage(MessageType.Error, "Tipo inválido.");
            return;
        }

        var propertyInterior = (PropertyInterior)type;
        if (propertyInterior == PropertyInterior.Personalized || propertyInterior == PropertyInterior.Building)
        {
            player.SendMessage(MessageType.Error, "Tipo não possui interior.");
            return;
        }

        player.SetPosition(GetExitPositionByInterior(propertyInterior), 0, false, Functions.GetIPLsByInterior(propertyInterior));
        await player.WriteLog(LogType.Staff, $"/int {type}", null);
    }

    [Command("irprop", "/irprop (número)")]
    public async Task CMD_irprop(MyPlayer player, int number)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == number);
        if (property is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhuma propriedade encontrada com o número {number}.");
            return;
        }

        player.SetPosition(property.GetEntrancePosition(), property.EntranceDimension, false);
        await player.WriteLog(LogType.Staff, $"/irprop {number}", null);
    }

    [Command("delprop", "/delprop (número)")]
    public async Task CMD_delprop(MyPlayer player, int number)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Properties))
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var property = Global.Properties.FirstOrDefault(x => x.Number == number);
        if (property is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhuma propriedade encontrada com o número {number}.");
            return;
        }

        if (property.CharacterId.HasValue)
        {
            player.SendMessage(MessageType.Error, "Propriedade possui um dono.");
            return;
        }

        if (property.Items!.Count > 0)
        {
            player.SendMessage(MessageType.Error, "Propriedade possui itens no armazenamento.");
            return;
        }

        if (property.Furnitures!.Count > 0)
        {
            player.SendMessage(MessageType.Error, "Propriedade possui mobílias.");
            return;
        }

        if (property.Entrances!.Count > 0)
        {
            player.SendMessage(MessageType.Error, "Propriedade possui entradas.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        context.Properties.Remove(property);
        await context.SaveChangesAsync();
        Global.Properties.Remove(property);
        property.RemoveIdentifier();
        await player.WriteLog(LogType.Staff, $"Remover Propriedade | {Functions.Serialize(property)}", null);
        player.SendMessage(MessageType.Success, "Propriedade excluída.");
    }

    [Command("rdonoprop", "/rdonoprop (número)")]
    public async Task CMD_rdonoprop(MyPlayer player, int number)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Properties))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var property = Global.Properties.FirstOrDefault(x => x.Number == number);
        if (property is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhuma propriedade encontrada com o número {number}.");
            return;
        }

        if (!property.CharacterId.HasValue)
        {
            player.SendMessage(MessageType.Error, $"Propriedade {number} não possui um dono.");
            return;
        }

        await property.ChangeOwner(null);
        await player.WriteLog(LogType.Staff, $"Remover Dono Propriedade {number}", null);
        player.SendMessage(MessageType.Success, "Dono da propriedade removido.");
    }

    [Command("criarpropriedade", "/criarpropriedade (interior) (valor)", Aliases = ["cprop"])]
    public static void CMD_criarpropriedade(MyPlayer player, int interior, int value)
    {
        player.Emit("CreateProperty", interior, value, string.Empty, null);
    }

    [Command("criarapartamento", "/criarapartamento (interior) (valor) (número propriedade mãe) (nome)", Aliases = ["cap"], GreedyArg = true)]
    public static void CMD_criarapartamento(MyPlayer player, int interior, int value, int parentPropertyName, string name)
    {
        player.Emit("CreateProperty", interior, value, name, parentPropertyName);
    }

    [Command("eprop", "/eprop (número)")]
    public static void CMD_eprop(MyPlayer player, int number)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Properties))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var property = Global.Properties.FirstOrDefault(x => x.Number == number);
        if (property is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhuma propriedade encontrada com o número {number}.");
            return;
        }

        var jsonInteriors = Functions.Serialize(
            Enum.GetValues<PropertyInterior>()
            .Select(x => new
            {
                Value = x,
                Label = x.ToString(),
            })
            .OrderBy(x => x.Label)
        );

        player.Emit("StaffProperty:Show", Functions.Serialize(new
        {
            property.Id,
            property.Number,
            property.Interior,
            InteriorDisplay = property.Interior.ToString(),
            property.Address,
            property.Value,
            property.EntranceDimension,
            property.EntrancePosX,
            property.EntrancePosY,
            property.EntrancePosZ,
            HasOwner = property.CharacterId.HasValue,
            FactionName = property.FactionId.HasValue ? Global.Factions.FirstOrDefault(y => y.Id == property.FactionId)?.Name : string.Empty,
            property.Name,
            property.ExitPosX,
            property.ExitPosY,
            property.ExitPosZ,
            property.EntranceRotR,
            property.EntranceRotP,
            property.EntranceRotY,
            property.ExitRotR,
            property.ExitRotP,
            property.ExitRotY,
            ParentPropertyNumber = Global.Properties.FirstOrDefault(y => y.Id == property.ParentPropertyId)?.Number,
            CompanyName = property.CompanyId.HasValue ? Global.Companies.FirstOrDefault(y => y.Id == property.CompanyId)?.Name : string.Empty,
        }), jsonInteriors);
    }

    [RemoteEvent(nameof(StaffPropertySave))]
    public async Task StaffPropertySave(Player playerParam, string json)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Properties))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var request = Functions.Deserialize<SavePropertyRequest>(json);

            if (!Enum.IsDefined(typeof(PropertyInterior), Convert.ToByte(request.Interior)))
            {
                player.SendNotification(NotificationType.Error, "Interior inválido.");
                return;
            }

            if (request.Value < 0)
            {
                player.SendNotification(NotificationType.Error, "Valor deve ser maior ou igual a 0.");
                return;
            }

            if (request.Name?.Length > 50)
            {
                player.SendNotification(NotificationType.Error, "Nome deve ter até 50 caracteres.");
                return;
            }

            Faction? faction = null;
            if (!string.IsNullOrWhiteSpace(request.FactionName))
            {
                faction = Global.Factions.FirstOrDefault(x => x.Name.ToLower() == request.FactionName.ToLower());
                if (faction is null)
                {
                    player.SendNotification(NotificationType.Error, $"Facção {request.FactionName} não encontrada.");
                    return;
                }
            }

            Property? parentProperty = null;
            if (request.ParentPropertyNumber > 0)
            {
                parentProperty = Global.Properties.FirstOrDefault(x => x.Number == request.ParentPropertyNumber);
                if (parentProperty is null)
                {
                    player.SendNotification(NotificationType.Error, $"Propriedade {request.ParentPropertyNumber} não encontrada.");
                    return;
                }
            }

            Company? company = null;
            if (!string.IsNullOrWhiteSpace(request.CompanyName))
            {
                company = Global.Companies.FirstOrDefault(x => x.Name.ToLower() == request.CompanyName.ToLower());
                if (company is null)
                {
                    player.SendNotification(NotificationType.Error, $"Empresa {request.CompanyName} não encontrada.");
                    return;
                }
            }

            var propertyInterior = (PropertyInterior)Convert.ToByte(request.Interior);

            if (faction is not null || company is not null || propertyInterior == PropertyInterior.Building)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    player.SendNotification(NotificationType.Error, "Nome é obrigatório.");
                    return;
                }

                if (request.Value > 0)
                {
                    player.SendNotification(NotificationType.Error, "Propriedade não pode ter preço pois é um prédio ou tem uma facção/empresa.");
                    return;
                }

                if (parentProperty is not null)
                {
                    player.SendNotification(NotificationType.Error, "Propriedade não pode ter uma propriedade mãe pois é um prédio ou tem uma facção/empresa.");
                    return;
                }

                if (faction is not null && company is not null)
                {
                    player.SendNotification(NotificationType.Error, "Propriedade deve ser da facção ou da empresa.");
                    return;
                }
            }

            if (parentProperty?.Interior == PropertyInterior.Building)
                request.EntrancePosition = new();

            var property = new Property();
            var id = request.Id;
            var isNew = request.Id is null;

            if (isNew)
            {
                request.ExitPosition = GetExitPositionByInterior(propertyInterior);

                property.Create(Global.Properties.Select(x => x.LockNumber).DefaultIfEmpty(0u).Max() + 1,
                    propertyInterior, request.EntrancePosition.X, request.EntrancePosition.Y, request.EntrancePosition.Z,
                    request.Dimension, request.Value, request.ExitPosition.X, request.ExitPosition.Y, request.ExitPosition.Z, request.Address,
                    Global.Properties.Select(x => x.Number).DefaultIfEmpty(0u).Max() + 1,
                    faction?.Id, request.Name, parentProperty?.Id,
                    request.EntranceRotation.X, request.EntranceRotation.Y, request.EntranceRotation.Z,
                    request.ExitRotation.X, request.ExitRotation.Y, request.ExitRotation.Z, company?.Id);
            }
            else
            {
                property = Global.Properties.FirstOrDefault(x => x.Id == id);
                if (property is null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                if (property.CharacterId.HasValue && faction is not null)
                {
                    player.SendNotification(NotificationType.Error, "Propriedade já possui um dono.");
                    return;
                }

                if (property.Interior != propertyInterior)
                    request.ExitPosition = GetExitPositionByInterior(propertyInterior);

                property.Update(propertyInterior, request.EntrancePosition.X, request.EntrancePosition.Y, request.EntrancePosition.Z,
                    request.Dimension, request.Value, request.ExitPosition.X, request.ExitPosition.Y, request.ExitPosition.Z, request.Address,
                    faction?.Id, request.Name, parentProperty?.Id,
                    request.EntranceRotation.X, request.EntranceRotation.Y, request.EntranceRotation.Z,
                    request.ExitRotation.X, request.ExitRotation.Y, request.ExitRotation.Z, company?.Id);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.Properties.AddAsync(property);
            else
                context.Properties.Update(property);

            await context.SaveChangesAsync();

            property.CreateIdentifier();

            if (isNew)
                Global.Properties.Add(property);

            await player.WriteLog(LogType.Staff, $"Editar Propriedade {property.Number} | {Functions.Serialize(new
            {
                property.Id,
                property.Interior,
                property.Value,
                property.EntranceDimension,
                property.EntrancePosX,
                property.EntrancePosY,
                property.EntrancePosZ,
                property.Address,
                property.FactionId,
                property.Name,
                property.ExitPosX,
                property.ExitPosY,
                property.ExitPosZ,
                property.CompanyId,
            })}", null);
            player.SendNotification(NotificationType.Success, $"Propriedade {(isNew ? "criada" : "editada")}.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("aarmazenamento")]
    public static void CMD_aarmazenamento(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Properties))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade.");
            return;
        }

        property.ShowInventory(player, false);
    }

    private static Vector3 GetExitPositionByInterior(PropertyInterior propertyInterior)
    {
        return propertyInterior switch
        {
            PropertyInterior.Motel => new(151.2564f, -1007.868f, -98.99999f),
            PropertyInterior.CasaBaixa => new(265.9522f, -1007.485f, -101.0085f),
            PropertyInterior.CasaMedia => new(346.4499f, -1012.996f, -99.19622f),
            PropertyInterior.IntegrityWay28 => new(-31.34092f, -594.9429f, 80.0309f),
            PropertyInterior.IntegrityWay30 => new(-17.61359f, -589.3938f, 90.11487f),
            PropertyInterior.DellPerroHeights4 => new(-1452.225f, -540.4642f, 74.04436f),
            PropertyInterior.DellPerroHeights7 => new(-1451.26f, -523.9634f, 56.92898f),
            PropertyInterior.RichardMajestic2 => new(-912.6351f, -364.9724f, 114.2748f),
            PropertyInterior.TinselTowers42 => new(-603.1113f, 58.93406f, 98.20017f),
            PropertyInterior.EclipseTowers3 => new(-785.1537f, 323.8156f, 211.9973f),
            PropertyInterior.WildOatsDrive3655 => new(-174.3753f, 497.3086f, 137.6669f),
            PropertyInterior.NorthConkerAvenue2044 => new(341.9306f, 437.7751f, 149.3901f),
            PropertyInterior.NorthConkerAvenue2045 => new(373.5803f, 423.7043f, 145.9078f),
            PropertyInterior.HillcrestAvenue2862 => new(-682.3693f, 592.2678f, 145.393f),
            PropertyInterior.HillcrestAvenue2868 => new(-758.4348f, 618.8454f, 144.1539f),
            PropertyInterior.HillcrestAvenue2874 => new(-859.7643f, 690.8358f, 152.8607f),
            PropertyInterior.WhispymoundDrive2677 => new(117.209f, 559.8086f, 184.3048f),
            PropertyInterior.MadWayneThunder2133 => new(-1289.775f, 449.3125f, 97.90256f),
            PropertyInterior.Modern1Apartment => new(-786.8663f, 315.7642f, 217.6385f),
            PropertyInterior.Modern2Apartment => new(-786.9563f, 315.6229f, 187.9136f),
            PropertyInterior.Modern3Apartment => new(-774.0126f, 342.0428f, 196.6864f),
            PropertyInterior.Mody1Apartment => new(-787.0749f, 315.8198f, 217.6386f),
            PropertyInterior.Mody2Apartment => new(-786.8195f, 315.5634f, 187.9137f),
            PropertyInterior.Mody3Apartment => new(-774.1382f, 342.0316f, 196.6864f),
            PropertyInterior.Vibrant1Apartment => new(-786.6245f, 315.6175f, 217.6385f),
            PropertyInterior.Vibrant2Apartment => new(-786.9584f, 315.7974f, 187.9135f),
            PropertyInterior.Vibrant3Apartment => new(-774.0223f, 342.1718f, 196.6863f),
            PropertyInterior.Sharp1Apartment => new(-787.0902f, 315.7039f, 217.6384f),
            PropertyInterior.Sharp2Apartment => new(-787.0155f, 315.7071f, 187.9135f),
            PropertyInterior.Sharp3Apartment => new(-773.8976f, 342.1525f, 196.6863f),
            PropertyInterior.Monochrome1Apartment => new(-786.9887f, 315.7393f, 217.6386f),
            PropertyInterior.Monochrome2Apartment => new(-786.8809f, 315.6634f, 187.9136f),
            PropertyInterior.Monochrome3Apartment => new(-774.0675f, 342.0773f, 196.6864f),
            PropertyInterior.Seductive1Apartment => new(-787.1423f, 315.6943f, 217.6384f),
            PropertyInterior.Seductive2Apartment => new(-787.0961f, 315.815f, 187.9135f),
            PropertyInterior.Seductive3Apartment => new(-773.9552f, 341.9892f, 196.6862f),
            PropertyInterior.Regal1Apartment => new(-787.029f, 315.7113f, 217.6385f),
            PropertyInterior.Regal2Apartment => new(-787.0574f, 315.6567f, 187.9135f),
            PropertyInterior.Regal3Apartment => new(-774.0109f, 342.0965f, 196.6863f),
            PropertyInterior.Aqua1Apartment => new(-786.9469f, 315.5655f, 217.6383f),
            PropertyInterior.Aqua2Apartment => new(-786.9756f, 315.723f, 187.9134f),
            PropertyInterior.Aqua3Apartment => new(-774.0349f, 342.0296f, 196.6862f),
            PropertyInterior.ArcadiusExecutiveRich => new(-141.1987f, -620.913f, 168.8205f),
            PropertyInterior.ArcadiusExecutiveCool => new(-141.5429f, -620.9524f, 168.8204f),
            PropertyInterior.ArcadiusExecutiveContrast => new(-141.2896f, -620.9618f, 168.8204f),
            PropertyInterior.ArcadiusOldSpiceWarm => new(-141.4966f, -620.8292f, 168.8204f),
            PropertyInterior.ArcadiusOldSpiceClassical => new(-141.3997f, -620.9006f, 168.8204f),
            PropertyInterior.ArcadiusOldSpiceVintage => new(-141.5361f, -620.9186f, 168.8204f),
            PropertyInterior.ArcadiusPowerBrokerIce => new(-141.392f, -621.0451f, 168.8204f),
            PropertyInterior.ArcadiusPowerBrokeConservative => new(-141.1945f, -620.8729f, 168.8204f),
            PropertyInterior.ArcadiusPowerBrokePolished => new(-141.4924f, -621.0035f, 168.8205f),
            PropertyInterior.MazeBankExecutiveRich => new(-75.8466f, -826.9893f, 243.3859f),
            PropertyInterior.MazeBankExecutiveCool => new(-75.49945f, -827.05f, 243.386f),
            PropertyInterior.MazeBankExecutiveContrast => new(-75.49827f, -827.1889f, 243.386f),
            PropertyInterior.MazeBankOldSpiceWarm => new(-75.44054f, -827.1487f, 243.3859f),
            PropertyInterior.MazeBankOldSpiceClassical => new(-75.63942f, -827.1022f, 243.3859f),
            PropertyInterior.MazeBankOldSpiceVintage => new(-75.47446f, -827.2621f, 243.386f),
            PropertyInterior.MazeBankPowerBrokerIce => new(-75.56978f, -827.1152f, 243.3859f),
            PropertyInterior.MazeBankPowerBrokeConservative => new(-75.51953f, -827.0786f, 243.3859f),
            PropertyInterior.MazeBankPowerBrokePolished => new(-75.41915f, -827.1118f, 243.3858f),
            PropertyInterior.LomBankExecutiveRich => new(-1579.756f, -565.0661f, 108.523f),
            PropertyInterior.LomBankExecutiveCool => new(-1579.678f, -565.0034f, 108.5229f),
            PropertyInterior.LomBankExecutiveContrast => new(-1579.583f, -565.0399f, 108.5229f),
            PropertyInterior.LomBankOldSpiceWarm => new(-1579.702f, -565.0366f, 108.5229f),
            PropertyInterior.LomBankOldSpiceClassical => new(-1579.643f, -564.9685f, 108.5229f),
            PropertyInterior.LomBankOldSpiceVintage => new(-1579.681f, -565.0003f, 108.523f),
            PropertyInterior.LomBankPowerBrokerIce => new(-1579.677f, -565.0689f, 108.5229f),
            PropertyInterior.LomBankPowerBrokeConservative => new(-1579.708f, -564.9634f, 108.5229f),
            PropertyInterior.LomBankPowerBrokePolished => new(-1579.693f, -564.8981f, 108.5229f),
            PropertyInterior.MazeBankWestExecutiveRich => new(-1392.667f, -480.4736f, 72.04217f),
            PropertyInterior.MazeBankWestExecutiveCool => new(-1392.542f, -480.4011f, 72.04211f),
            PropertyInterior.MazeBankWestExecutiveContrast => new(-1392.626f, -480.4856f, 72.04212f),
            PropertyInterior.MazeBankWestOldSpiceWarm => new(-1392.617f, -480.6363f, 72.04208f),
            PropertyInterior.MazeBankWestOldSpiceClassical => new(-1392.532f, -480.7649f, 72.04207f),
            PropertyInterior.MazeBankWestOldSpiceVintage => new(-1392.611f, -480.5562f, 72.04214f),
            PropertyInterior.MazeBankWestPowerBrokerIce => new(-1392.563f, -480.549f, 72.0421f),
            PropertyInterior.MazeBankWestPowerBrokeConservative => new(-1392.528f, -480.475f, 72.04206f),
            PropertyInterior.MazeBankWestPowerBrokePolished => new(-1392.416f, -480.7485f, 72.04207f),
            PropertyInterior.Clubhouse1 => new(1110.145f, -3166.932f, -37.529663f),
            PropertyInterior.Clubhouse2 => new(997.2791f, -3164.4395f, -38.911377f),
            PropertyInterior.MethLab => new(996.8967f, -3200.6902f, -36.400757f),
            PropertyInterior.WeedFarm => new(1066.2594f, -3183.521f, -39.164062f),
            PropertyInterior.CocaineLockup => new(1088.6901f, -3187.5562f, -38.995605f),
            PropertyInterior.CounterfeitCashFactory => new(1138.1143f, -3199.1472f, -39.669556f),
            PropertyInterior.DocumentForgeryOffice => new(1173.7451f, -3196.6682f, -39.01245f),
            PropertyInterior.WarehouseSmall => new(1087.4374f, -3099.323f, -39.01245f),
            PropertyInterior.WarehouseMedium => new(1048.0352f, -3097.1077f, -39.01245f),
            PropertyInterior.WarehouseLarge => new(1027.7803f, -3101.6309f, -39.01245f),
            PropertyInterior.SmallGarage => new(173.2903f, -1003.6f, -99.65707f),
            PropertyInterior.MediumGarage => new(197.8153f, -1002.293f, -99.65749f),
            PropertyInterior.LargeGarage => new(229.9559f, -981.7928f, -99.66071f),
            PropertyInterior.Morgue => new(275.446f, -1361.11f, 24.5378f),
            _ => new Vector3(),
        };
    }
}