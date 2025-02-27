namespace TrevizaniRoleplay.Server.Models;

public enum AnimationFlags
{
    Loop = 1 << 0,
    StopOnLastFrame = 1 << 1,
    OnlyAnimateUpperBody = 1 << 4,
    AllowPlayerControl = 1 << 5,
    Cancellable = 1 << 7
}