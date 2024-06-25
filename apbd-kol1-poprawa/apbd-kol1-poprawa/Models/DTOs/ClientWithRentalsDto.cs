namespace apbd_kol1_poprawa.Models.DTOs;

public class ClientWithRentalsDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<RentalDto> Rentals { get; set; } = new List<RentalDto>();

}

public class RentalDto
{
    public string Vin { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int TotalPrice { get; set; }
}