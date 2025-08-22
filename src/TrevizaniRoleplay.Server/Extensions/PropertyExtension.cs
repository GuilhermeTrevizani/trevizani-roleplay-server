using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Extensions;

public static class PropertyExtension
{
    public static void CreateIdentifier(this Property property)
    {
        RemoveIdentifier(property);

        Functions.RunOnMainThread(() =>
        {
            var parentProperty = Global.Properties.FirstOrDefault(x => x.Id == property.ParentPropertyId);
            if (parentProperty?.Interior == PropertyInterior.Building)
                return;

            var pos = property.GetEntrancePosition();
            pos.Z -= 0.95f;

            var entranceMarker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, pos, Constants.MARKER_SCALE, Global.MainRgba, property.EntranceDimension);
            entranceMarker.PropertyId = property.Id;

            var isGarage = property.Interior == PropertyInterior.SmallGarage
                || property.Interior == PropertyInterior.MediumGarage
                || property.Interior == PropertyInterior.LargeGarage;
            var canBuy = property.Value > 0 && !property.CharacterId.HasValue;
            var tip = canBuy ?
                $"Use /comprar para comprar por ${property.Value:N0}."
                :
                (isGarage ? "Pressione Y para entrar apé ou K para entrar com veículos." : "Pressione Y para entrar.");

            var entranceColShape = Functions.CreateColShapeCylinder(pos, 1, 1.5f, property.EntranceDimension);
            entranceColShape.Description = $"[{property.FormatedAddress}] {{#FFFFFF}}{tip}";
            entranceColShape.PropertyId = property.Id;

            if (!canBuy)
            {
                foreach (var entrance in property.Entrances!)
                {
                    var position = entrance.GetEntrancePosition();
                    position.Z -= 0.95f;

                    var propertyEntranceMarker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, position, Constants.MARKER_SCALE, Global.MainRgba, property.EntranceDimension);
                    propertyEntranceMarker.PropertyId = property.Id;

                    var propertyEntranceColShape = Functions.CreateColShapeCylinder(position, 1, 1.5f, property.EntranceDimension);
                    propertyEntranceColShape.Description = $"[{property.FormatedAddress}] {{#FFFFFF}}{tip}";
                    propertyEntranceColShape.PropertyId = property.Id;
                }
            }
        });
    }

    public static void RemoveIdentifier(this Property property)
    {
        Functions.RunOnMainThread(() =>
        {
            foreach (var item in Global.Markers.Where(x => x.PropertyId == property.Id))
                item.Delete();

            foreach (var item in Global.ColShapes.Where(x => x.PropertyId == property.Id))
                item.Delete();
        });
    }

    public static void ShowInventory(this Property property, MyPlayer player, bool update)
    {
        player.ShowInventory(InventoryShowType.Property,
            property.FormatedAddress,
            Functions.Serialize(
                property.Items!.Select(x => new
                {
                    x.Id,
                    Image = x.GetImage(),
                    Name = x.GetName(),
                    x.Quantity,
                    x.Slot,
                    Extra = x.GetExtra(),
                    Weight = x.GetWeight(),
                    IsStack = x.GetIsStack(),
                })
        ), update, property.Id);
    }

    public static bool CanAccess(this Property property, MyPlayer player)
    {
        return property.CharacterId == player.Character.Id
            || (property.FactionId.HasValue && property.FactionId == player.Character.FactionId)
            || (property.CompanyId.HasValue && player.Companies.Any(y => y.Id == property.CompanyId))
            || player.PropertiesAccess.Contains(property.Id);
    }

    public static async Task ActivateProtection(this Property property)
    {
        if (property.ProtectionLevel >= 1)
            StartAlarm(property);

        if (property.ProtectionLevel >= 2)
        {
            var context = Functions.GetDatabaseContext();
            var targetCellphone = (await context.Characters.FirstOrDefaultAsync(x => x.Id == property.CharacterId))?.Cellphone ?? 0;
            if (targetCellphone != 0)
            {
                var phoneMessage = new PhoneMessage();
                phoneMessage.CreateTextToContact(Constants.EMERGENCY_NUMBER, targetCellphone,
                    $"O alarme de {property.FormatedAddress} foi acionado.");

                await Functions.SendSMS(null, [targetCellphone], phoneMessage);
            }
        }

        if (property.ProtectionLevel >= 3)
        {
            var emergencyCall = new EmergencyCall();
            emergencyCall.Create(EmergencyCallType.Police, Constants.EMERGENCY_NUMBER, property.EntrancePosX, property.EntrancePosY,
                $"O alarme de {property.FormatedAddress} foi acionado.", property.Address, property.Address);
            var context = Functions.GetDatabaseContext();
            await context.EmergencyCalls.AddAsync(emergencyCall);
            await context.SaveChangesAsync();
            Global.EmergencyCalls.Add(emergencyCall);
            await emergencyCall.SendMessage();
        }
    }

    public static void StartAlarm(this Property property)
    {
        if (Global.AudioSpots.Any(x => x?.PropertyId == property.Id))
            return;

        var parentProperty = Global.Properties.FirstOrDefault(x => x.Id == property.ParentPropertyId);
        if (parentProperty?.Interior != PropertyInterior.Building)
        {
            var exteriorAlarmAudioSpot = new AudioSpot
            {
                Position = property.GetEntrancePosition(),
                Dimension = property.EntranceDimension,
                Source = Constants.URL_AUDIO_PROPERTY_ALARM,
                Loop = true,
                PropertyId = property.Id,
                Range = 75,
            };
            exteriorAlarmAudioSpot.SetupAllClients();
        }

        var interiorAlarmAudioSpot = new AudioSpot
        {
            Position = property.GetExitPosition(),
            Dimension = property.Number,
            Source = Constants.URL_AUDIO_PROPERTY_ALARM,
            Loop = true,
            PropertyId = property.Id,
            Range = 75,
        };
        interiorAlarmAudioSpot.SetupAllClients();
    }

    public static void StopAlarm(this Property property)
    {
        do
        {
            Global.AudioSpots.FirstOrDefault(x => x?.PropertyId == property.Id)?.RemoveAllClients();
        } while (Global.AudioSpots.Any(x => x?.PropertyId == property.Id));
    }

    public static async Task ChangeOwner(this Property property, Guid? characterId)
    {
        if (characterId is null)
            property.RemoveOwner();
        else
            property.SetOwner(characterId.Value);

        var context = Functions.GetDatabaseContext();
        context.Properties.Update(property);

        property.CreateIdentifier();

        var linkedProperties = Global.Properties.Where(x => x.ParentPropertyId == property.Id && x.Value == 0);
        if (linkedProperties.Any())
        {
            foreach (var linkedProperty in linkedProperties)
            {
                if (characterId is null)
                    linkedProperty.RemoveOwner();
                else
                    linkedProperty.SetOwner(characterId.Value);

                linkedProperty.CreateIdentifier();

                await context.CharactersProperties
                    .Where(x => x.PropertyId == linkedProperty.Id)
                    .ExecuteDeleteAsync();
            }
            context.Properties.UpdateRange(linkedProperties);
        }

        await context.CharactersProperties
            .Where(x => x.PropertyId == property.Id)
            .ExecuteDeleteAsync();

        foreach (var player in Global.SpawnedPlayers)
            player.PropertiesAccess.RemoveAll(x => x == property.Id);

        await context.SaveChangesAsync();
    }

    public static AudioSpot? GetAudioSpot(this Property property)
        => Global.AudioSpots.FirstOrDefault(x => x?.PropertyId == property.Id && !x.Loop);

    public static async Task<(UserPremium, int)> GetOwnerPremium(this Property property)
    {
        if (property.FactionId.HasValue)
            return new(UserPremium.Gold, 0);

        Guid? ownerId = null;
        if (property.CharacterId.HasValue)
            ownerId = property.CharacterId;
        else if (property.CompanyId.HasValue)
            ownerId = Global.Companies.FirstOrDefault(x => x.Id == property.CompanyId)?.CharacterId;

        if (!ownerId.HasValue)
            return new(UserPremium.None, 0);

        var context = Functions.GetDatabaseContext();
        var character = await context.Characters
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == ownerId);
        if (character is null)
            return new(UserPremium.None, 0);

        return new(character.User!.GetCurrentPremium(), character.User.ExtraInteriorFurnitureSlots);
    }

    public static Vector3 GetEntrancePosition(this Property property)
        => new(property.EntrancePosX, property.EntrancePosY, property.EntrancePosZ);

    public static Vector3 GetEntranceRotation(this Property property)
        => new(property.EntranceRotR, property.EntranceRotP, property.EntranceRotY);

    public static Vector3 GetExitPosition(this Property property)
        => new(property.ExitPosX, property.ExitPosY, property.ExitPosZ);

    public static Vector3 GetExitRotation(this Property property)
        => new(property.ExitRotR, property.ExitRotP, property.ExitRotY);
}