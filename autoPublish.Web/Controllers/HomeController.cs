using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using autoPublish.Core.Models;
using autoPublish.Core.Runner;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;

namespace autoPublish.Web.Controllers
{
    [EnableCors("any")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private IConfiguration _configuration;
        private Logger _logger => LogManager.GetCurrentClassLogger();
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _logger.Info("构造函数");
        }

        private async Task<string> GetRequestBodyUTF8String()
        {
            try
            {
                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8))
                {
                    var body = await reader.ReadToEndAsync();
                    // Do some processing with body…
                    // Reset the request body stream position so the next middleware can read it
                    Request.Body.Position = 0;
                    return body;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return string.Empty;
            }
        }

        //sha1加密（标准）
        private string HMAC1(string publickey, string str)
        {
            byte[] byte1 = System.Text.Encoding.UTF8.GetBytes(publickey);
            byte[] byte2 = System.Text.Encoding.UTF8.GetBytes(str);
            HMACSHA1 hmac = new HMACSHA1(byte1);

            //把比特流连接起来，附加到字符串里面。

            //比特流转化字符串万能linq：.Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s)
            string hashValue = hmac.ComputeHash(byte2).Aggregate("", (s, e) => s + string.Format("{0:x2}", e), s => s);
            return hashValue;
        }

        private bool isNeededBranch(string refStr, string branch = "master")
        {
            return refStr.Contains("\"ref\":\"refs/heads/master\"");
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Input()
        {
            _logger.Info("action");
            var signature = Request.Headers["X-Hub-Signature"];
            var gitIssuer = Request.Headers["X-GitHub-Event"];

            var body = await GetRequestBodyUTF8String();

            _logger.Info("X-Hub-Signature: " + signature);
            _logger.Info("Body: " + body);
            _logger.Info("X-GitHub-Event: " + gitIssuer);

            var isMaster = isNeededBranch(HttpUtility.UrlDecode(body));
            if (!isMaster)
            {
                _logger.Info("不是master");
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            var list = _configuration.GetSection("Repositories").Get<List<RepositoryModel>>();

            foreach (var model in list)
            {
                HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(model.Secret));
                byte[] rstRes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(body));
                string localSignature = "sha1=" + HMAC1(model.Secret, body);
                _logger.Info(localSignature);

                if (signature == localSignature)
                {
                    foreach (var projectModel in model.OutputProjects)
                    {
                        var project = new Project(projectModel, new GitHubRepositryFactory());

                        var publisher = new AutoPublisher(project);
                        await publisher.Core();
                    }
                }
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Index(string data)
        {
            if (data != "123456")
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            var allowed = _configuration.GetSection("ManualUrl").Value;
            if (allowed == "*" || allowed.Split(",").Contains(ip))
            {
                _logger.Info("---手动更新---");
                var list = _configuration.GetSection("Repositories").Get<List<RepositoryModel>>();
                foreach (var model in list)
                {
                    foreach (var projectModel in model.OutputProjects)
                    {
                        _logger.Info(projectModel.LiveRootDir);
                        var project = new Project(projectModel, new GitHubRepositryFactory());

                        var publisher = new AutoPublisher(project);
                        await publisher.Core();
                    }
                }
                _logger.Info("===手动更新===");
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
