using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("premium")]
public class PremiumController(DatabaseContext context,
    IOptions<MercadoPagoSettings> mercadoPagoSettings) : BaseController(context)
{
    [HttpGet]
    public async Task<GetPremiumResponse> Get()
    {
        var premiumPointPurchase = await context.PremiumPointPurchases
            .FirstOrDefaultAsync(x => x.UserId == UserId && string.IsNullOrWhiteSpace(x.Status));

        var parameter = await context.Parameters.FirstOrDefaultAsync();

        var response = new GetPremiumResponse
        {
            Packages = Deserialize<List<GetPremiumResponse.Package>>(parameter!.PremiumPointPackagesJSON)!.OrderBy(x => x.Quantity),
            CurrentPurchaseName = premiumPointPurchase?.Name,
            CurrentPurchasePreferenceId = premiumPointPurchase?.PreferenceId,
            Items = Deserialize<List<GetPremiumResponse.Item>>(parameter!.PremiumItemsJSON)!.OrderBy(x => x.Value),
        };

        return response;
    }

    [HttpPost("request")]
    public async Task<string> CreateRequest([FromBody] CreatePremiumRequest request)
    {
        var targetUser = await context.Users.FirstOrDefaultAsync(x => x.DiscordUsername == request.UserName)
            ?? throw new ArgumentException($"Usuário {request.UserName} não encontrado.");

        var premiumPointPurchase = await context.PremiumPointPurchases
             .FirstOrDefaultAsync(x => x.UserId == UserId && string.IsNullOrWhiteSpace(x.Status));
        if (premiumPointPurchase is not null)
            throw new ArgumentException("Você já possui uma compra em andamento.");

        var parameter = await context.Parameters.FirstOrDefaultAsync();
        var packages = Deserialize<List<GetPremiumResponse.Package>>(parameter!.PremiumPointPackagesJSON)!;

        var package = packages.FirstOrDefault(x => x.Quantity == request.Quantity)
            ?? throw new ArgumentException("Pacote não encontrado.");

        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);

        premiumPointPurchase = new PremiumPointPurchase();

        MercadoPagoConfig.AccessToken = mercadoPagoSettings.Value.AccessToken;

        var packageName = $"{package.Name} para {targetUser.DiscordUsername}";

        var preferenceRequest = new PreferenceRequest
        {
            AutoReturn = "approved",
            BackUrls = new()
            {
                Failure = $"{mercadoPagoSettings.Value.BackUrl}3",
                Pending = $"{mercadoPagoSettings.Value.BackUrl}2",
                Success = $"{mercadoPagoSettings.Value.BackUrl}1",
            },
            Items =
            [
                new()
                {
                    Title = packageName,
                    Quantity = 1,
                    CurrencyId = "BRL",
                    UnitPrice = package.Value,
                    Description = user!.Name,
                },
            ],
            NotificationUrl = $"{mercadoPagoSettings.Value.TokenUrl}{premiumPointPurchase.Id}",
        };

        var client = new PreferenceClient();
        var preference = await client.CreateAsync(preferenceRequest);

        premiumPointPurchase.Create(UserId, packageName, package.Quantity, package.Value, preference.Id, targetUser.Id);

        await context.PremiumPointPurchases.AddAsync(premiumPointPurchase);
        await context.SaveChangesAsync();

        return preference.Id;
    }

    [HttpPost("webhook/{id}"), AllowAnonymous]
    public async Task Webhook(Guid id, [FromQuery] MercadoPagoWebhookRequest request)
    {
        if (request.Topic.ToLower() != "payment")
            return;

        var premiumPointPurchase = await context.PremiumPointPurchases.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException($"Id {id} inválido");

        var isAlreadyApproved = premiumPointPurchase.Status.ToLower() == "approved";

        MercadoPagoConfig.AccessToken = mercadoPagoSettings.Value.AccessToken;

        var client = new PaymentClient();
        var payment = await client.GetAsync(request.Id);

        premiumPointPurchase.UpdateStatus(payment.Status);

        context.PremiumPointPurchases.Update(premiumPointPurchase);

        if (!isAlreadyApproved && premiumPointPurchase.Status.ToLower() == "approved")
        {
            var ucpAction = new UCPAction();
            ucpAction.Create(UCPActionType.GivePremiumPoints, premiumPointPurchase.TargetUserId,
                Serialize(new UCPActionGivePremiumPointsRequest
                {
                    PremiumPointPurchaseId = premiumPointPurchase.Id,
                    Quantity = premiumPointPurchase.Quantity
                }));

            await context.UCPActions.AddAsync(ucpAction);
        }

        await context.SaveChangesAsync();
    }

    [HttpPost("cancel")]
    public async Task Cancel()
    {
        var premiumPointPurchase = await context.PremiumPointPurchases
             .FirstOrDefaultAsync(x => x.UserId == UserId && string.IsNullOrWhiteSpace(x.Status));
        if (premiumPointPurchase is null)
            return;

        premiumPointPurchase.UpdateStatus("canceledManually");
        context.PremiumPointPurchases.Update(premiumPointPurchase);
        await context.SaveChangesAsync();
    }

    [HttpGet("purchases"), Authorize(Policy = PolicySettings.POLICY_MANAGEMENT)]
    public async Task<IEnumerable<PremiumPointPurchaseResponse>> GetStaffers()
    {
        return await context.PremiumPointPurchases
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new PremiumPointPurchaseResponse
            {
                Quantity = x.Quantity,
                Value = x.Value,
                RegisterDate = x.RegisterDate,
                PaymentDate = x.PaymentDate,
                Status = x.Status,
                UserOrigin = $"{x.User!.Name} ({x.User.DiscordUsername})",
                UserTarget = $"{x.TargetUser!.Name} ({x.TargetUser.DiscordUsername})",
            })
            .ToListAsync();
    }
}