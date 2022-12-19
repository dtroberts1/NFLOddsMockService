using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportsMockService.Contracts;
using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;

namespace SportsMockService.Controllers
{
    [Route("api/games")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly IGameRepository _gameRepo;
        private readonly IConfiguration _configuration;

        public GamesController(IGameRepository gameRepo, IConfiguration configuration)
        {
            _gameRepo = gameRepo;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("allGames")]
        public async Task<IActionResult> GetGames()
        {
            try
            {
                var games = await _gameRepo.GetGames();
                return Ok(games);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost]
        [Route("postGame")]
        public async Task<IActionResult> PostGame(Game newGame)
        {
            try
            {
                int gameId = await _gameRepo.AddGame(newGame);
                return Ok(gameId);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("putGame")]
        public async Task<IActionResult> PutGame(Game game)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var games = await _gameRepo.PutGame(game);
                }
                else
                {
                    return BadRequest("Model state is invalid");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }
    }
}
