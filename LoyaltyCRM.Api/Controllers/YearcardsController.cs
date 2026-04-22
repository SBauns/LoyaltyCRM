using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mapster;
using LoyaltyCRM.Authorization;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.Domain.Exceptions;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.DTOs.Requests.Yearcard;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyCRM.DTOs.Requests.Checkin;

namespace LoyaltyCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YearcardsController : ControllerBase
    {
        private readonly IYearcardService _yearcardService;

        public YearcardsController(IYearcardService yearcardService)
        {
            _yearcardService = yearcardService;
        }

        //GET ALL YEARCARDS
        // GET: api/Yearcards
        [HttpGet]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult<IEnumerable<YearcardGetResponse>>> GetYearcards()
        {
            var response = await _yearcardService.GetYearcards();

            return Ok(response);
        }

        //GET SPECIFIC YEARCARD
        // GET: api/Yearcards/5
        [HttpGet("{id}")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<YearcardGetResponse>> GetYearcard(Guid id)
        {
            try
            {
                return await _yearcardService.GetYearcard(id);
            }
            catch (EntityNotFoundException notFound)
            {
                return NotFound(notFound.Message);
            }

        }

        //UPDATE A YEARCARD
        // PUT: api/Yearcards/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [RequireRole(Role.Papa)]
        public async Task<IActionResult> PutYearcard(Guid id, YearcardUpdateRequest yearcard)
        {
            if (id != yearcard.Id)
            {
                return BadRequest();
            }
            try
            {
                Yearcard updated = await _yearcardService.UpdateYearcard(id, yearcard);

                if (updated == null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest( new { Code = ex.Message });
            }

        }

        //CREATE NEW YEARCARD OR REFRESH OLD ONE
        //POST: api/Yearcards
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<YearcardCreateResponse>> PostYearcard(YearcardCreateRequest request)
        {
            try
            {
                var response = await _yearcardService.CreateOrExtendYearcard(request);

                return CreatedAtAction("PostYearcard", new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                // Bad Request for invalid arguments
                return BadRequest(new { Code = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                // Conflict for database-related issues
                return Conflict(new { Code = "yearcard.database_error", details = ex.Message });
            }
            catch (Exception ex)
            {
                // Internal Server Error for any other unhandled exceptions
                return StatusCode(500, new { Code = "yearcard.unexpected_error", details = ex.Message });
            }

        }

        //DELETE YEARCARD
        // DELETE: api/Yearcards/5
        [HttpDelete("{id}")]
        [RequireRole(Role.Papa)]
        public async Task<IActionResult> DeleteYearcard(Guid id)
        {
            bool isDeleted = await _yearcardService.DeleteYearcard(id);
            if (!isDeleted)
            {
                return NotFound();
            }
            else
            {
                return NoContent();
            }
        }

        //TODO Consider endpoint for seeing if card is valid for discount (remember to use settings)

        //CHECK IF YEARCARD IS VALID
        [HttpPost("checkin/{id}")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<bool>> CheckInWithYearcard(Guid id)
        {
            return await _yearcardService.CheckInWithYearcards(id);
        }

        //CHECK IF YEARCARD IS VALID WITH PHONE
        [HttpPost("checkinphone")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<bool>> CheckInWithPhone(PhoneNumberCheckInRequest phone)
        {
            try
            {
                PhoneNumber validatedNumber = new PhoneNumber(phone.Phone!);
                return await _yearcardService.CheckInWithPhone(validatedNumber);      
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //CHECK IF YEARCARD IS VALID WITH Email
        [HttpPost("checkinemail")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<bool>> CheckInWithEmail(EmailCheckInRequest email)
        {
            try
            {
                Email validatedEmail = new Email(email.Email!);
                return await _yearcardService.CheckInWithEmail(validatedEmail);      
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //CHECK IF YEARCARD IS VALID WITH Email
        [HttpPost("checkinusername")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<bool>> CheckInWithUserName(UsernameCheckInRequest userName)
        {
            try
            {
                UserName validatedUserName = new UserName(userName.UserName!);
                return await _yearcardService.CheckInWithUserName(validatedUserName);      
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //CHECK IF YEARCARD IS VALID WITH NAME
        [HttpPost("checkinname")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<IEnumerable<Yearcard>>> CheckInWithName(NameRequest name)
        {
            try
            {
                Name validatedName = new Name(name.name!);
                return Ok(await _yearcardService.CheckInWithName(validatedName.Value)); //TODO CREATE A PROPER DTO HERE
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class NameRequest
        {
            public string? name { get; set; }
        }
    }
}
