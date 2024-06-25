namespace apbd_kol1_poprawa.Models.DTOs;

public class AddClientWithRentalDto
{
    public AddClientDto Client { get; set; } = null!;
    public int CarId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}

public class AddClientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

