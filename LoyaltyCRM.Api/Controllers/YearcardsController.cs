using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PapasCRM_API.Authorization;
using PapasCRM_API.Context;
using PapasCRM_API.DomainPrimitives;
using PapasCRM_API.Requests;
using PapasCRM_API.Requests.Yearcard;
using PapasCRM_API.Entities;
using PapasCRM_API.Enums;
using PapasCRM_API.Exceptions;
using PapasCRM_API.Mappers;
using PapasCRM_API.Models;
using PapasCRM_API.Repositories;
using PapasCRM_API.Repositories.Interfaces;
using static PapasCRM_API.Services.TranslationService;

namespace PapasCRM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YearcardsController : ControllerBase
    {
        private readonly IYearcardRepo _yearcardRepo;
        private readonly ICustomerRepo _customerRepo;

        private readonly IBarMapper _mapper;

        public YearcardsController(IYearcardRepo yearcardRepo, IBarMapper mapper, ICustomerRepo customerRepo)
        {
            _yearcardRepo = yearcardRepo;
            _customerRepo = customerRepo;
            _mapper = mapper;
        }

        //GET ALL YEARCARDS
        // GET: api/Yearcards
        [HttpGet]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult<IEnumerable<YearcardGetResponse>>> GetYearcards()
        {
            List<YearcardGetResponse> yearcards = new List<YearcardGetResponse>();
            IEnumerable<YearcardEntity> yearcardsEntities = await _yearcardRepo.GetYearcards();
            foreach (var yearcardEntity in yearcardsEntities)
            {
                YearcardGetResponse response = _mapper.ReflectTo<YearcardEntity, YearcardGetResponse>(yearcardEntity);
                response = _mapper.ReflectTo(yearcardEntity.User, response);
                response.ValidTo = yearcardEntity.ValidityIntervals.Any()
                    ? yearcardEntity.ValidityIntervals.Max(v => v.EndDate)
                    : new DateTime();
                foreach (ValidityIntervalEntity validityIntervalEntity in yearcardEntity.ValidityIntervals)
                {
                    ValidityIntervalResponseAndRequest validityIntervalResponse = _mapper.ReflectTo<ValidityIntervalEntity, ValidityIntervalResponseAndRequest>(validityIntervalEntity);
                    response.ValidityIntervals.Add(validityIntervalResponse);
                }
                yearcards.Add(response);
            }
            return Ok(yearcards);
        }

        //GET ALL UNCONFIRMED YEARCARDS
        [HttpGet("unconfirmed")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<IEnumerable<YearcardGetUnconfirmedResponse>>> GetUnconfirmedYearcards()
        {
            List<YearcardGetUnconfirmedResponse> yearcards = new List<YearcardGetUnconfirmedResponse>();
            IEnumerable<YearcardEntity> yearcardsEntities = await _yearcardRepo.GetUnconfirmedYearcards();
            foreach (var yearcardEntity in yearcardsEntities)
            {
                YearcardGetUnconfirmedResponse yearcard = _mapper.ReflectTo<YearcardEntity, YearcardGetUnconfirmedResponse>(yearcardEntity);
                yearcard = _mapper.ReflectTo(yearcardEntity.User, yearcard);
                yearcards.Add(yearcard);
            }
            return Ok(yearcards);
        }

        //GET SPECIFIC YEARCARD
        // GET: api/Yearcards/5
        [HttpGet("{id}")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<YearcardGetResponse>> GetYearcard(Guid id)
        {
            try
            {
                YearcardEntity yearcardEntity = await _yearcardRepo.GetYearcard(id);
                YearcardGetResponse response = _mapper.ReflectTo<YearcardEntity, YearcardGetResponse>(yearcardEntity);
                response = _mapper.ReflectTo(yearcardEntity.User, response);

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
                YearcardEntity updatedYearcard = _mapper.ReflectTo<YearcardUpdateRequest, YearcardEntity>(yearcard);
                updatedYearcard.User = _mapper.ReflectTo<YearcardUpdateRequest, ApplicationUserEntity>(yearcard);
                YearcardEntity updated = await _yearcardRepo.UpdateYearcard(id, updatedYearcard);

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
        public async Task<ActionResult<YearcardCreateResponse>> PostYearcard(YearcardCreateRequest yearcard)
        {
            try
            {
                ApplicationUserEntity newCustomer = _mapper.ReflectTo<YearcardCreateRequest, ApplicationUserEntity>(yearcard);
                ApplicationUserEntity Customer = await _customerRepo.CreateOrReturnFirstCustomer(newCustomer);

                if (Customer.Yearcard == null)
                {
                    YearcardEntity newYearcard = _mapper.ReflectTo<YearcardCreateRequest, YearcardEntity>(yearcard);
                    YearcardEntity NewYearCard = await _yearcardRepo.CreateYearcard(newYearcard, Customer.Id);

                    YearcardCreateResponse response = _mapper.ReflectTo<YearcardEntity, YearcardCreateResponse>(NewYearCard);
                    response = _mapper.ReflectTo(Customer, response);

                    return CreatedAtAction("GetYearcard", new { id = NewYearCard.Id }, response);
                }
                else
                {
                    Customer.Yearcard.StartDate = yearcard.StartDate;
                    await _yearcardRepo.AddValidityToCurrentYearcard(Customer.Yearcard);
                    YearcardCreateResponse response = _mapper.ReflectTo<YearcardEntity, YearcardCreateResponse>(Customer.Yearcard);
                    response = _mapper.ReflectTo(Customer, response);

                    return CreatedAtAction("GetYearcard", new { id = Customer.Yearcard.Id }, response);
                }
            }
            catch (ArgumentException ex)
            {
                // Bad Request for invalid arguments
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                // Conflict for database-related issues
                return Conflict(new { message = Translate("A database error occurred while creating the yearcard."), details = ex.Message });
            }
            catch (Exception ex)
            {
                // Internal Server Error for any other unhandled exceptions
                return StatusCode(500, new { message = Translate("An unexpected error occurred."), details = ex.Message });
            }

        }

        //CONFIRM UNCONFIRMED YEARCARD
        // PUT: api/Yearcards/5
        //TODO Only Bartenders or Admins should be allowed to do this.
        [HttpPut("confirm/{id}")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<IActionResult> ConfirmYearcard(Guid id)
        {
            try
            {
                bool isUpdated = await _yearcardRepo.ConfirmYearcard(id);

                if (!isUpdated)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (EntityNotFoundException notFound)
            {
                return NotFound(notFound.Message);
            }
            catch (DbUpdateException dbEx)
            {
                return Conflict(new { message = Translate("Database update error occurred."), details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //REJECT UNCONFIRMED YEARCARD
        // PUT: api/Yearcards/5
        //TODO Only Bartenders or Admins should be allowed to do this.
        [HttpPut("reject/{id}")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<IActionResult> RejectYearcard(Guid id)
        {
            try
            {
                bool isUpdated = await _yearcardRepo.RejectYearcard(id);

                return NoContent();
            }
            catch (EntityNotFoundException notFound)
            {
                return NotFound(notFound.Message);
            }
            catch (DbUpdateException dbEx)
            {
                return Conflict(new { message = Translate("Database update error occurred."), details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //DELETE YEARCARD
        // DELETE: api/Yearcards/5
        [HttpDelete("{id}")]
        [RequireRole(Role.Papa)]
        public async Task<IActionResult> DeleteYearcard(Guid id)
        {
            bool isDeleted = await _yearcardRepo.DeleteYearcard(id);
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
            return await _yearcardRepo.CheckInWithYearcards(id);
        }

        //CHECK IF YEARCARD IS VALID WITH PHONE
        [HttpPost("checkinphone")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<bool>> CheckInWithPhone(PhoneRequest phone)
        {
            try
            {
                PhoneNumber validatedNumber = new PhoneNumber(phone.phone!);
                return await _yearcardRepo.CheckInWithPhone(validatedNumber);      
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
                return await _yearcardRepo.CheckInWithEmail(validatedEmail);      
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
                return await _yearcardRepo.CheckInWithUserName(validatedUserName);      
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //CHECK IF YEARCARD IS VALID WITH NAME
        [HttpPost("checkinname")]
        [RequireRole(Role.Papa, Role.Bartender)]
        public async Task<ActionResult<IEnumerable<YearcardEntity>>> CheckInWithName(NameRequest name)
        {
            try
            {
                Name validatedName = new Name(name.name!);
                return Ok(await _yearcardRepo.CheckInWithName(validatedName.GetValue())); 
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
