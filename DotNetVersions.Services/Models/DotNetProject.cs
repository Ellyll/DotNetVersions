namespace DotNetVersions.Services.Models;

public record DotNetProject
{
    public required string RepositoryName { get; init; }
    public required string ProjectName { get; init; }
    public required string ProjectFile { get; init; }
    public required string DotNetVersion { get; init; }
};