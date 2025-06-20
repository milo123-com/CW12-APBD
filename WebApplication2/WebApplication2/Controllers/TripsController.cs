﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
using WebApplication2.DTO;

namespace TravelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly TravelDbContext _context;

    public TripsController(TravelDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public IActionResult GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Trips
            .Include(t => t.CountryTrips)
                .ThenInclude(ct => ct.Country)
            .Include(t => t.ClientTrips)
                .ThenInclude(ct => ct.Client)
            .OrderByDescending(t => t.DateFrom);

        var totalCount = query.Count();

        var trips = query.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Name,
                t.Description,
                t.DateFrom,
                t.DateTo,
                t.MaxPeople,
                Countries = t.CountryTrips.Select(c => new { c.Country.Name }),
                Clients = t.ClientTrips.Select(c => new { c.Client.FirstName, c.Client.LastName })
            }).ToList();

        return Ok(new
        {
            pageNum = page,
            pageSize,
            allPages = (int)Math.Ceiling((double)totalCount / pageSize),
            trips
        });
    }


    [HttpPost("{idTrip}/clients")]
    public IActionResult AssignClient(int idTrip, [FromBody] AddClientDto dto)
    {
        if (_context.Clients.Any(c => c.Pesel == dto.Pesel))
            return BadRequest("Client with this PESEL already exists.");

        var trip = _context.Trips.FirstOrDefault(t => t.IdTrip == idTrip);
        if (trip == null)
            return NotFound("Trip not found.");

        if (trip.DateFrom <= DateTime.Now)
            return BadRequest("Trip already started or finished.");

        var client = new Client
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Telephone = dto.Telephone,
            Pesel = dto.Pesel
        };
        _context.Clients.Add(client);
        _context.SaveChanges();

        bool alreadyAssigned = _context.ClientTrips
            .Any(ct => ct.IdClient == client.IdClient && ct.IdTrip == idTrip);

        if (alreadyAssigned)
            return BadRequest("Client already assigned to this trip.");

        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = dto.PaymentDate
        };
        _context.ClientTrips.Add(clientTrip);
        _context.SaveChanges();

        return Ok("Client assigned to trip.");
    }
}
