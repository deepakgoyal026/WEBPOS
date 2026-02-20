using EazyPOS.BLL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EazyPOS.Common
{
    
    public class APIKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration config;
        private APIResponse ObjResp;
        private string Message = "";
        EncryptDecrypt_v1 _EncryptDecrypt_v1;
        LoggerService _LoggerService;
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment _environment;

        public APIKeyMiddleware(RequestDelegate next, IConfiguration _config)
        {
            _next = next;
            config = _config;
            _EncryptDecrypt_v1 = new EncryptDecrypt_v1();
            ObjResp = new APIResponse(config); 
            _LoggerService = new LoggerService(config, _environment);
        }

        public async Task Invoke(HttpContext httpCont)
        {
            string APIName = "";
            try
            {
                APIName = httpCont.Request.Path;

                if ((APIName.ToLower().Contains("api/login") || APIName.ToLower().Contains("api/home") || APIName.ToLower().Contains(".xlsx") || APIName.ToLower().Contains(".xls") || APIName.ToLower().Contains("api/resetpassword") || APIName.ToLower().Contains("api/otp_verification") || APIName.ToLower().Contains("api/get_image")))
                {
                    await _next(httpCont);
                }
                else
                {
                    Int32 User_id = 0;
                    Microsoft.Extensions.Primitives.StringValues HeaderVal;

                    if (httpCont.Request.Headers.TryGetValue("Token", out HeaderVal))
                    {
                        //Getting origin from header
                        //string origin = httpCont.Request.Headers["Origin"].FirstOrDefault();
                        //var origin1 = origin.Split(new[] { "://" }, StringSplitOptions.None);
                        //string protocol = origin1[0];
                        //string domain = origin1[1];

                        //_LoggerService.WriteLogs("domain :- " + domain);

                        //Getting token from header
                        string token = HeaderVal.FirstOrDefault();
                        _LoggerService.WriteLogs("HeaderVal token :- " + token);

                        //Decrypt the token
                        var test = _EncryptDecrypt_v1.DecryptStringFromBytes_Aes("TRP0vC7oNbW61j8m42hDVNmsayVeLkT+EIghwNvPu5A8+aFBhNxLyf3BbKm7tIknsfmOVL0UCJVuvXyW2z7okdFwHKrfQogHGK0DycnxnqInxIDnI36riZGRHjzL69aP");
                        var DecryptKey = _EncryptDecrypt_v1.DecryptStringFromBytes_Aes(token);

                        //Getting user id from header
                        if (httpCont.Request.Headers.TryGetValue("UserId", out HeaderVal) || httpCont.Request.Headers.TryGetValue("variuserid", out HeaderVal))
                        {
                            User_id = Convert.ToInt32(HeaderVal.FirstOrDefault());
                        }

                        //Getting value from token and insert into the var for builed the connection
                        var components = DecryptKey.Split(';');
                        if (components.Length > 6)
                        {
                            UserInfo._DatabasServer = components[0];
                            UserInfo._DatabasePort = components[1];
                            UserInfo._DatabaseName = components[2];
                            UserInfo._Origin = components[3];
                            UserInfo._UserID = components[4];
                            UserInfo._SchemaName = components[5];
                        }

                        ////Skip the cookies feature for "api/getuserdetail" abd Compare with header and cookies token
                        ////if (!APIName.ToLower().Contains("api/getuserdetail"))
                        ////{
                        ////    if (token.ToLower() != cookiesToken.ToLower())
                        ////    {
                        ////        await httpCont.Response.WriteAsJsonAsync(ObjResp.ResponseFormat(null, false, "You are not vailide user!", 401));
                        ////        return;
                        ////    }
                        ////}

                        ////Compare with domain
                        ////if (domain.ToLower() != components[3].ToString().ToLower())
                        ////{
                        ////    await httpCont.Response.WriteAsJsonAsync(ObjResp.ResponseFormat(null, false, "You are not authorized for this URL!", 401));
                        ////}

                        ////Compare with user id
                        //if (Convert.ToInt32(User_id) != Convert.ToInt32(components[4]))
                        //{
                        //    await httpCont.Response.WriteAsJsonAsync(ObjResp.ResponseFormat(null, false, "You are not vailide user!", 401));
                        //    return;
                        //}

                        Authenticate validateUserobj = new Authenticate(config);
                        if (validateUserobj.IsValidToken(token, ref Message) == true)
                        {
                            //if (httpCont.Request.Headers.TryGetValue("BranchId", out HeaderVal))
                            //{
                            //    APIResponse.BranchId = Convert.ToInt32(HeaderVal.FirstOrDefault());
                            //}
                            //if (httpCont.Request.Headers.TryGetValue("FormId", out HeaderVal))
                            //{
                            //    APIResponse.FormId = Convert.ToInt32(HeaderVal.FirstOrDefault());
                            //}
                            //APIResponse.UserId = User_id;

                            //Regenerate the token
                            //string newToken = validateUserobj.BuildToken(User_id, domain);
                            APIResponse.AuthTokenKey = token;

                            //Token pass in the cookies
                            httpCont.Response.Cookies.Append("POS", token, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = httpCont.Request.IsHttps,
                                //SameSite = SameSiteMode.Strict,
                                SameSite = SameSiteMode.None,
                                // Expires = DateTimeOffset.UtcNow.AddHours(1)
                            });
                            //----------------

                            await _next(httpCont);
                        }
                        else
                        {
                            await httpCont.Response.WriteAsJsonAsync(ObjResp.ResponseFormat(null, false, Message, 401));
                            return;
                        }
                    }
                    else
                    {
                        await httpCont.Response.WriteAsJsonAsync(ObjResp.ResponseFormat(null, false, "Token not passed in the header!", 401));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _LoggerService.WriteLogs("Meddileware catch block api name :- " + APIName);
                _LoggerService.WriteLogs("Meddileware catch block error :- " + ex.Message);
                await httpCont.Response.WriteAsJsonAsync(ObjResp.ResponseFormat(ex, false, ex.Message, 400));
            }
            finally
            {
                //ObjResp.PGConnection_Close();
                _LoggerService.WriteLogs("Fially block api name :- " + APIName);
            }
        }

    }
}
