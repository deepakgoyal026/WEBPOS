using Devart.Data.PostgreSql;
using DocumentFormat.OpenXml.Spreadsheet;
using EazyPOS.Common;
using EazyPOS.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EazyPOS.BLL
{
    public class Authenticate : DBConnect
    {
        public static TokenModel ObjToken;
        public static string AuthTokenKey;
        EncryptDecrypt_v1 _EncryptDecrypt_v1;
        BaseDBConnect _BaseDBConnect;
        IConfiguration Config;
        LoggerService _LoggerService;
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment _environment;
        PgSqlDataAdapter da = null;
        DataTable Dt=new DataTable ();

        public Authenticate(IConfiguration _Config) : base(_Config)
        {
            _EncryptDecrypt_v1 = new EncryptDecrypt_v1();
            Config = _Config;
            _LoggerService = new LoggerService(Config, _environment);
        }       

        //public async Task<object> IsValidUser(string userName, string pwd)
        public async Task<object> IsValidUser(LoginModel ObjLogin)
        {
            var resultDict = new Dictionary<string, object>();
            try
            {
                DataTable DT;
                StringBuilder sQuery = new StringBuilder();

                sQuery.AppendLine("SELECT su.id ,su.username ,su.employeeid ,he.employeename ,su.password,false as superuser  ");
                sQuery.AppendLine("from sec_usermaster su  ");
                sQuery.AppendLine("left join hr_employeemaster he on su.employeeid = he.employeeid ");
                sQuery.AppendLine("WHERE LOWER(su.username)='" + ObjLogin.PosUserName + "' AND su.isactive=true AND  su.isauthorized=true;");

                _LoggerService.WriteLogs("IsValidUser:- " + sQuery.ToString());
                DT = ReturnDataTable(sQuery.ToString(),ref da,ref Dt);
                if (DT != null)
                {
                    if (DT.Rows.Count > 0)
                    {
                        _LoggerService.WriteLogs("with main db DT:- " + DT.Rows.Count);
                        if (DT.Rows.Count == 1)// && DbNull(DT.Rows[0]["password"], VarType.Text) == _EncryptDecrypt_v1.EncryptStringFromPlainText_Aes(Encoding.UTF8.GetString(Convert.FromBase64String(ObjLogin.Password))))                         
                        {
                            UserModel user = new UserModel();
                            
                            user.UserId = DbNull(DT.Rows[0]["id"], VarType.Integer32);
                            user.UserName = DbNull(DT.Rows[0]["username"], VarType.Text); 
                            user.EmployeeName = DbNull(DT.Rows[0]["employeename"], VarType.Text);
                            user.EmployeeId = DbNull(DT.Rows[0]["employeeid"], VarType.Integer32);
                            user.issuperuser = DbNull(DT.Rows[0]["superuser"], VarType.Bool);

                            resultDict.Add("Status", true);
                            resultDict.Add("Result", user);
                        }
                        else
                        {
                            resultDict.Add("Status", false);
                            resultDict.Add("Result", "Invalid password!");
                        }
                    }
                    else
                    {
                        resultDict.Add("Status", false);
                        resultDict.Add("Result", "User name not found! or not active user!");
                    }
                }
                else
                {
                    resultDict.Add("Status", false);
                    resultDict.Add("Result", "Something went wrong in server!");
                }
            }
            catch (Exception ex)
            {
                resultDict.Add("Status", false);
                resultDict.Add("Result", ex.Message);
            }

            return resultDict;
        }

        public string BuildToken(int userId, string url)
        {
            _BaseDBConnect = new BaseDBConnect(Config);
            DataTable dt = new DataTable();
            try
            {
                if (userId == 0)
                {
                    return "";
                }

                TokenModel tokenModel = new TokenModel();
                tokenModel.DatabaseServer = UserInfo._DatabasServer;
                tokenModel.DatabasePort = UserInfo._DatabasePort;
                tokenModel.DatabaseName = UserInfo._DatabaseName;
                tokenModel.SchemaName = UserInfo._SchemaName;
                tokenModel.UserID = userId.ToString();
                tokenModel.Origin = url;

                string StrToken = EncryptToken(tokenModel);
                AuthTokenKey = StrToken;

                _LoggerService.WriteLogs("BuildToken:- " + StrToken);

                StringBuilder sQuery = new System.Text.StringBuilder();

                sQuery.Clear();
                sQuery.Append("update Sec_UserSessionLog set LogoutTIme=now(),IsAsctive=false where UserID=" + userId + " and IsAsctive=true  and moduletype='EazyPOS';");
                _BaseDBConnect.PgExecuteScript(sQuery.ToString());

                //sQuery.Clear();
                //sQuery.Append("insert into Sec_UserSessionLog(UserID,Token,IsAsctive,LoginTime,LastAcessedTime,IPAddress,moduletype)");
                //sQuery.Append("values(" + userId + ",'" + StrToken + "',true,now(),now(),'','EazyPOS');");
                //_BaseDBConnect.PgExecuteScript(sQuery.ToString());

                string CheckTokenQuery = @"update Sec_UserSessionLog set LogoutTIme=now(),IsAsctive=false where UserID=@userid and IsAsctive=true  and moduletype='EazyPOS' ";
                using (var cmd = new PgSqlCommand(CheckTokenQuery, _BaseDBConnect.PgCon_V1))
                {
                    cmd.Parameters.AddWithValue("@userid", userId);
                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }
                    cmd.ExecuteNonQuery();
                }

                CheckTokenQuery = @"insert into Sec_UserSessionLog(UserID,Token,IsAsctive,LoginTime,LastAcessedTime,IPAddress,moduletype)";
                CheckTokenQuery += " values(@userid,@token,true,now(),now(),'','EazyPOS');";  /*ON CONFLICT (token) DO NOTHING*/
                using (var cmd = new PgSqlCommand(CheckTokenQuery, _BaseDBConnect.PgCon_V1))
                {
                    cmd.Parameters.AddWithValue("@userid", userId);
                    cmd.Parameters.AddWithValue("@token", StrToken.ToString());
                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return "";
                    }
                    else
                    {
                        return StrToken;
                    }
                }

                return StrToken;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetClientList()
        {
            string Client_ID = "";
            try
            {
                DataTable dt = new DataTable();
                StringBuilder sQuery = new StringBuilder();

                sQuery.Append("select client_id as clientid from comn_companydetail limit 1");

                dt = ReturnDataTable(sQuery.ToString(),ref da,ref Dt);
                Client_ID = dt.Rows[0]["clientid"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Client_ID;
        }

        public bool IsValidToken(string _TokenKey, ref string _Message)
        {
            _BaseDBConnect = new BaseDBConnect(Config);

            try
            {
                if (_TokenKey == null || _TokenKey == "")
                {
                    return false;
                }
                else 
                {
                    DataTable dtLog = new DataTable();
                    //dtLog = PgGetDetDT("select * from Sec_UserSessionLog where Token='" + _TokenKey + "'and IsAsctive = true and moduletype='EazyPOS' ");
                    dtLog = _BaseDBConnect.PgGetDetDT("select sessionid from Sec_UserSessionLog where Token='" + _TokenKey + "' and moduletype='EazyPOS' ");
                    if (dtLog == null || dtLog.Rows.Count == 0)
                    {
                        _Message = "Authentication failed due to invalid token.";
                        return false;
                    }
                    else
                    {
                        _BaseDBConnect.PgExecuteScript("update Sec_UserSessionLog set lastacessedtime=now() where Token='" + _TokenKey + "' and IsAsctive=true  and moduletype='EazyPOS' ");
                        return true;
                    }
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<object> GetClientDetails(string url, string username,LoginModel ObjLogin)//(string url,string username)
        {
            _BaseDBConnect = new BaseDBConnect(Config);
            var _resultDict = new Dictionary<string, object>();

            try
            {
                string query = $"select * from agencymaster where inactive = false and webloginuser ='{username}' and origin = '{url}'";
                DataTable dt = _BaseDBConnect.PgGetDetDT(query);

                if (dt.Rows.Count != 0)
                {
                    _LoggerService.WriteLogs("GetClientDetails for settings database :-" + dt.Rows.Count);
                    //Base DB conection closed
                    _LoggerService.WriteLogs("GetClientDetails for settings database connection close");

                    UserInfo._DatabasServer = dt.Rows[0]["servername"].ToString();
                    UserInfo._DatabasePort = dt.Rows[0]["portno"].ToString();
                    UserInfo._DatabaseName = dt.Rows[0]["databasename"].ToString();
                    UserInfo._SchemaName = dt.Rows[0]["schemaname"].ToString();
                    ObjLogin.PosUserName = dt.Rows[0]["posloginuser"].ToString();

                    //With main DB
                    PGConStr = BuildConnectionUserDatabase("");
                    //PGConnection();
                    _LoggerService.WriteLogs("GetClientDetails for main database connection strig :- " + PGConStr);

                    _resultDict.Add("Status", true);
                    _resultDict.Add("Result", "");
                    return _resultDict;
                }
                else
                {
                    _resultDict.Add("Status", false);
                    _resultDict.Add("Result", "Client does not exist!");
                    return _resultDict;
                }
                
            }
            catch (Exception ex)
            {
                _resultDict.Add("Status", false);
                _resultDict.Add("Result", ex.Message);
                return _resultDict;
            }
        }

        public async Task<object> GetUserDetail(int id)
        {
            var resultDict = new Dictionary<string, object>();
            try
            {
                DataTable DT;
                StringBuilder sQuery = new StringBuilder();

                //sQuery.AppendLine("SELECT UM.id,UM.username, EM.employeename,UM.password,Em.EmployeeId,(case when UM.issuperuser=true then 'true' else 'false' end) superuser ");
                //sQuery.AppendLine("FROM sec_usermaster AS UM");
                //sQuery.AppendLine("LEFT JOIN hr_EmployeeMaster AS EM ON EM.employeeid = UM.employeeid ");
                //sQuery.AppendLine("WHERE UM.id=" + id + " AND UM.isactive=true AND  UM.isauthorized=true;");

                sQuery.AppendLine("SELECT su.id ,su.username ,su.employeeid ,he.employeename ,su.password,false as superuser  ");
                sQuery.AppendLine("from sec_usermaster su  ");
                sQuery.AppendLine("left join hr_employeemaster he on su.employeeid = he.employeeid ");
                sQuery.AppendLine("WHERE su.id=" + id + " AND su.isactive=true AND  su.isauthorized=true;");

                DT = ReturnDataTable(sQuery.ToString(), ref da, ref Dt);
                if (DT != null)
                {
                    if (DT.Rows.Count > 0)
                    {
                        UserModel user = new UserModel();

                        user.UserId = DbNull(DT.Rows[0]["id"], VarType.Integer32);
                        user.UserName = DbNull(DT.Rows[0]["username"], VarType.Text);
                        user.EmployeeName = DbNull(DT.Rows[0]["employeename"], VarType.Text);
                        user.EmployeeId = DbNull(DT.Rows[0]["employeeid"], VarType.Integer32);
                        user.issuperuser = DbNull(DT.Rows[0]["superuser"], VarType.Bool);

                        resultDict.Add("Status", true);
                        resultDict.Add("Result", user);
                    }
                    else
                    {
                        resultDict.Add("Status", false);
                        resultDict.Add("Result", "User name not found or not active user!");
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
