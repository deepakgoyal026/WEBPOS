using EazyPOS.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;



namespace EazyPOS.Controllers
{
    
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger _Logger;
        private readonly IConfiguration _Config;
        APIResponse ObjB;
        private readonly IWebHostEnvironment _env;
        private HttpContext objhttpContext;

        public HomeController(IConfiguration configuration, ILogger<HomeController> Logger, IWebHostEnvironment env)
        {
            _Config = configuration;
            ObjB = new APIResponse(_Config);
            _Logger = Logger;
            //_Logger.LogInformation("Log message in the About() method");
            _env = env;
            
        }


        [HttpGet]
        [Route("api/home")]
        public JsonResult GetHome()
        {
            try
            {
                var Result = ObjB.PgScaler("SELECT '['||ROW_TO_JSON((SELECT Cols FROM (SELECT emailid,URL,contactnumber1,FALSE IsShowCaptcha)Cols(\"ContactEmail\",\"URL\",\"ContactNumber\",\"IsShowCaptcha\")))||']' FROM comn_companydetail ; ");
                JArray ResJson = JArray.Parse(Result.ToString());
                if (ResJson != null)
                {
                    return (ObjB.ResponseFormat(ResJson, true, "Data found!"));
                }
                else
                {
                    return (ObjB.ResponseFormat(null, false, "No data found!"));
                }

            }
            catch (Exception ex)
            {
                return (ObjB.ResponseFormat(ex, false, ex.Message, 500));
            }
            finally
            {
                ObjB.PGConnection_Close();
            }

        }


       
       
    }
}
