namespace WebApplication2.Models;

public class Country
{
    public int IdCountry { get; set; }
    public string Name { get; set; }

    public ICollection<CountryTrip> CountryTrips { get; set; }
}