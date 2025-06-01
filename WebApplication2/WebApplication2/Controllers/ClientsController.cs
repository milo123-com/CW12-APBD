namespace WebApplication2.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly TravelDbContext _context;

    public ClientsController(TravelDbContext context)
    {
        _context = context;
    }

    [HttpDelete("{idClient}")]
    public IActionResult DeleteClient(int idClient)
    {
        var client = _context.Clients.Include(c => c.ClientTrips).FirstOrDefault(c => c.IdClient == idClient);

        if (client == null)
            return NotFound("Client not found.");

        if (client.ClientTrips.Any())
            return BadRequest("Cannot delete client with assigned trips.");

        _context.Clients.Remove(client);
        _context.SaveChanges();

        return NoContent();
    }
}