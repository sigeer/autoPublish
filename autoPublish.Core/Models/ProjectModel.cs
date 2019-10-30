using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace autoPublish.Core.Models
{
    public class ProjectModel
    {

        public int ProjectType { get; set; }
        public string ProjectRootDir { get; set; }

        public string LiveRootDir { get; set; }

        public string[] IgnoreFiles { get; set; }
    }

    public class RepositoryModel
    {
        public string Secret { get; set; }

        public List<ProjectModel> OutputProjects { get; set; }
    }
}
