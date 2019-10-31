using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace autoPublish.Core.Runner
{
    /// <summary>
    /// 1.输入站点路径
    /// 2.输入app_offline.htm文件路径 为空时使用工具自带的默认模板
    /// 3.输入更新的内容 即app_offline.htm存在替换域  没有替换域时 此输入无效
    /// 5.输入需要更换的文件
    /// 6.替换---- 在目的路径生成app_offline.htm，替换文件，删除app_offline.htm。
    /// 7.结束
    /// </summary>
    public class WebSiteUpdate
    {
        /// <summary>
        /// 不可为空
        /// </summary>
        private string WebsiteRootPath { get; set; }
        /// <summary>
        /// 可空
        /// </summary>
        private string AppOfflineHtmlPath { get; set; }
        /// <summary>
        /// 可空
        /// </summary>
        private string UpdateContent { get; set; }

        /// <summary>
        /// 不可为空 1.单个文件，2.压缩包，3.目录
        /// </summary>
        private string UpdateFiles { get; set; }

        private string appOfflineHtmlFileNameLocal = "app_offline.txt";
        private string appOfflineHtmlFileNameLive = "app_offline.htm";

        public string[] IgnoreFiles { get; set; }

        public WebSiteUpdate(string websiteRootPath, string appOfflineHtmlPath, string updateContent, string fileOrDir, string [] ignoreFiles = null)
        {
            WebsiteRootPath = websiteRootPath;
            AppOfflineHtmlPath = appOfflineHtmlPath;
            UpdateContent = updateContent;
            UpdateFiles = fileOrDir;

            IgnoreFiles = ignoreFiles;
        }

        public void NewUpdateConfig(string appOfflineHtmlPath, string updateContent, string files)
        {
            AppOfflineHtmlPath = appOfflineHtmlPath;
            UpdateContent = updateContent;
            UpdateFiles = files;
        }

        public void Core()
        {
            GenerateOfflineHtml();
            ReplaceFiles();
            RemoveOfflineHtml();
        }

        private bool ReplaceFiles()
        {
            if (File.Exists(UpdateFiles))
            {
                var fileType = Path.GetExtension(UpdateFiles).ToUpper();
                FileInfo fileInfo = new FileInfo(UpdateFiles);

                if (fileType == ".ZIP")
                {
                    //ZipHelper.UnZip(UpdateFiles, WebsiteRootPath);
                    //Console.WriteLine($"unzip from {UpdateFiles} to {WebsiteRootPath}");
                }
                else
                {
                    var targetPath = Path.Combine(WebsiteRootPath, fileInfo.Name);
                    File.Copy(UpdateFiles, targetPath, true);
                    Console.WriteLine($"copy from {UpdateFiles} to {targetPath}");
                }

                return true;
            }

            else if (Directory.Exists(UpdateFiles))
            {
                var str = FileWorker.CopyFiles(UpdateFiles, WebsiteRootPath, true, false, IgnoreFiles);
                Console.WriteLine(str);
                return true;
            }

            return false;
        }

        private void GenerateOfflineHtml()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;

            var filePath = Path.Combine(dir, appOfflineHtmlFileNameLocal);

            var targetPath = Path.Combine(WebsiteRootPath, appOfflineHtmlFileNameLive);

            var htmlContent = File.Exists(filePath)?File.ReadAllText(filePath, System.Text.Encoding.UTF8):"";
            Console.WriteLine($"read from {filePath}");

            htmlContent = htmlContent.Replace("{CONTENT}", UpdateContent);
            File.WriteAllText(targetPath, htmlContent, System.Text.Encoding.UTF8);
            Console.WriteLine($"write into {targetPath}");
            Console.WriteLine($"wait 5 second");
            Thread.Sleep(5000);
        }

        private void RemoveOfflineHtml()
        {
            var targetPath = Path.Combine(WebsiteRootPath, appOfflineHtmlFileNameLive);
            File.Delete(targetPath);
        }
    }

    public class FileWorker
    {
        public static string CopyFiles(string varFromDirectory, string varToDirectory, bool overWrite = false, bool strict = false, string[] blacklist = null, string preFix = "file:\\\\")
        {
            try
            {
                if (!Directory.Exists(varToDirectory))
                {
                    if (strict)
                    {
                        return $"{preFix}{varToDirectory} not exists";
                    }
                    else
                    {
                        Directory.CreateDirectory(varToDirectory);
                    }
                }

                if (!Directory.Exists(varFromDirectory))
                {
                    return $"{preFix}{varFromDirectory} not exists";
                }

                string[] directories = Directory.GetDirectories(varFromDirectory);
                if (directories.Length > 0)
                {
                    foreach (string d in directories)
                    {
                        var result = CopyFiles(d, varToDirectory + d.Substring(d.LastIndexOf("\\")), overWrite, strict, blacklist);
                        if (result != "Success")
                        {
                            return result;
                        }
                    }
                }
                string[] files = Directory.GetFiles(varFromDirectory);
                if (files.Length > 0)
                {
                    foreach (string s in files)
                    {
                        var fileName = Path.GetFileName(s);
                        if (blacklist != null && blacklist.Contains(fileName))
                        {
                            continue;
                        }
                        File.Copy(s, varToDirectory + s.Substring(s.LastIndexOf("\\")), overWrite);
                    }
                }
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }

}
