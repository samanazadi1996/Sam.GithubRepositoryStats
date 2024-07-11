using GithubRepositoryStats.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GithubRepositoryStats
{
    public partial class MyToolWindowControl : UserControl
    {

        public GithubInfoDto GithubInfo = new GithubInfoDto();
        public MyToolWindowControl()
        {
            InitializeComponent();
        }

        private async void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            await GetInfoAsync();
            Sync();
            await VS.MessageBox.ShowAsync("GithubRepositoryStats", "Button clicked");
        }

        void Sync()
        {
            if (repoOwner != null && GithubInfo.Repository != null)
            {
                repoName.Content = $"Name: {GithubInfo.Repository.name}";
                repoOwner.Content = $"Owner: {GithubInfo.Repository.owner.login}";
                repoStars.Content = $"Stars: {GithubInfo.Repository.open_issues_count}";
                repoForks.Content = $"Forks: {GithubInfo.Repository.forks_count}";
                repoWatching.Content = $"Watching: {GithubInfo.Repository.watchers_count}";
                repoVisibility.Content = $"Visibility: {GithubInfo.Repository.visibility}";
                repoLanguage.Content = $"Language: {GithubInfo.Repository.language}";
                repoLicense.Content = $"License: {GithubInfo.Repository.license?.name ?? "None"}";
            }
        }
        async Task GetInfoAsync()
        {
            try
            {

                var sln = await VS.Solutions.GetCurrentSolutionAsync();

                var projectPath = GetDirectory(Directory.GetParent(sln.FullPath).FullName);

                string gitHubUrl = GetGitHubInfoFromConfig(projectPath);

                if (!string.IsNullOrEmpty(gitHubUrl))
                {
                    var (username, repoName) = ParseGitHubUrl(gitHubUrl);

                    GithubInfo.IsSuccess = true;
                    var url = $"https://api.github.com/repos/{username}/{repoName}";
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("GithubRepositoryStats");

                    var res = await client.GetStringAsync(url);
                    GithubInfo.Repository = JsonConvert.DeserializeObject<Repository>(res);
                }
                else
                {
                    GithubInfo.IsSuccess = false;
                }
            }
            catch (Exception)
            {
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

        private void repoOwner_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var url = GithubInfo.Repository?.owner?.html_url;
            OpenIfNotNull(url);
        }
        private void OpenIfNotNull(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch
                {
                }
            }

        }

        private void repoName_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var url = GithubInfo.Repository?.html_url;
            OpenIfNotNull(url);

        }
    }
}