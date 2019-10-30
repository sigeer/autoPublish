using autoPublish.Core.Models;
using autoPublish.Core.Runner;
using System;

namespace autoPublish.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = new RepositoryModel();
            model.OutputProjects = new System.Collections.Generic.List<ProjectModel>()
            {
                new ProjectModel()
                {
                    ProjectRootDir = @"C:\user\AutoPublisher\xyqBBLinkWeb\xyqBBLink.Web",
                    ProjectType = 1,
                    LiveRootDir = @"C:\user\publish\publish",
                    IgnoreFiles = new string[]{ "data.db" }
                }
            };
            foreach (var projectModel in model.OutputProjects)
            {
                var project = new Project(projectModel, new GitHubRepositryFactory());

                var publisher = new AutoPublisher(project);
                publisher.Core();
            }
            Console.ReadLine();
        }
    }
}
