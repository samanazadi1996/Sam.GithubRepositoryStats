using GithubRepositoryStats.Infrastructure;
using GithubRepositoryStats.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GithubRepositoryStats
{

    public partial class MyToolWindowControl : UserControl
    {
        public MyToolWindowControl()
        {
            InitializeComponent();
            GithubHelper.StaticDataChanged += StaticDataChanged;
        }

        private void StaticDataChanged(object sender, PropertyChangedEventArgs e)
        {
            Sync();
        }

        private async void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            await GithubHelper.RefreshInfoAsync();
        }

        private void repoOwner_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Helper.OpenIfNotNull(GithubHelper.Data.Repository?.owner?.html_url);
        }

        private void repoName_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Helper.OpenIfNotNull(GithubHelper.Data.Repository?.html_url);
        }

        private async void repoLanguage_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var url = GithubHelper.Data.Repository?.languages_url;
                if (!string.IsNullOrEmpty(url))
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("GithubRepositoryStats");

                    var res = await client.GetStringAsync(url);
                    var model = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);

                    if (repoOwner != null && GithubHelper.Data.Repository != null)
                    {
                        repoLanguage.Content = $"Language: {string.Join(" , ", model.Select(p => p.Key))}";
                    }
                }
            }
            catch
            {
            }
        }

        private void repoForks_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Helper.OpenIfNotNull(GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/forks"));
        }

        private void repoWatching_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Helper.OpenIfNotNull(GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/watchers"));
        }

        private void repoStars_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Helper.OpenIfNotNull(GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/stargazers"));
        }

        void Sync()
        {
            if (repoOwner != null && GithubHelper.Data.Repository != null)
            {
                textMessage.Text = GithubHelper.Data.Message;
                repoData.Visibility = GithubHelper.Data.IsSuccess ? Visibility.Visible : Visibility.Hidden;
                if (GithubHelper.Data.IsSuccess)
                {
                    repoName.Content = $"Name: {GithubHelper.Data.Repository.name}";
                    repoOwner.Content = $"Owner: {GithubHelper.Data.Repository.owner.login}";
                    repoStars.Content = $"Stars: {GithubHelper.Data.Repository.stargazers_count}";
                    repoForks.Content = $"Forks: {GithubHelper.Data.Repository.forks_count}";
                    repoWatching.Content = $"Watching: {GithubHelper.Data.Repository.watchers_count}";
                    repoVisibility.Content = $"Visibility: {GithubHelper.Data.Repository.visibility}";
                    repoLanguage.Content = $"Language: {GithubHelper.Data.Repository.language}";
                    repoLicense.Content = $"License: {GithubHelper.Data.Repository.license?.name ?? "None"}";
                }
            }
        }

    }
}
