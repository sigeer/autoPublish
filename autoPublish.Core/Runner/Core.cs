using autoPublish.Core.Models;
using autoPublish.Core.Utility;
using System;
using System.IO;
using System.Threading.Tasks;

namespace autoPublish.Core.Runner
{
    public class AutoPublisher
    {
        //1.拉取文件
        //2.编译文件 dot publish
        //3.覆盖目标站点
        private Project _project;


        public AutoPublisher(Project project)
        {
            _project = project;
        }

        public async Task Core()
        {
            await Task.Run(() =>
            {
                Pull();
                BuildProject();
                Publish();
            });
        }

        private void Pull()
        {
            Console.WriteLine("Start Pull...");
            var cmdResult = _project.Pull();
            Console.WriteLine(cmdResult);
        }

        private void BuildProject()
        {
            Console.WriteLine("Start Build...");
            var cmdResult = _project.Build();
            Console.WriteLine(cmdResult);
        }

        private void Publish()
        {
            Console.WriteLine("Start Publish...");
            _project.Publish();
        }
    }

    public class Project
    {
        private GitRepository _gitRepository;
        private ProjectBuilder _projectBuilder;

        public string ProjectRootDir { get; set; }
        public string ProjectPublishedDir { get; set; }
        public string LiveRootDir { get; set; }

        public ProjectType Type { get; set; }

        public Project(ProjectModel projectModel, GitRepositoryFactory factory)
        {
            ProjectRootDir = projectModel.ProjectRootDir;
            Type = (ProjectType)projectModel.ProjectType;
            LiveRootDir = projectModel.LiveRootDir;

            ProjectPublishedDir = Path.Combine(ProjectRootDir, "publish");
            _gitRepository = factory.GetRepository(ProjectRootDir);
            _projectBuilder = ProjectBuilderFactory.GetBuilder(Type);
        }

        public string Pull()
        {
            return _gitRepository.Pull();
        }

        public async Task<string> PullAsync()
        {
            return await _gitRepository.PullAsync();
        }

        public string Build()
        {
            return _projectBuilder.BuildProject(ProjectRootDir);
        }

        public async Task<string> BuildAsync()
        {
            return await _projectBuilder.BuildProjectAsync(ProjectRootDir);
        }

        public void Publish()
        {
            //
            WebSiteUpdate webSiteUpdate = new WebSiteUpdate(LiveRootDir, "", "", ProjectPublishedDir);
            webSiteUpdate.Core();
        }

    }

    public enum ProjectType
    {
        DotNetCore = 1
    }


    public abstract class ProjectBuilder
    {
        public abstract string BuildProject(string projectDir);
        public abstract Task<string> BuildProjectAsync(string projectDir);
    }

    public class DotNetCoreBuilder : ProjectBuilder
    {
        public override string BuildProject(string projectDir)
        {
            Console.WriteLine(projectDir);
            return CMDHelper.BuildProject(projectDir);
        }

        public override async Task<string> BuildProjectAsync(string projectDir)
        {
            Console.WriteLine(projectDir);
            return await Task.Run(() => CMDHelper.BuildProject(projectDir));
        }
    }

    public class ProjectBuilderFactory
    {
        public static ProjectBuilder GetBuilder(ProjectType type)
        {
            switch (type)
            {
                case ProjectType.DotNetCore:
                    return new DotNetCoreBuilder();
                default:
                    break;
            }
            return null;
        }
    }
}
