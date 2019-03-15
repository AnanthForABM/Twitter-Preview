using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LinkedInConnect.Models;
using DalSoft.RestClient;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace LinkedInConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConnectionStrings _connectionString;

         public HomeController(IOptions<ConnectionStrings> connectionString)
        {
            _connectionString = connectionString.Value;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> SetupLinkedInAsync()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id=8190mlhxycvws0&redirect_uri=https%3A%2F%2Flocalhost%3A44351%2Fhome%2FValidateOAuth&state=DCEeFWf45A53sdfKef424&scope=r_liteprofile%20r_emailaddress%20w_member_social");
            

            return Redirect(response.RequestMessage.RequestUri.AbsoluteUri);
        }


        public IActionResult GetMetaData(string url)
        {
            string responseBody;
            var request = (HttpWebRequest)WebRequest.Create(url);

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var encoding = Encoding.GetEncoding(response.CharacterSet);

                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream, encoding))
                    responseBody = reader.ReadToEnd();
            }

            Regex metaregex = new Regex(@"<meta property=""og.+>", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            Dictionary<string, string> metaAttributeList = new Dictionary<string, string>();
            foreach (Match metamatch in metaregex.Matches(responseBody))
            {
                string name = string.Empty;
                string value = string.Empty;

                Regex submetaregex =
                        new Regex(@"(?<name>\b(\w|-)+\b)\" +
                                  @"s*=\s*(""(?<value>" +
                                  @"[^""]*)""|'(?<value>[^']*)'" +
                                  @"|(?<value>[^""'<> ]+)\s*)+",
                                  RegexOptions.IgnoreCase |
                                  RegexOptions.ExplicitCapture);

                foreach (Match submetamatch in submetaregex.Matches(metamatch.Value.ToString()))
                {
                    if (("property" ==
                         submetamatch.Groups["name"].ToString().ToLower()))
                        name = submetamatch.Groups["value"].ToString();

                    if ("content" ==
                        submetamatch.Groups["name"].ToString().ToLower())
                    {
                        value = submetamatch.Groups["value"].ToString();
                        if(!metaAttributeList.ContainsKey(name))
                            metaAttributeList.Add(name, value);
                        else
                        {
                            metaAttributeList.Remove(name);
                            metaAttributeList.Add(name, value);
                        }
                    }
                }
            }
            return Json(metaAttributeList);
        }

        public IActionResult Preview()
        {
            var AccessToken = LinkedInToken.GetAcessToken(_connectionString.DefaultConnection);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            //var data = client.GetAsync("https://api.linkedin.com/v1/people/~?format=json&projection=(id,firstName)").Result.Content.ReadAsStringAsync().Result;
            var data = client.GetAsync("https://api.linkedin.com/v2/me?projection=(id,firstName,lastName,profilePicture(displayImage~:playableStreams))").Result.Content.ReadAsStringAsync().Result;
            var ProfileData = JsonConvert.DeserializeObject<LinkedInPreviewViewModel>(data);
            return View(ProfileData);
        }

        public async Task<IActionResult> PostShare(string url, string text)
        {
            try
            {
                var AccessToken = LinkedInToken.GetAcessToken(_connectionString.DefaultConnection);
                var json = @"{
                ""author"": ""urn:li:person:G--ZxpMZ0Z"",
                ""lifecycleState"": ""PUBLISHED"",
                ""specificContent"": {
                            ""com.linkedin.ugc.ShareContent"": {
                                ""shareCommentary"": {
                                    ""text"": ""{0}""
                                },
                        ""shareMediaCategory"": ""ARTICLE"",
                        ""media"": [
                            {
                                ""status"": ""READY"",
                                ""originalUrl"": ""{1}""
                            }
                        ]
                    
                     }
                  },
                ""visibility"": {
                            ""com.linkedin.ugc.MemberNetworkVisibility"": ""PUBLIC""
                }
            }";
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, "https://api.linkedin.com/v2/ugcPosts"))
                {
                    //var content = JsonConvert.SerializeObject((json.Replace("{0}",text)).Replace("{1}",url));
                    using (var stringContent = new StringContent(((json.Replace("{0}", text)).Replace("{1}", url)), Encoding.UTF8, "application/json"))
                    {
                        request.Content = stringContent;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                        using (var response = await client
                            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                            .ConfigureAwait(false))
                        {
                            response.EnsureSuccessStatusCode();
                            return Json(response.Content);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                return Content("rror");
            }
            
        }
        
        public IActionResult ValidateOAuth()
        {
            var code = Request.Query["code"].ToString();

            HttpClient client = new HttpClient();
            var response = client.GetAsync("https://www.linkedin.com/oauth/v2/accessToken?grant_type=authorization_code&code=" + code + "&redirect_uri=https%3A%2F%2Flocalhost%3A44351%2Fhome%2FValidateOAuth&client_id=8190mlhxycvws0&client_secret=OTmp9DfVo80qKRjB").Result.Content.ReadAsStringAsync().Result;


            var Token = JsonConvert.DeserializeObject<LinkedInToken>(response);
            Token.InsertToken(_connectionString.DefaultConnection);

            return RedirectToAction("Preview","Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
