using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Models;

public class WeatherInfo
{
    public List<WeatherInfoWeather> Weather { get; set; }
    public WeatherInfoMain Main { get; set; }
    public Weather WeatherType { get; set; } = GTANetworkAPI.Weather.CLEAR;
    public bool Manual { get; set; }
}

public class WeatherInfoWeather
{
    public string Main { get; set; }
}

public class WeatherInfoMain
{
    public float Temp { get; set; }
}