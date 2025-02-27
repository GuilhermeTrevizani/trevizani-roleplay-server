namespace TrevizaniRoleplay.Server.Models;

public class JobRequest
{
    public Guid Id { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public int Salary { get; set; }
    public uint BlipType { get; set; }
    public byte BlipColor { get; set; }
    public string BlipName { get; set; } = string.Empty;
    public string VehicleRentModel { get; set; } = string.Empty;
    public int VehicleRentValue { get; set; }
    public float VehicleRentPosX { get; set; }
    public float VehicleRentPosY { get; set; }
    public float VehicleRentPosZ { get; set; }
    public float VehicleRentRotR { get; set; }
    public float VehicleRentRotP { get; set; }
    public float VehicleRentRotY { get; set; }
}