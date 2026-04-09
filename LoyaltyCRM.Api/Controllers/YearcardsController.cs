using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
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

namespace LoyaltyCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YearcardsController : ControllerBase
    {
        private readonly IYearcardService _yearcardService;

        private readonly IMapper _mapper;

        public YearcardsController(IYearcardService yearcardService, IMapper mapper)
        {
            _yearcardService = yearcardService;
            _mapper = mapper;
        }

        //GET ALL YEARCARDS
        // GET: api/Yearcards
        [HttpGet]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult<IEnumerable<YearcardGetResponse>>> GetYearcards()
        {
            IEnumerable<Yearcard> yearcards = await _yearcardService.GetYearcards();

            var response = _mapper.Map<IEnumerable<YearcardGetResponse>>(yearcards);
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
                Yearcard yearcard = await _yearcardService.GetYearcard(id);

                var response = _mapper.Map<YearcardGetResponse>(yearcard);

                return response;   
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
                Yearcard updatedYearcard = _mapper.Map<Yearcard>(yearcard);

                Yearcard updated = await _yearcardService.UpdateYearcard(id, updatedYearcard);

                if (updated == null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest( new { message = ex.Message });
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
                Yearcard yearcard = _mapper.Map<Yearcard>(request);
                Yearcard createdYearcard = await _yearcardService.CreateOrExtendYearcard(yearcard, new StartDate(request.StartDate));

                var response = _mapper.Map<YearcardCreateResponse>(createdYearcard);

                return CreatedAtAction("PostYearcard", new { id = createdYearcard.Id }, response);
            }
            catch (ArgumentException ex)
            {
                // Bad Request for invalid arguments
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                // Conflict for database-related issues
                return Conflict(new { message = "A database error occurred while creating the yearcard.", details = ex.Message }); //TRANSLATE
            }
            catch (Exception ex)
            {
                // Internal Server Error for any other unhandled exceptions
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); //TRANSLATE
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
        public async Task<ActionResult<bool>> CheckInWithPhone(PhoneRequest phone)
        {
            try
            {
                PhoneNumber validatedNumber = new PhoneNumber(phone.phone!);
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
        public async Task<ActionResult<bool>> CheckInWithEmail(EmailRequest email)
        {
            try
            {
                Email validatedEmail = new Email(email.email!);
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
        public async Task<ActionResult<bool>> CheckInWithUserName(UserNameRequest userName)
        {
            try
            {
                UserName validatedUserName = new UserName(userName.userName!);
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

        public class PhoneRequest
        {
            public string? phone { get; set; }
        }

        public class EmailRequest
        {
            public string? email { get; set; }
        }

        public class UserNameRequest
        {
            public string? userName { get; set; }
        }

        public class NameRequest
        {
            public string? name { get; set; }
        }
    }
}
