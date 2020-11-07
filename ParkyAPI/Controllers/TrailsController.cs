using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Models;
using ParkyAPI.Models.Dtos;
using ParkyAPI.Repository.IRepository;

namespace ParkyAPI.Controllers
{
    [Route("api/v{version:apiVersion}/trails")]
    //[Route("api/[controller]")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "ParkyOpenAPISpecTrail")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class TrailsController : ControllerBase
    {

        private readonly ITrailRepository _trailRepo;
        private readonly IMapper _mapper;

        public TrailsController(ITrailRepository trailRepo, IMapper mapper)
        {
            _trailRepo = trailRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Get a list of all trails
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]", Name = "GetTrails")]
        [ProducesResponseType(200, Type = typeof(List<TrailDto>))]
        public IActionResult GetTrails()
        {
            var objList = _trailRepo.GetTrails();
            var objDtoList = new List<TrailDto>();

            foreach (var obj in objList)
            {
                objDtoList.Add(_mapper.Map<TrailDto>(obj));
            }
            return Ok(objDtoList);
        }

        /// <summary>
        /// Get specific trail
        /// </summary>
        /// <param name="trailId">specific trail ID</param>
        /// <returns></returns>
        [HttpGet("[action]/{trailId:int}", Name = "GetTrail")]
        [ProducesResponseType(200, Type = typeof(TrailDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [Authorize(Roles = "Admin")]
        public IActionResult GetTrail(int trailId)
        {
            var obj = _trailRepo.GetTrail(trailId);
            if (obj == null)
            {
                return NotFound();
            }
            var objDto = _mapper.Map<TrailDto>(obj);
            return Ok(objDto);
        }

        /// <summary>
        /// Get a list of trails from specific national park
        /// </summary>
        /// <param name="nationalParkId">specific national park ID</param>
        /// <returns></returns>
        [HttpGet("[action]/{nationalParkId:int}", Name = "GetTrailsInNationalPark")]
        [ProducesResponseType(200, Type = typeof(List<TrailDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult GetTrailsInNationalPark(int nationalParkId)
        {
            var objList = _trailRepo.GetTrailsInNationalPark(nationalParkId);
            if (objList == null)
            {
                return NotFound();
            }

            var objDtoList = new List<TrailDto>();
            foreach (var obj in objList)
            {
                objDtoList.Add(_mapper.Map<TrailDto>(obj));
            }
            return Ok(objDtoList);
        }

        /// <summary>
        /// Create a new trail to specific national park
        /// </summary>
        /// <param name="trailDto">trail DTO</param>
        /// <returns></returns>
        [HttpPost("[action]", Name = "CreateTrail")]
        [ProducesResponseType(201, Type = typeof(TrailDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateTrail([FromBody] TrailCreateDto trailDto)
        {
            if (trailDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_trailRepo.TrailExists(trailDto.Name))
            {
                ModelState.AddModelError("", "Trail Exists!");
                return StatusCode(404, ModelState);
            }

            var obj = _mapper.Map<Trail>(trailDto);
            if (!_trailRepo.CreateTrail(obj))
            {
                ModelState.AddModelError("", $"Something went wrong when saving the record {obj.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetTrail", new { trailId = obj.Id }, obj);
        }

        /// <summary>
        /// Update specific trail
        /// </summary>
        /// <param name="trailId">specific trail ID</param>
        /// <param name="trailDto">new trail object</param>
        /// <returns></returns>
        [HttpPatch("[action]/{trailId:int}", Name = "UpdateTrail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateTrail(int trailId, [FromBody] TrailUpdateDto trailDto)
        {
            if (trailDto == null || trailId != trailDto.Id)
            {
                return BadRequest(ModelState);
            }

            var obj = _mapper.Map<Trail>(trailDto);
            if (!_trailRepo.UpdateTrail(obj))
            {
                ModelState.AddModelError("", $"Something went wrong when updating the record {obj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete specific trail
        /// </summary>
        /// <param name="trailId">specific trail ID</param>
        /// <returns></returns>
        [HttpDelete("[action]/{trailId:int}", Name = "DeleteTrail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteTrail(int trailId)
        {
            if (!_trailRepo.TrailExists(trailId))
            {
                return NotFound();
            }

            var obj = _trailRepo.GetTrail(trailId);
            if (!_trailRepo.DeleteTrail(obj))
            {
                ModelState.AddModelError("", $"Something went wrong when deleting the record {obj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
