using LoyaltyCRM.Api.Extensions;
using LoyaltyCRM.Application.Models;
using LoyaltyCRM.Application.Services;
using LoyaltyCRM.Authorization;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCRM.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TermsController : ControllerBase
{
    private const int MaxHtmlLength = 512 * 1024; // 512 KB is generous for a legal document

    private readonly ITermsService termsService;
    private readonly ILogger<TermsController> logger;

    public TermsController(ITermsService termsService, ILogger<TermsController> logger)
    {
        this.termsService = termsService;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTerms([FromServices] ITermsService service, CancellationToken cancellationToken)
    {
        TermsDocument? current = await termsService.GetCurrentAsync(cancellationToken);

        if (current is null)
        {
            return NotFound("Terms not found.");
        }

        return Content(current.Html, "text/html");
    }

    [HttpPut]
    [Consumes("text/html", "text/plain", "application/json")]
    [RequireRole(Role.Papa)]
    public async Task<IActionResult> UpdateTerms(CancellationToken cancellationToken)
    {
        string html = await Request.ReadBodyAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(html))
        {
            return BadRequest("Cannot update empty terms.");
        }

        if (html.Length > MaxHtmlLength)
        {
            return BadRequest($"Terms exceed maximum length of {MaxHtmlLength} characters.");
        }

        try
        {
            TermsDocument newVersion = await termsService.PublishNewVersionAsync(
                html,
                cancellationToken);

            return Created($"/api/terms/{newVersion.Id}", "Terms published.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to publish terms version {Length} chars", html.Length);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while publishing the terms.");
        }
    }

        [HttpGet("history")]
    [RequireRole(Role.Papa)]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        IReadOnlyList<TermsVersionSummary> history = await termsService.GetVersionHistoryAsync(cancellationToken);
        return Ok(history);
    }

    [HttpGet("{id:int}")]
    [RequireRole(Role.Papa)]
    public async Task<IActionResult> GetVersion(int id, CancellationToken cancellationToken)
    {
        TermsDocument? version = await termsService.GetVersionAsync(id, cancellationToken);

        if (version is null)
        {
            return NotFound("Version not found.");
        }

        return Content(version.Html, "text/html");
    }

    [HttpPut("{id:int}/current")]
    [RequireRole(Role.Papa)]
    public async Task<IActionResult> SetCurrentVersion(int id, CancellationToken cancellationToken)
    {
        TermsDocument? newCurrent = await termsService.SetCurrentVersionAsync(id, cancellationToken);

        if (newCurrent is null)
        {
            return NotFound("Version not found.");
        }

        return Ok(new { newCurrent.Id, newCurrent.CreatedAtUtc });
    }

    [HttpDelete("{id:int}")]
    [RequireRole(Role.Papa)]
    public async Task<IActionResult> DeleteVersion(int id, CancellationToken cancellationToken)
    {
        TermsDeleteResult result = await termsService.DeleteVersionAsync(id, cancellationToken);

        switch (result)
        {
            case TermsDeleteResult.Deleted:
                return Ok("Version deleted.");
            case TermsDeleteResult.NotFound:
                return NotFound("Version not found.");
            case TermsDeleteResult.IsCurrentVersion:
                return Conflict("The current version cannot be deleted. Make another version current first.");
            default:
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the version.");
        }
    }
}