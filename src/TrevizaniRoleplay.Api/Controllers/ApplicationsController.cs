using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("applications")]
public class ApplicationsController(DatabaseContext context) : BaseController(context)
{
    [HttpGet, Authorize(Policy = PolicySettings.POLICY_TESTER)]
    public async Task<IEnumerable<ApplicationListResponse>> GetAll()
    {
        var apps = (await context.Characters
               .Where(x => !x.EvaluatorStaffUserId.HasValue)
               .Include(x => x.User)
               .Include(x => x.EvaluatingStaffUser)
               .ToListAsync())
               .OrderByDescending(x => x.User!.GetCurrentPremium())
               .ThenBy(x => x.RegisterDate);

        return apps.Select(x => new ApplicationListResponse
        {
            Name = x.Name,
            UserName = x.User!.Name,
            StafferName = x.EvaluatingStaffUser is not null ? x.EvaluatingStaffUser.Name : string.Empty,
            Date = x.RegisterDate,
        });
    }

    [HttpGet("current"), Authorize(Policy = PolicySettings.POLICY_TESTER)]
    public async Task<ApplicationResponse> GetCurrent()
    {
        static ApplicationResponse ConvertResponse(Character app)
        {
            return new()
            {
                Name = app.Name,
                Date = app.RegisterDate,
                History = app.History,
                Sex = app.Sex.GetDescription(),
                UserName = app.User!.Name,
                Age = app.Age,
            };
        }

        var app = await context.Characters
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.EvaluatingStaffUserId == UserId);
        if (app is not null)
            return ConvertResponse(app);

        app = (await context.Characters
                .Where(x => !x.EvaluatorStaffUserId.HasValue && !x.EvaluatingStaffUserId.HasValue)
                .Include(x => x.User)
                .ToListAsync())
                .OrderByDescending(x => x.User!.GetCurrentPremium())
                .ThenBy(x => x.RegisterDate)
                .FirstOrDefault();
        if (app is not null)
        {
            app.SetEvaluatingStaffUser(UserId);
            context.Characters.Update(app);

            var ucpAction = new UCPAction();
            ucpAction.Create(UCPActionType.AddCharacterApplicationsQuantity, UserId, string.Empty);
            await context.UCPActions.AddAsync(ucpAction);

            await context.SaveChangesAsync();

            return ConvertResponse(app);
        }


        throw new ArgumentException("Nenhuma aplicação está aguardando avaliação.");
    }

    [HttpPost("accept"), Authorize(Policy = PolicySettings.POLICY_TESTER)]
    public async Task Accept()
    {
        var app = await context.Characters
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.EvaluatingStaffUserId == UserId)
            ?? throw new ArgumentException("Você não está avaliando uma aplicação.");

        app.AcceptAplication(UserId);
        context.Characters.Update(app);

        var notification = new Notification(app.User!.Id, $"A aplicação do seu personagem {app.Name} foi aceita.");
        await context.Notifications.AddAsync(notification);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.SendDiscordMessage, UserId, Serialize(new UCPActionSendDiscordMessageRequest
        {
            DiscordUserId = Convert.ToUInt64(app.User!.DiscordId),
            Message = notification.Message,
        }));
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
    }

    [HttpPost("deny"), Authorize(Policy = PolicySettings.POLICY_TESTER)]
    public async Task Deny([FromBody] DenyApplicationRequest request)
    {
        if (request.Reason.Length < 1 || request.Reason.Length > 1000)
            throw new ArgumentException("Motivo deve ter entre 1 e 1000 caracteres.");

        var app = await context.Characters
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.EvaluatingStaffUserId == UserId)
            ?? throw new ArgumentException("Você não está avaliando uma aplicação.");

        app.RejectApplication(UserId, request.Reason);
        context.Characters.Update(app);

        var notification = new Notification(app.User!.Id, $"A aplicação do seu personagem {app.Name} foi negada.");
        await context.Notifications.AddAsync(notification);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.SendDiscordMessage, UserId, Serialize(new UCPActionSendDiscordMessageRequest
        {
            DiscordUserId = Convert.ToUInt64(app.User!.DiscordId),
            Message = notification.Message,
        }));
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
    }
}