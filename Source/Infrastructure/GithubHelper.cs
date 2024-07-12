using GithubRepositoryStats.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace GithubRepositoryStats.Infrastructure
{
    public class GithubHelper
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticDataChanged;
        public static GithubInfoDto Data = new GithubInfoDto();

        public async static Task GetLanguagesAsync()
        {
            try
            {
                var url = Data.Repository?.languages_url;
                if (!string.IsNullOrEmpty(url))
                {
                    using HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("GithubRepositoryStats");

                    var res = await client.GetStringAsync(url);
                    var model = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);

                    Data.Repository.language = string.Join(" , ", model.Select(p => p.Key));
                }
            }
            catch
            {
            }
            finally
            {
                StaticDataChanged?.Invoke(null, new PropertyChangedEventArgs("Data"));
            }
        }
        public async static Task RefreshInfoAsync()
        {

            try
            {
                var sln = await VS.Solutions.GetCurrentSolutionAsync();

                if (sln == null) return;

                var projectPath = GetDirectory(Directory.GetParent(sln.FullPath).FullName);
                string gitHubUrl = GetGitHubInfoFromConfig(projectPath);

                if (!string.IsNullOrEmpty(gitHubUrl))
                {
                    var (username, repoName) = ParseGitHubUrl(gitHubUrl);

                    var url = $"https://api.github.com/repos/{username}/{repoName}";
                    using HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("GithubRepositoryStats");

                    var res = client.GetStringAsync(url).Result;

                    Data.Ok(JsonConvert.DeserializeObject<Repository>(res), $"Last Update : {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
                }
                else
                {
                    Data.Fail("Repository not found on GitHub.");
                }
            }
            catch
            {
                Data.Fail("An error occurred while fetching GitHub repository information.");
            }
            finally
            {
                StaticDataChanged?.Invoke(null, new PropertyChangedEventArgs("Data"));
            }
            string GetDirectory(string path)
            {
                while (path != null && Directory.Exists(path))
                {
                    if (File.Exists(Path.Combine(path, ".git", "config")))
                    {
                        return path;
                    }

                    var parentDirectory = Directory.GetParent(path);
                    path = parentDirectory?.FullName;
                }

                return null;
            }
            string GetGitHubInfoFromConfig(string projectPath)
            {
                string gitConfigPath = Path.Combine(projectPath, ".git", "config");
                if (!File.Exists(gitConfigPath))
                {
                    Console.WriteLine("The .git/config file does not exist. Make sure the path is correct and the project is a Git repository.");
                    return null;
                }

                string gitConfigContent = File.ReadAllText(gitConfigPath);
                string[] lines = gitConfigContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                bool remoteOriginSection = false;

                foreach (string line in lines)
                {
                    if (line.Trim() == "[remote \"origin\"]")
                    {
                        remoteOriginSection = true;
                    }
                    else if (remoteOriginSection && line.Trim().StartsWith("url = "))
                    {
                        string url = line.Trim().Substring("url = ".Length).Trim();
                        return url;
                    }
                }

                return null;
            }
            (string, string) ParseGitHubUrl(string url)
            {
                // Assuming the URL is in the format: https://github.com/username/repo.git or git@github.com:username/repo.git
                Uri uri;
                if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    var segments = uri.AbsolutePath.Trim('/').Split('/');
                    if (segments.Length >= 2)
                    {
                        return (segments[0], segments[1].Replace(".git", ""));
                    }
                }
                else
                {
                    // Handle SSH URL
                    var parts = url.Split(':');
                    if (parts.Length == 2)
                    {
                        var segments = parts[1].Split('/');
                        if (segments.Length >= 2)
                        {
                            return (segments[0], segments[1].Replace(".git", ""));
                        }
                    }
                }

                return (null, null);
            }
        }

    }
}
