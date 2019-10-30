using autoPublish.Core.Utility;
using System;
using System.Threading.Tasks;

namespace autoPublish.Core.Models
{
    public class GitRepository : IGitRepository
    {
        public string sercret;
        public string user;
        public string password;
        public string originalUrl;
        public string RepositoryDir;

        public GitRepository(string repositoryDir)
        {
            RepositoryDir = repositoryDir;
        }

        public virtual string Pull()
        {
            Console.WriteLine(RepositoryDir);
            return CMDHelper.GitPull(RepositoryDir);
        }

        public virtual async Task<string> PullAsync()
        {
            Console.WriteLine(RepositoryDir);
            return await Task.Run(()=> CMDHelper.GitPull(RepositoryDir));
        }
    }

    public class GitHubRepository : GitRepository
    {
        public GitHubRepository(string repositoryDir) : base(repositoryDir)
        {

        }
    }
}
