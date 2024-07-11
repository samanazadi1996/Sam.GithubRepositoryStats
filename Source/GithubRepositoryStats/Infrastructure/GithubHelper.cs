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

namespace GithubRepositoryStats.Infrastructure
{
    public class GithubHelper
    {
        public static GithubInfoDto Data = new GithubInfoDto();
        public static event EventHandler<PropertyChangedEventArgs> StaticDataChanged;

        public async static Task<bool> RefreshInfoAsync()
        {

            try
            {
                var sln = await VS.Solutions.GetCurrentSolutionAsync();

                if (sln == null)
                {
                    return false;
                }

                var projectPath = GetDirectory(Directory.GetParent(sln.FullPath).FullName);
                string gitHubUrl = GetGitHubInfoFromConfig(projectPath);

                if (!string.IsNullOrEmpty(gitHubUrl))
                {
                    var (username, repoName) = ParseGitHubUrl(gitHubUrl);

                    GithubHelper.Data.IsSuccess = true;
                    var url = $"https://api.github.com/repos/{username}/{repoName}";
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("GithubRepositoryStats");

                    var res = client.GetStringAsync(url).Result;
                    GithubHelper.Data.Repository = JsonConvert.DeserializeObject<Repository>(res);

                    Data.Message = $"Last Update : {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}";
                }
                else
                {
                    GithubHelper.Data.IsSuccess = false;
                    Data.Message = "Repository not found on GitHub.";
                }
            }
            catch
            {
                GithubHelper.Data.IsSuccess = false;
                Data.Message = "An error occurred while fetching GitHub repository information.";

                StaticDataChanged?.Invoke(null, new PropertyChangedEventArgs("Data"));
                return false;
            }

            StaticDataChanged?.Invoke(null, new PropertyChangedEventArgs("Data"));
            return true;
        }

        static string GetDirectory(string path)
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

        static string GetGitHubInfoFromConfig(string projectPath)
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

        static (string, string) ParseGitHubUrl(string url)
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
