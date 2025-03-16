namespace DotNetVersions.Services.Services;

public class GitHubServiceSettings
{
    public required string GitHubTargetUsername { get; init; }
    public required string GitHubToken { get; init; }
}