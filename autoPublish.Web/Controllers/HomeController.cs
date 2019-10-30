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
            _logger.Info("1111");
        }

        private string GetRequestBodyUTF8String()
        {
            this.Request.EnableBuffering();
            this.Request.Body.Position = 0;
            Encoding encoding = System.Text.UTF8Encoding.Default;
            if (this.Request.ContentLength > 0 && this.Request.Body != null && this.Request.Body.CanRead)
            {
                using (var buffer = new MemoryStream())
                {
                    this.Request.Body.CopyTo(buffer);
                    buffer.Position = 0;
                    var reader = new StreamReader(buffer, encoding);
                    var body = reader.ReadToEnd();
                    return body;
                }
            }

            return string.Empty;
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
            var signature = Request.Headers["X-Hub-Signature"];
            var gitIssuer = Request.Headers["X-GitHub-Event"];

            var body = GetRequestBodyUTF8String();

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
    }
}
