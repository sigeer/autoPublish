using autoPublish.Core.Runner;
using autoPublish.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace autoPublish.Core.Models
{
    public enum Method
    {
        FromCoding = 1,
        FromCommand = 2
    }

    public class RepositoryModel
    {
        public int Method { get; set; }
        public string Secret { get; set; }

        public List<ProjectModel> OutputProjects { get; set; }
    }

    public class ProjectModel
    {
        public int ProjectType { get; set; }
        public string ProjectRootDir { get; set; }

        public string LiveRootDir { get; set; }

        public string[] IgnoreFiles { get; set; }
        /// <summary>
        /// e.g. [ "dotnet a.dll -urls "{{AAA}}" "]
        /// </summary>
        public string[] CommandList { get; set; }
        private static string Path { get; set; }
        public (string, bool) ExecuteMethodFromCommand(string cmd, Project pro)
        {
            var propertyReg = new Regex(@"\{\{(.+?)\}\}");
            if (propertyReg.IsMatch(cmd))
            {
                var ms = propertyReg.Matches(cmd);
                var ass = typeof(Project);
                foreach (Match m in ms)
                {
                    var property = ass.GetProperty(m.Groups[1].Value);
                    if (property == null)
                    {
                        continue;
                    }
                    var replaceValue = property.GetValue(pro).ToString();
                    cmd = cmd.Replace(m.Value, replaceValue);
                }
            }
            var methodReg = new Regex(@"\[\[(.+?)\]\]");
            if (methodReg.IsMatch(cmd))
            {
                var m = methodReg.Match(cmd);
                var ass = typeof(Project);
                var method = ass.GetMethod(m.Groups[1].Value);
                if (method != null)
                {
                    var result = method.Invoke(pro, null);
                    return (result?.ToString(), false);
                }

            }
            return (cmd, true);
        }

        public virtual string Execute(Project pro)
        {
            var sb = new StringBuilder();
            foreach (var cmd in CommandList)
            {
                var (realCMD, isCmd) = ExecuteMethodFromCommand(cmd, pro);
                sb.AppendLine(realCMD);
                if (isCmd)
                {
                    var isJump = realCMD.StartsWith("cd");
                    if (isJump)
                    {
                        Path = realCMD.Split(" ")[1];
                    }
                    sb.AppendLine(CMDHelper.RunCmd(Path, realCMD));
                }
            }
            return sb.ToString();
        }
    }
}
