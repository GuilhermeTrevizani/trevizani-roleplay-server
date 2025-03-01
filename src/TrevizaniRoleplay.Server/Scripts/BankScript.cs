using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class BankScript : Script
{
    [Command("atm")]
    public static void CMD_atm(MyPlayer player) => player.Emit("ATMCheck");

    [RemoteEvent(nameof(ATMUse))]
    public async Task ATMUse(Player playerParam, bool success)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            await Functions.ShowBank(player, true, success, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BankDeposit))]
    public async Task BankDeposit(Player playerParam, int value)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (value <= 0)
            {
                player.SendNotification(NotificationType.Error, "Valor inválido.");
                return;
            }

            if (player.Money < value)
            {
                player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, value));
                return;
            }

            await player.RemoveMoney(value);

            player.AddBank(value);

            player.SendNotification(NotificationType.Success, $"Você depositou ${value:N0}.");
            await player.WriteLog(LogType.Money, $"Depositar {value}", null);

            var financialTransaction = new FinancialTransaction();
            financialTransaction.Create(FinancialTransactionType.Deposit, player.Character.Id, value, "Depósito");
            var context = Functions.GetDatabaseContext();
            await context.FinancialTransactions.AddAsync(financialTransaction);

            await context.SaveChangesAsync();
            await Functions.ShowBank(player, true, true, true);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BankWithdraw))]
    public async Task BankWithdraw(Player playerParam, int value)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (value <= 0)
            {
                player.SendNotification(NotificationType.Error, "Valor inválido.");
                return;
            }

            if (player.Character.Bank < value)
            {
                player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughInYourBankAccount, value));
                return;
            }

            var res = await player.GiveMoney(value);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                return;
            }

            player.RemoveBank(value);

            player.SendNotification(NotificationType.Success, $"Você sacou ${value:N0}.");
            await player.WriteLog(LogType.Money, $"Sacar {value}", null);

            var financialTransaction = new FinancialTransaction();
            financialTransaction.Create(FinancialTransactionType.Withdraw, player.Character.Id, value, "Saque");
            var context = Functions.GetDatabaseContext();
            await context.FinancialTransactions.AddAsync(financialTransaction);
            await context.SaveChangesAsync();
            await Functions.ShowBank(player, true, true, true);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BankTransfer))]
    public async Task BankTransfer(Player playerParam, int bankAccount, int value, string description, bool confirm)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (value <= 0)
            {
                player.SendMessage(MessageType.Error, "Valor inválido.");
                return;
            }

            if (player.Character.Bank < value)
            {
                player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughInYourBankAccount, value));
                return;
            }

            description ??= string.Empty;
            if (description.Length < 1 || description.Length > 300)
            {
                player.SendMessage(MessageType.Error, "Descrição deve ter entre 1 e 300 caracteres.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var target = await context.Characters.FirstOrDefaultAsync(x => x.BankAccount == bankAccount);
            if (target is null)
            {
                player.SendMessage(MessageType.Error, $"Nenhum conta bancária foi encontrada com o código {bankAccount}.");
                return;
            }

            if (player.User.Id == target.UserId)
            {
                player.SendMessage(MessageType.Error, "Você não pode fazer uma transferência para outro personagem que seja seu.");
                await Functions.SendServerMessage($"{player.Character.Name} tentou fazer uma transferência para {target.Name} (mesmo usuário).", UserStaff.JuniorServerAdmin, true);
                return;
            }

            if (!confirm)
            {
                player.Emit("BankTransferConfirm", target.Name);
                return;
            }

            var playerTarget = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == target.Id);
            if (playerTarget is not null)
            {
                playerTarget.AddBank(value);
                playerTarget.SendMessage(MessageType.Success, $"{player.Character.Name} transferiu ${value:N0} para sua conta bancária.");
            }
            else
            {
                target.AddBank(value);
                context.Characters.Update(target);
            }

            player.RemoveBank(value);

            player.SendMessage(MessageType.Success, $"Você transferiu ${value:N0} para conta bancária {bankAccount} de {target.Name}.");
            await player.WriteLog(LogType.Money, $"Transferir {value} {target.Id}", playerTarget);

            if (!string.IsNullOrWhiteSpace(description))
                description = $" ({description})";

            var financialTransactionWithdraw = new FinancialTransaction();
            financialTransactionWithdraw.Create(FinancialTransactionType.Withdraw, player.Character.Id, value, $"Transferência para {target.Name}{description}");
            await context.FinancialTransactions.AddAsync(financialTransactionWithdraw);

            var financialTransactionDeposit = new FinancialTransaction();
            financialTransactionDeposit.Create(FinancialTransactionType.Deposit, target.Id, value, $"Transferência de {player.Character.Name}{description}");
            await context.FinancialTransactions.AddAsync(financialTransactionDeposit);

            await context.SaveChangesAsync();
            await Functions.ShowBank(player, true, true, true);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("transferir", "/transferir (conta bancária) (valor) (descrição)", GreedyArg = true)]
    public async Task CMD_transferir(MyPlayer player, int bankAccount, int value, string description)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.Character.JailFinalDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso pois está preso.");
            return;
        }

        if (player.CellphoneItem.FlightMode)
        {
            player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
            return;
        }

        await BankTransfer(player, bankAccount, value, description, true);
    }

    [RemoteEvent(nameof(BankPoliceTicketPayment))]
    public async Task BankPoliceTicketPayment(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var fine = await context.Fines.FirstOrDefaultAsync(x => x.Id == id);
            if (fine is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (player.Character.Bank < fine.Value)
            {
                player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughInYourBankAccount, fine.Value));
                return;
            }

            player.RemoveBank(fine.Value);

            fine.Pay();
            context.Fines.Update(fine);

            var financialTransaction = new FinancialTransaction();
            financialTransaction.Create(FinancialTransactionType.Withdraw, player.Character.Id, fine.Value, "Pagamento de Multa");
            await context.FinancialTransactions.AddAsync(financialTransaction);

            await context.SaveChangesAsync();

            player.SendNotification(NotificationType.Success, "Você pagou a multa.");
            await Functions.ShowBank(player, true, true, true);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}