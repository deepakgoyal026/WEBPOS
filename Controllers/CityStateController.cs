using EazyPOS.BLL;
using EazyPOS.Common;
using EazyPOS.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Voice;
using Twilio.Types;

namespace EazyPOS.Controllers
{
    public class CityStateController : ControllerBase
    {
        private readonly IConfiguration _Config;
        APIResponse ObjB;   
        private readonly CityState _CityState;

        public CityStateController(IConfiguration configuration)
        {
            _Config = configuration;
            ObjB = new APIResponse(_Config);            
            _CityState = new CityState(_Config);
        }


        [HttpPost]
        [Route("api/CityMaster/List")]
        public async Task<IActionResult> CityList ([FromBody] GetParameterValue objvalue)
        {
            try
            {
                int userId = int.Parse(Request.Headers["UserId"]);
                object result = await _CityState.GetCityList(userId, objvalue);
                var resultDict = result as Dictionary<string, object>;
                return ObjB.ResponseFormat(resultDict, true, "Success!");
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
        [Route("api/StateMaster/List")]
        public async Task<IActionResult> StateList([FromBody] GetParameterValue objvalue)
        {
            try
            {
                int userId = int.Parse(Request.Headers["UserId"]);
                object result = await _CityState.GetStateList(userId, objvalue);
                var resultDict = result as Dictionary<string, object>;
                return ObjB.ResponseFormat(resultDict, true, "Success!");
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
        [Route("api/CountryMaster/List")]
        public async Task<IActionResult> CountryList([FromBody] GetParameterValue objvalue)
        {
            try
            {
                int userId = int.Parse(Request.Headers["UserId"]);
                object result = await _CityState.GetCountryList(userId, objvalue);
                var resultDict = result as Dictionary<string, object>;
                return ObjB.ResponseFormat(resultDict, true, "Success!");
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
        //Changes By Deepak Goyal


    }
}
