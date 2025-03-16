using DotNetVersions.Services.Models;

namespace DotNetVersions.Services.Services;

public interface IGitHubService
{
    public Task<IEnumerable<DotNetProject>> GetDotNetProjects();
}