using System.Diagnostics;

namespace autoPublish.Core.Utility
{
    public static class CMDHelper
    {
        public static string RunCmd(string path, string command)
        {
            Process pro = new Process();
            //Pro = pro;

            pro.StartInfo.FileName = "cmd.exe";

            pro.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            pro.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            pro.StartInfo.RedirectStandardInput = true;  // 重定向输入    
            pro.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
            pro.StartInfo.RedirectStandardError = false; //重定向标准错误
                                                         // 重定向错误输出  
            pro.StartInfo.WorkingDirectory = path;  //定义执行的路径 


            pro.Start();



            pro.StandardInput.WriteLine(command); //向cmd中输入命令

            pro.StandardInput.AutoFlush = true;
            pro.StandardInput.WriteLine("exit");  //退出

            string outRead = pro.StandardOutput.ReadToEnd();  //获得所有标准输出流

            pro.WaitForExit(); //等待命令执行完成后退出

            pro.Close(); //关闭窗口
            return outRead;
        }

        public static string GitPull(string path)
        {
            return RunCmd(path, "git pull");
        }

        public static string BuildProject(string path,string cmd = "dotnet publish -c release -o publish")
        {
            return RunCmd(path, cmd);
        }
    }
}
