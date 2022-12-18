using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportsMockService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Controllers
{
    [Route("api/teams")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamRepository _teamRepo;

        public TeamsController(ITeamRepository teamRepo)
        {
            _teamRepo = teamRepo;
        }

        [HttpGet]
        [Route("allTeams")]
        public async Task<IActionResult> GetTeams()
        {
            try
            {
                var teams = await _teamRepo.GetTeams();
                return Ok(teams);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }
    }
}
