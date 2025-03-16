using DotNetVersions.Services.Models;
using Octokit;
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml.Linq;

namespace DotNetVersions.Services.Services;

public class GitHubService(IOptions<GitHubServiceSettings> options) : IGitHubService
{
    private readonly GitHubServiceSettings _settings = options.Value;

    public async Task<IEnumerable<DotNetProject>> GetDotNetProjects()
    {
        var gitHubUsername = _settings.GitHubTargetUsername ?? throw new Exception($"{nameof(_settings.GitHubTargetUsername)} not set");
        var gitHubToken = _settings.GitHubToken ?? throw new Exception($"{nameof(_settings.GitHubToken)} not set");
        
        string[] projFiles = [".csproj", ".fsproj", ".vbproj"];
        
        var github = new GitHubClient(new ProductHeaderValue("DotNetVersions"));
        github.Credentials = new Credentials(gitHubToken);

        var repos = await github.Repository.GetAllForUser(gitHubUsername);
        var tasks = repos.Select(async repo =>
            {
                // Get the repository's file tree
                var treeResponse = await github.Git.Tree.GetRecursive(gitHubUsername, repo.Name, repo.DefaultBranch);
                
                // Filter for .NET proj files
                var projectFiles = treeResponse.Tree
                    .Where(file => projFiles.Any(pf => file.Path.EndsWith(pf)))
                    .Select(file => file.Path);

                var projectsTasks = projectFiles
                    .Select(async pf =>
                    {
                        // Get the file contents from the specified repository, branch, and file path
                        var fileContents = await github.Repository.Content.GetAllContentsByRef(gitHubUsername, repo.Name, pf, repo.DefaultBranch);

                        // Assuming you're dealing with a single file, return its content
                        // The content is Base64-encoded, so decode it
                        //var decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(fileContents[0].Content));

                        var dotNetVersion = fileContents[0].Content is null ? string.Empty : ExtractTargetVersion(fileContents[0].Content) ?? string.Empty;
                        
                        return new DotNetProject
                        {
                            RepositoryName = repo.Name,
                            ProjectName = Path.GetFileNameWithoutExtension(pf),
                            ProjectFile = pf,
                            DotNetVersion = dotNetVersion
                        };
                    });
                var projects = await Task.WhenAll(projectsTasks);
                
                return projects;
            });
        
        var projects = await Task.WhenAll(tasks);
        var dotnetProjects = projects.SelectMany(p => p);
        
        return dotnetProjects;
    }
    
    private static string? ExtractTargetVersion(string projectFileContents)
    {
        try
        {
            // Parse the project file contents as XML (removing BOM if needed)
            var projectXml = XDocument.Parse(projectFileContents.TrimStart(['\uFEFF']));

            // Check for .NET Framework TargetFrameworkVersion
            var targetFrameworkVersion = projectXml
                .Root?
                .Element("PropertyGroup")?
                .Element("TargetFrameworkVersion");

            if (targetFrameworkVersion != null)
            {
                return $"{targetFrameworkVersion.Value}";
            }

            // Check for .NET Core or .NET 5+ TargetFramework
            var targetFramework = projectXml
                .Root?
                .Element("PropertyGroup")?
                .Element("TargetFramework");

            if (targetFramework != null)
            {
                return targetFramework.Value;
            }

            // Handle cases where TargetFrameworks (plural) is used
            var targetFrameworks = projectXml
                .Root?
                .Element("PropertyGroup")?
                .Element("TargetFrameworks");

            if (targetFrameworks != null)
            {
                return $"{targetFrameworks.Value}";
            }

            return null;
        }
        catch (Exception ex)
        {
            return $"Error parsing project file: {ex.Message}";
        }
    }
}