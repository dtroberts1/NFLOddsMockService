using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SportsMockService.Contracts;
using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SportsMockService.Controllers
{
    [Route("api/gameodds")]
    [ApiController]
    public class GameOddsController : Controller
    {
        private readonly IGameOddRepository _outcomeRepo;

        public GameOddsController(IGameOddRepository outcomeRepo)
        {
            _outcomeRepo = outcomeRepo;
        }

        [HttpGet]
        [Route("getGameOdds")]
        public async Task<IActionResult> GetGameOdds(int? gameId)
        {
            try
            {
                var gameOdds = await _outcomeRepo.GetGameOdds(gameId);
                return Ok(gameOdds);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("postGameOdd")]
        public async Task<IActionResult> PostGameOdd(GameOdd gameOdd)
        {
            var result = await _outcomeRepo.AddGameOdd(gameOdd);
            return Ok(result);
        }

        [HttpPut]
        [Route("putGameOdd")]
        public async Task<IActionResult> PutGameOdd([FromBody] GameOdd gameOdd, [FromQuery] string changeCategory, [FromQuery] string innerCategory)
        {
            try
            {
                var result = await _outcomeRepo.UpdateGameOdd(gameOdd, changeCategory, innerCategory);
            }
            catch(Exception ex)
            {
                if(ex.Message.Contains("DOES NOT EXIST"))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest();
                }
            }
            return Ok();
        }
    }
}
