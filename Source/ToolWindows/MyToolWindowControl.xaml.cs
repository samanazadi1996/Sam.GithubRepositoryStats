using GithubRepositoryStats.Infrastructure;
using System.ComponentModel;
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
            textMessage.Text = GithubHelper.Data?.Message;

            if (GithubHelper.Data.IsSuccess && GithubHelper.Data.Repository is not null)
            {
                repoData.Visibility = Visibility.Visible;
                SetData(repoOwner, GithubHelper.Data.Repository.owner.login, GithubHelper.Data.Repository?.owner?.html_url);
                SetData(repoName, GithubHelper.Data.Repository.name, GithubHelper.Data.Repository?.html_url);
                SetData(repoWatching, GithubHelper.Data.Repository.subscribers_count.ToString(), GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/watchers"));
                SetData(repoStars, GithubHelper.Data.Repository.stargazers_count.ToString(), GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/stargazers"));
                SetData(repoOpenIssues, GithubHelper.Data.Repository.open_issues_count.ToString(), GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/issues"));
                SetData(repoForks, GithubHelper.Data.Repository.forks_count.ToString(), GithubHelper.Data.Repository?.html_url?.AddIfNotNull("/forks"));
                SetData(repoVisibility, GithubHelper.Data.Repository.visibility);
                SetData(repoLanguage, GithubHelper.Data.Repository.language);
                SetData(repoLicense, GithubHelper.Data.Repository.license?.name ?? "None");
            }
            else
            {
                repoData.Visibility = Visibility.Hidden;
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
        }


        private void BaseRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Helper.OpenIfNotNull(sender, e);
        }

        private async void repoLanguage_Click(object sender, RoutedEventArgs e)
        {
            await GithubHelper.GetLanguagesAsync();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await GithubHelper.RefreshInfoAsync();
        }
    }
}
