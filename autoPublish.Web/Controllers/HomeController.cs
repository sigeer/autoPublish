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
using Microsoft.Extensions.Logging;
using Sigeer.Tool.Extension;

namespace autoPublish.Web.Controllers
{
    [EnableCors("any")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private ILogger<HomeController> _logger;
        private Project _project;
        private List<RepositoryModel> _repositoryList;

        public HomeController(ILogger<HomeController> logger, Project project, List<RepositoryModel> repositoryList)
        {
            _logger = logger;
            _project = project;
            _repositoryList = repositoryList;
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
                _logger.LogError(ex.Message);
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
            _logger.LogInformation("action");
            var signature = Request.Headers["X-Hub-Signature"];
            var gitIssuer = Request.Headers["X-GitHub-Event"];

            var body = await GetRequestBodyUTF8String();

            _logger.LogInformation("X-Hub-Signature: " + signature);
            _logger.LogInformation("Body: " + body);
            _logger.LogInformation("X-GitHub-Event: " + gitIssuer);

            var isMaster = isNeededBranch(HttpUtility.UrlDecode(body));
            if (!isMaster)
            {
                _logger.LogInformation("不是master");
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            foreach (var model in _repositoryList)
            {
                HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(model.Secret));
                byte[] rstRes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(body));
                string localSignature = "sha1=" + HMAC1(model.Secret, body);
                _logger.LogInformation(localSignature);

                if (signature == localSignature)
                {
                    foreach (var projectModel in model.OutputProjects)
                    {
                        _project.Init(projectModel, new GitHubRepositryFactory());

                        var str = await _project.Core();
                        _logger.LogInformation(str);
                    }
                }
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Index(string data)
        {
            _logger.LogInformation("request code :" + data);
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            _logger.LogInformation("remote ip : " + ip);
            foreach (var model in _repositoryList)
            {
                if (data != model.Secret)
                {
                    continue;
                }
                foreach (var projectModel in model.OutputProjects)
                {
                    _logger.LogInformation(projectModel.LiveRootDir);
                    _project.Init(projectModel, new GitHubRepositryFactory());

                    if (model.Method == Method.FromCommand.ToInt())
                    {
                        var str = await _project.Core();
                        _logger.LogInformation(str);
                    }
                    else
                    {
                        await _project.CoreOld();
                    }
                }
            }
            _logger.LogInformation("===手动更新===");
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
