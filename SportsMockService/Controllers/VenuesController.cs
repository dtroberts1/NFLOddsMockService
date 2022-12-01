using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportsMockService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Controllers
{
    [Route("api/venues")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueRepository _venueRepo;

        public VenuesController(IVenueRepository venueRepo)
        {
            _venueRepo = venueRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetVenues()
        {
            try
            {
                var venues = await _venueRepo.GetVenues();
                return Ok(venues);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }
    }
}
