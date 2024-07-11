using System.Diagnostics;
using System.Windows.Navigation;

namespace GithubRepositoryStats.Infrastructure
{
    public static class Helper
    {
        public static string AddIfNotNull(this string? url, string value)
        {
            if (url is null) return null;

            return url + value;
        }
        public static void OpenIfNotNull(object sender, RequestNavigateEventArgs e)
        {
                try
                {
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                }
                catch
                {
                }
        }

    }
}
