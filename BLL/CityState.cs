using Devart.Data.PostgreSql;
using DocumentFormat.OpenXml.Vml;
using EazyPOS.Common;
using EazyPOS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Twilio.TwiML.Voice;


namespace EazyPOS.BLL
{
    public class CityState : DBConnect
    {
        EncryptDecrypt_v1 _EncryptDecrypt_v1;
        LoggerService _LoggerService;
        IConfiguration Config;
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment _environment;
        PgSqlDataAdapter da = null;
        DataTable Dt = new DataTable();

        public CityState(IConfiguration _Config) : base(_Config)
        {
            _EncryptDecrypt_v1 = new EncryptDecrypt_v1();
            Config = _Config;
            _LoggerService = new LoggerService(Config, _environment);
        }

        public async Task<object> GetCityList(int id,GetParameterValue objval)
        {
            var resultDict = new Dictionary<string, object>();
            try
            {
                DataTable DT;
                StringBuilder sQuery = new StringBuilder();
                sQuery.AppendLine("SELECT * FROM fun_masterdata_json_v1('CITYMASTER','0', True, True,'1900-01-01'," + id + ",0)  ");                

                DT = ReturnDataTable(sQuery.ToString(), ref da, ref Dt);
                if (DT != null)
                {
                    if (DT.Rows.Count > 0)
                    {
                        UserModel user = new UserModel();

                        var jsonString = DbNull(DT.Rows[0][0], VarType.Text);
                        var obj =  JsonSerializer.Deserialize<object>(jsonString);                      

                        resultDict.Add("Status", true);
                        resultDict.Add("Result", obj);
                    }
                    else
                    {
                        resultDict.Add("Status", false);
                        resultDict.Add("Result", "Data not found!");
                    }
                }
                else
                {
                    resultDict.Add("Status", false);
                    resultDict.Add("Result", "Something went wrong!");
                }
            }
            catch (Exception ex)
            {
                resultDict.Add("Status", false);
                resultDict.Add("Result", ex.Message);
            }
            return resultDict;
        }

        public async Task<object> GetStateList(int id, GetParameterValue objval)
        {
            var resultDict = new Dictionary<string, object>();
            try
            {
                DataTable DT;
                StringBuilder sQuery = new StringBuilder();
                sQuery.AppendLine("SELECT * FROM fun_masterdata_json_v1('STATEMASTER','0', True, True,'1900-01-01'," + id + ",0)  ");

                DT = ReturnDataTable(sQuery.ToString(), ref da, ref Dt);
                if (DT != null)
                {
                    if (DT.Rows.Count > 0)
                    {
                        UserModel user = new UserModel();

                        var jsonString = DbNull(DT.Rows[0][0], VarType.Text);
                        var obj = JsonSerializer.Deserialize<object>(jsonString);

                        resultDict.Add("Status", true);
                        resultDict.Add("Result", obj);
                    }
                    else
                    {
                        resultDict.Add("Status", false);
                        resultDict.Add("Result", "Data not found!");
                    }
                }
                else
                {
                    resultDict.Add("Status", false);
                    resultDict.Add("Result", "Something went wrong!");
                }
            }
            catch (Exception ex)
            {
                resultDict.Add("Status", false);
                resultDict.Add("Result", ex.Message);
            }
            return resultDict;
        }
        public async Task<object> GetCountryList(int id, GetParameterValue objval)
        {
            var resultDict = new Dictionary<string, object>();
            try
            {
                DataTable DT;
                StringBuilder sQuery = new StringBuilder();
                // Using same server-side helper as City/State implementations; name set to COUNTRYMASTER
                sQuery.AppendLine("SELECT * FROM fun_masterdata_json_v1('COUNTRYMASTER','0', True, True,'1900-01-01'," + id + ",0)  ");

                DT = ReturnDataTable(sQuery.ToString(), ref da, ref Dt);
                if (DT != null)
                {
                    if (DT.Rows.Count > 0)
                    {
                        var jsonString = DbNull(DT.Rows[0][0], VarType.Text);
                        var obj = JsonSerializer.Deserialize<object>(jsonString);

                        resultDict.Add("Status", true);
                        resultDict.Add("Result", obj);
                    }
                    else
                    {
                        resultDict.Add("Status", false);
                        resultDict.Add("Result", "Data not found!");
                    }
                }
                else
                {
                    resultDict.Add("Status", false);
                    resultDict.Add("Result", "Something went wrong!");
                }
            }
            catch (Exception ex)
            {
                resultDict.Add("Status", false);
                resultDict.Add("Result", ex.Message);
            }
            return resultDict;
        }

    }





}
