using autoPublish.Core.Models;
using autoPublish.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace autoPublish.Core.Runner
{
    public class Project
    {
        private ILogger<Project> _logger;
        private GitRepository _gitRepository;
        private ProjectBuilder _projectBuilder;
        private ProjectModel _projectModel;
        public string ProjectRootDir { get; set; }
        public string ProjectPublishedDir { get; set; }
        public string LiveRootDir { get; set; }

        public ProjectType Type { get; set; }

        public Project(ILogger<Project> logger)
        {
            _logger = logger;

        }

        public void Init(ProjectModel projectModel, GitRepositoryFactory factory)
        {
            _projectModel = projectModel;
            ProjectRootDir = _projectModel.ProjectRootDir;
            Type = (ProjectType)_projectModel.ProjectType;
            LiveRootDir = _projectModel.LiveRootDir;

            ProjectPublishedDir = Path.Combine(ProjectRootDir, "publish");
            _gitRepository = factory.GetRepository(ProjectRootDir);
            _projectBuilder = ProjectBuilderFactory.GetBuilder(Type);
            _logger.LogInformation($"LiveRootDir = {LiveRootDir}, ProjectRootDir = {ProjectRootDir}, ProjectPublishedDir = {ProjectPublishedDir}");
        }

        public async Task<string> Core()
        {
            return await Task.Run(() => _projectModel.Execute(this));
        }

        public async Task CoreOld()
        {
            await Task.Run(() =>
            {
                Pull();
                Build();
                Publish();
            });
        }

        public string Pull()
        {
            _logger.LogInformation("---Pull---");
            return _gitRepository.Pull();
        }

        public async Task<string> PullAsync()
        {
            _logger.LogInformation("---PullAsync---");
            return await _gitRepository.PullAsync();
        }

        public string Build()
        {
            _logger.LogInformation("---Build---");
            return _projectBuilder.BuildProject(ProjectRootDir);
        }

        public async Task<string> BuildAsync()
        {
            _logger.LogInformation("---BuildAsync---");
            return await _projectBuilder.BuildProjectAsync(ProjectRootDir);
        }

        public string Publish()
        {
            _logger.LogInformation("---Publish---");
            WebSiteUpdate webSiteUpdate = new WebSiteUpdate(LiveRootDir, "", "", ProjectPublishedDir);
            webSiteUpdate.Core();
            return "Publish finished";
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
