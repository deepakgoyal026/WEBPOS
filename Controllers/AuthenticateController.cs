using EazyPOS.BLL;
using EazyPOS.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace EazyPOS.Controller
{
    public class AuthenticateController : ControllerBase
    {
        private readonly IConfiguration _Config;
        APIResponse ObjB;
        UserMaster um;
        private readonly Authenticate _Authenticate;

        public AuthenticateController(IConfiguration configuration)
        {
            _Config = configuration;
            ObjB = new APIResponse(_Config);
            um = new UserMaster();
            _Authenticate = new Authenticate(_Config);
        }


        [HttpPost]
        [Route("api/LogIn")]
        public async Task<JsonResult> GetLogIn([FromBody] LoginModel ObjLogin)
        {
            try
            {
                //Getting origin from header
                string origin = Request.Headers["Origin"].ToString();
                var domain = origin.Split(new[] { "://" }, StringSplitOptions.None);
                //string protocol = origin1[0];
                string url = domain[1];
                //--------------

                object result = await _Authenticate.GetClientDetails(url, ObjLogin.UserName, ObjLogin);
                var resultDict = result as Dictionary<string, object>;
                bool status = (bool)resultDict["Status"];

                if (status)
                {
                    //object userDetails = await _Authenticate.IsValidUser(ObjLogin.UserName, ObjLogin.Password);
                    object userDetails = await _Authenticate.IsValidUser(ObjLogin);
                    var _resultDict = userDetails as Dictionary<string, object>;
                    bool _status = (bool)_resultDict["Status"];

                    if (_status)
                    {
                        UserModel _userDetails = (UserModel)_resultDict["Result"];
                        var userId = _userDetails.UserId;

                        _Authenticate.BuildToken(userId, url);

                        //----------------------------
                        LoginResponseModel loginResponseModel = new LoginResponseModel();

                        loginResponseModel.ClientId = _Authenticate.GetClientList();
                        loginResponseModel.UserDet = _userDetails;

                        return (ObjB.ResponseFormat(loginResponseModel, true, "User logged in"));
                    }
                    else
                    {
                        string msg = (string)_resultDict["Result"];
                        return (ObjB.ResponseFormat(null, false, msg, 401));
                    }
                }
                else
                {
                    string message = (string)resultDict["Result"];
                    return (ObjB.ResponseFormat(null, false, message));
                }
            }
            catch (Exception ex)
            {
                return (ObjB.ResponseFormat(ex, false, ex.Message));
            }
            finally
            {
                ObjB.PGConnection_Close();
            }
        }


        [HttpGet]
        [Route("api/getuserdetail")]
        public async Task<IActionResult> GetUseerDetail()
        {
            try
            {
                int userId = int.Parse(Request.Headers["UserId"]);

                object result = await _Authenticate.GetUserDetail(userId);
                var resultDict = result as Dictionary<string, object>;
                bool status = (bool)resultDict["Status"];

                if (status)
                {
                    UserModel userModel = (UserModel)resultDict["Result"];

                    LoginResponseModel loginResponseModel = new LoginResponseModel();

                    loginResponseModel.ClientId = _Authenticate.GetClientList();
                    loginResponseModel.UserDet = userModel;

                    return ObjB.ResponseFormat(loginResponseModel, true, "Success!");
                }
                else
                {
                    string msg = (string)resultDict["Result"];
                    return ObjB.ResponseFormat(null, false, msg);
                }
            }
            catch (Exception)
            {
                return ObjB.ResponseFormat(null, false, "Something Wrong! Please try again.");
            }
            finally
            {
                ObjB.PGConnection_Close();
            }
        }


        [HttpPost]
        [Route("api/webhook")]
        public async Task<IActionResult> ReceiveMessage([FromForm] WhatsAppRequest request)
        {
            string incomingMsg = request.Body;
            string fromNumber = request.From;

            // Example message: SALES DLR001
            string dealerCode = ExtractDealerCode(incomingMsg);

            decimal todaySales = await GetTodaySales(dealerCode);

            await SendWhatsAppReply(fromNumber, dealerCode, todaySales);

            return Ok();
        }

        string ExtractDealerCode(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var parts = message.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // SALES DLR001
            if (parts.Length >= 2)
                return parts[1].ToUpper();

            return null;
        }
        public class WhatsAppRequest
        {
            public string From { get; set; }
            public string Body { get; set; }
        }
        async Task<decimal> GetTodaySales(string dealerCode)
        {
            //        using var conn = new NpgsqlConnection(_connectionString);
            //        await conn.OpenAsync();

            //        string sql = @"
            //SELECT COALESCE(SUM(invoice_amount),0)
            //FROM sales_transactions
            //WHERE dealer_code = @dealerCode
            //AND invoice_date::date = CURRENT_DATE";

            //        using var cmd = new NpgsqlCommand(sql, conn);
            //        cmd.Parameters.AddWithValue("@dealerCode", dealerCode);

            return Convert.ToDecimal("200");
        }

        async Task SendWhatsAppReply(string toNumber, string dealerCode, decimal todaySales)
        {
            // Read credentials from configuration / environment variables
            var accountSid = _Config["Twilio:AccountSid"];
            var authToken = _Config["Twilio:AuthToken"];
            var fromNumber = _Config["Twilio:FromNumber"]; // e.g. "whatsapp:+1415XXXXXXX"

            if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(fromNumber))
                throw new InvalidOperationException("Twilio credentials not configured.");

            TwilioClient.Init(accountSid, authToken);

            var body = $"Dealer {dealerCode} - Today's sales: {todaySales:C}";
            await MessageResource.CreateAsync(
                body: body,
                from: new PhoneNumber($"whatsapp:{fromNumber}"),
                to: new PhoneNumber($"whatsapp:{toNumber}")
            );
        }


    }
}
