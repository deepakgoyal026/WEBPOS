using EazyPOS.BLL;
using EazyPOS.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace EazyPOS.Common
{

    public class APIResponse : Authenticate
    {
        public APIResponse(IConfiguration _Config) : base(_Config)
        {
        }

        public JsonResult ResponseFormat(object _Result, bool _IsScuess, string _Msg, int _StatusCode = 0)
        {
            try
            {
                if (_StatusCode == 0)
                {
                    if (_IsScuess == true)
                    { _StatusCode = 200; }
                    else if (_IsScuess == false)
                    { _StatusCode = 404; }
                }
                var Response = new ResponseResult()
                {
                    Status = _IsScuess,
                    StatusCode = _StatusCode,
                    Message = _Msg,
                    Result = _Result,
                    Token = AuthTokenKey
                };
                return new JsonResult(Response);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string GetWebURL(HttpContext httpCont)
        {
            string WebURL = httpCont.Request.Host.Value.ToString();
            return WebURL;

        }
    }
}
