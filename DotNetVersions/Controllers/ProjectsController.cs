using DotNetVersions.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetVersions.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectsController(IGitHubService gitHubService) : Controller
{
    private readonly IGitHubService _gitHubService = gitHubService;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var projects = await _gitHubService.GetDotNetProjects();
        return Json(projects);
    }
}