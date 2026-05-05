using LoyaltyCRM.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCRM.Api.Controllers
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
