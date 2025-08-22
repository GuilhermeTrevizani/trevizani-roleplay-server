using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("notifications")]
public class NotificationsController(DatabaseContext context) : BaseController(context)
{
    [HttpGet("mine")]
    public Task<List<NotificationResponse>> GetMine()
    {
        return context.Notifications
            .Where(x => x.UserId == UserId)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new NotificationResponse
            {
                Id = x.Id,
                Message = x.Message,
                Date = x.RegisterDate,
                ReadDate = x.ReadDate
            })
            .ToListAsync();
    }

    [HttpPost("mark-as-read/{id}")]
    public async Task MarkAsRead(Guid id)
    {
        var notification = await context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (!notification.ReadDate.HasValue)
        {
            notification.MarkAsRead();
            context.Notifications.Update(notification);
            await context.SaveChangesAsync();
        }
    }
}