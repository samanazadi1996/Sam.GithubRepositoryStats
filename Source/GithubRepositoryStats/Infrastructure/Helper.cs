using System.Diagnostics;

namespace GithubRepositoryStats.Infrastructure
{
    public static class Helper
    {
        public static string AddIfNotNull(this string? url, string value)
        {
            if (url is null) return null;

            return url + value;
        }
        public static void OpenIfNotNull(string url)
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

    }
}
