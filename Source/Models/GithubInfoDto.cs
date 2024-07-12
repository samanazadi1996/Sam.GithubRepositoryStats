namespace GithubRepositoryStats.Models
{
    public class GithubInfoDto
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        public Repository Repository { get; private set; }

        public void Fail(string msg)
        {
            Repository = null;
            Message = msg;
            IsSuccess = false;
        }
        public void Ok(Repository repo, string msg)
        {
            Repository = repo;
            Message = msg;
            IsSuccess = true;
        }
    }

    public class Owner
    {
        public string login { get; set; }
        public string html_url { get; set; }
    }

    public class License
    {
        public string name { get; set; }
    }

    public class Repository
    {
        public string name { get; set; }
        public Owner owner { get; set; }
        public string html_url { get; set; }
        public string languages_url { get; set; }
        public int stargazers_count { get; set; }
        public string language { get; set; }
        public int forks_count { get; set; }
        public int open_issues_count { get; set; }
        public License license { get; set; }
        public string visibility { get; set; }
        public int subscribers_count { get; set; }
    }
}