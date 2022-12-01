using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportsMockService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Controllers
{
    [Route("api/statuses")]
    [ApiController]
    public class StatusesController : ControllerBase
    {
        private readonly IStatusRepository _statusRepo;

        public StatusesController(IStatusRepository statusRepo)
        {
            _statusRepo = statusRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatuses()
        {
            try
            {
                var statuses = await _statusRepo.GetStatuses();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }
    }
}
