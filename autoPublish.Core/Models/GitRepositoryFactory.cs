using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace autoPublish.Core.Models
{
    public abstract class GitRepositoryFactory
    {
        public abstract GitRepository GetRepository(string repositoryDir);
    }

    public class GitHubRepositryFactory : GitRepositoryFactory
    {
        public override GitRepository GetRepository(string repositoryDir)
        {
            return new GitHubRepository(repositoryDir);
        }
    }
}
