using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PapasCRM_API.Authorization;
using PapasCRM_API.Requests;
using PapasCRM_API.Enums;
using PapasCRM_API.Models;
using PapasCRM_API.Repositories;
using System.Security.Claims;

namespace PapasCRM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerRepo _repo;

        public CustomersController(CustomerRepo repo)
        {
            _repo = repo;
        }
    }
}
