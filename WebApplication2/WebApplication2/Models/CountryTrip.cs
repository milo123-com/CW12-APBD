namespace WebApplication2.Models;

public class CountryTrip
{
    public int IdCountry { get; set; }
    public int IdTrip { get; set; }

    public Country Country { get; set; }
    public Trip Trip { get; set; }
}