using System;
using System.ComponentModel.DataAnnotations;

namespace Common;

public class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
public class CredentialsSignIn
{
    [Required]
    public string? UserName { get; set; }
    [Required]
    public string? Password { get; set; }
}
public class CredentialsSignUp
{
    [Required]
    public string? UserName { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Telephone { get; set; }
    public bool isAdmin { get; set; } = false;
}
public class Roles
{
    public const string Admin = "admin";
    public const string User = "user";
}