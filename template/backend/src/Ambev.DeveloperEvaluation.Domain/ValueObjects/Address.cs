namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

public class Address
{
    public string City { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public int Number { get; private set; }
    public string Zipcode { get; private set; } = string.Empty;
    public Geolocation Geolocation { get; private set; } = new();

    protected Address() { }

    public Address(string city, string street, int number, string zipcode, Geolocation geolocation)
    {
        City = city;
        Street = street;
        Number = number;
        Zipcode = zipcode;
        Geolocation = geolocation;
    }
}

public class Geolocation
{
    public string Lat { get; private set; } = string.Empty;
    public string Long { get; private set; } = string.Empty;

    public Geolocation() { }

    public Geolocation(string lat, string @long)
    {
        Lat = lat;
        Long = @long;
    }
}
