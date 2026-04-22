using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Authorization;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.DTOs.DTOs;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCRM.Api.Controllers
{
    [ApiController]
    [RequireRole(Role.Papa)]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var settings = await _settingsService.GetAllSettingsAsync();
            var dto = settings.Select(s => new SettingDto { Key = s.Key, Value = s.Value });
            return Ok(dto);
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> Upsert(string key, [FromBody] SettingDto request)
        {
            if (request == null)
                return BadRequest(new { Code = "setting.missing_body" });

            if (string.IsNullOrWhiteSpace(request.Value))
                return BadRequest(new { Code = "setting.missing_value" });

            try
            {
                var result = await _settingsService.UpsertSettingAsync(key, request.Value);
                return Ok(new SettingDto { Key = result.Key, Value = result.Value });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            var deleted = await _settingsService.DeleteSettingAsync(key);
            if (!deleted)
                return NotFound(new { Code = "setting.delete_failed" });

            return Ok(new { Code = "setting.setting_deleted" });
        }
    }
}