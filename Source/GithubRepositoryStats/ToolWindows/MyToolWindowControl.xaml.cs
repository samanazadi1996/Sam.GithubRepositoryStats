using GithubRepositoryStats.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GithubRepositoryStats
{

    public partial class MyToolWindowControl : UserControl
    {
        public MyToolWindowControl()
        {
            InitializeComponent();
            GithubHelper.StaticDataChanged += StaticDataChanged;
            repoData.Visibility = Visibility.Hidden;
        }

        private void StaticDataChanged(object sender, PropertyChangedEventArgs e)
        {
            Sync();
        }

        private async void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            await GithubHelper.RefreshInfoAsync();
        }

        void Sync()
        {
            if (repoData != null && GithubHelper.Data.Repository != null)
            {
                textMessage.Text = GithubHelper.Data.Message;
                if (GithubHelper.Data.IsSuccess)
                {
                    repoData.Visibility = Visibility.Visible;
                    SetData(repoOwner, GithubHelper.Data.Repository.owner.login, GithubHelper.Data.Repository?.owner?.html_url);
                    SetData(repoName, GithubHelper.Data.Repository.name, GithubHelper.Data.Repository?.html_url);
                    SetData(repoWatching, GithubHelper.Data.Repository.subscribers_count.ToString(), GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/watchers"));
                    SetData(repoStars, GithubHelper.Data.Repository.stargazers_count.ToString(), GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/stargazers"));
                    SetData(repoForks, GithubHelper.Data.Repository.forks_count.ToString(), GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/forks"));
                    SetData(repoVisibility, GithubHelper.Data.Repository.visibility);
                    SetData(repoLanguage, GithubHelper.Data.Repository.language);
                    SetData(repoLicense, GithubHelper.Data.Repository.license?.name ?? "None");
                }
                else
                {
                    repoData.Visibility = Visibility.Hidden;
                }
            }

        }
        void SetData(Span element, string text, string url = null)
        {
            if (url is not null && element is Hyperlink hyperlink)
                hyperlink.NavigateUri = new Uri(url);

            element.Inlines.Clear();
            element.Inlines.Add(new Run(text));

            ToolTip toolTip = new ToolTip();
            toolTip.Content = url ?? text;
            element.ToolTip = toolTip;
        }


        private void baseRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Helper.OpenIfNotNull(sender, e);
        }

        private async void repoLanguage_Click(object sender, RoutedEventArgs e)
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
                        SetData(repoLanguage, string.Join(" , ", model.Select(p => p.Key)));
                    }
                }
            }
            catch
            {
            }
        }
    }
}
