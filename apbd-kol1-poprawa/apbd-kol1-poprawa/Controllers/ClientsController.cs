using apbd_kol1_poprawa.Models.DTOs;
using apbd_kol1_poprawa.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace apbd_kol1_poprawa.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientsRepository _repository;

    public ClientsController(IClientsRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClientWithRentals(int id)
    {
        if (!await _repository.DoesClientExist(id))
        {
            return NotFound($"Client with Id == {id} does not exist.");
        }

        var clientWithRentals = await _repository.GetClientWithRentals(id);

        return Ok(clientWithRentals);
    }

    [HttpPost]
    public async Task<IActionResult> AddCarWithRental(AddClientWithRentalDto addClientWithRentalDto)
    {
        if (!await _repository.DoesCarExist(addClientWithRentalDto.CarId))
        {
            return NotFound($"Car with Id == {addClientWithRentalDto.CarId} does not exist.");
        }

        await _repository.AddClientWithRental(addClientWithRentalDto);

        return Created(Request.Path.Value ?? "api/clients", addClientWithRentalDto);
    }
}