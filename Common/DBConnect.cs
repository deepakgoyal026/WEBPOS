using Devart.Data.PostgreSql;
using EazyPOS.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Text;

namespace EazyPOS.Common
{
    public class DBConnect
    {
        private readonly IConfiguration Config;
        public PgSqlConnection PgCon;
        private PgSqlCommand PgCmd;
        private PgSqlDataAdapter PgAdp;
        private PgSqlScript PgScr;
        public PgSqlTransaction PgTran;
        DataTable DT;
        DataView DV;
        DataRow DR;
        internal static string PGConStr;
        public static int UserId;
        public static int EmployeeId;
        public static string EmployeeName;
        public static string UserName;
        public static int BranchId;
        public static int FormId;
        public bool UNICODEEnabled526 = false;

        public bool ExecutionDebugMode = false;
        public static string Session_GUID = Guid.NewGuid().ToString();
        public string ExeFilePath = "";
        EncryptDecrypt_v1 _EncryptDecrypt_v1;

        public DBConnect(IConfiguration _Config)
        {
            try
            {
                Config = _Config;
                _EncryptDecrypt_v1 = new EncryptDecrypt_v1();
                PGConStr = BuildConnectionUserDatabase();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public String BuildConnectionUserDatabase(string Database = "")
        {
            return commonConfig();
        }

        private string commonConfig()
        {
            StringBuilder connString = new StringBuilder();

            connString.Append("Host = " + UserInfo._DatabasServer + ";");
            connString.Append("Database = " + UserInfo._DatabaseName + ";");
            connString.Append("port = " + UserInfo._DatabasePort + ";");
            connString.Append("Schema = " + UserInfo._SchemaName + ";");
            connString.Append("User Id= " + _EncryptDecrypt_v1.DecryptStringFromBytes_Aes(Config.GetConnectionString("UID")) + ";");
            connString.Append("Password= " + _EncryptDecrypt_v1.DecryptStringFromBytes_Aes(Config.GetConnectionString("Pwd")) + ";");
            connString.Append("Pooling= true; Min Pool Size= 0; Max Pool Size= 1000;");
            connString.Append("ValidateConnection=true;");
            //connString.Append("maxLifetime=60000;");
            //connString.Append("Connection Idle Lifetime=300;");
            connString.Append("applicationname=eazypos;");
            connString.Append("License Key= " + Config.GetConnectionString("DevartKeyPath") + ";");
            return connString.ToString();
        }

        public bool PGConnection()
        {
            try
            {
                if (PGConStr != String.Empty)
                {
                    if (PgCon == null)
                    {
                        PgCon = new PgSqlConnection(PGConStr);
                        PgCon.Open();
                        if (UNICODEEnabled526 == true) { PgCon.Unicode = true; }
                    }
                    else if (PgCon.State == ConnectionState.Closed)
                    {
                        PgCon.Open();
                        if (UNICODEEnabled526 == true) { PgCon.Unicode = true; }
                    }
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
     
        public string PgScaler(string StrQuery)
        {
            try
            {
                if (PGConnection() == true)
                {
                    if (StrQuery.Contains("$~"))
                    {
                        StrQuery = SplitQuery(StrQuery);
                    }

                    PgCmd = new PgSqlCommand(StrQuery, PgCon);
                    Object Obj = PgCmd.ExecuteScalar();

                    if (Obj != null)
                    {
                        return Obj.ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    
        public string SplitQuery(string _StrQuery)
        {
            try
            {
                string[] StrQueries = _StrQuery.Split("$~");
                for (int i = 0; i < StrQueries.Length - 1; i++)
                {
                    PgExecute(StrQueries[i]);
                }
                return StrQueries[StrQueries.Length - 1];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public long PgExecute(string StrQry)
        {
            try
            {
                if (PGConnection() == true)
                {
                    PgCmd = new PgSqlCommand(StrQry, PgCon);
                    PgCmd.CommandTimeout = 0;
                    return PgCmd.ExecuteNonQuery();
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
     
        public void PGConnection_Close()
        {
            try
            {
                if (PgCon != null && PgCon.State != ConnectionState.Closed)
                {
                    ExecuteQuery("select * from drop_inactive_connections()");
                    PgCon.Close();
                    PgCon.Dispose();
                }
            }
            catch (Exception ex)
            {
                //throw;
            }
        }

        public string EncryptToken(TokenModel ObjToken)
        {
            try
            {
                string toEncrypt = ObjToken.DatabaseServer + ";" + ObjToken.DatabasePort + ";" + ObjToken.DatabaseName + ";" + ObjToken.Origin + ";" + ObjToken.UserID + ";" + ObjToken.SchemaName + ";" + Guid.NewGuid().ToString();
                return _EncryptDecrypt_v1.EncryptStringFromPlainText_Aes(toEncrypt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable ReturnDataTable(string query, ref PgSqlDataAdapter dataAdapter,  ref DataTable dt)
        {
            using (PgSqlConnection conn = new PgSqlConnection(PGConStr))
            {
                conn.Open();
                using (PgSqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    dataAdapter = new PgSqlDataAdapter(cmd);
                    dt.Clear();
                    dataAdapter.Fill(dt);
                    return dt;               }
            }
        }

        public DataRow ReturnRecord(string query, ref PgSqlTransaction objTransaction, bool bTransBegin = false)
        {

            try
            {
                using (PgSqlCommand command = new PgSqlCommand())
                {
                    command.Connection = PgCon;
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;

                    if (PgCon.State == ConnectionState.Closed)
                        PgCon.Open();

                    if (bTransBegin)
                        objTransaction = PgCon.BeginTransaction();

                    command.Transaction = objTransaction;

                    using (PgSqlDataAdapter da = new PgSqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                            return dt.Rows[0];
                        else
                            return null;
                    }
                }
            }
            catch (PgSqlException ex)
            {
                if (objTransaction != null)
                {
                    PgCon?.Rollback();
                }

                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void ExecuteQuery(string sQuery)
        {
            try
            {
                if (PGConnection() == true)
                {
                    PgCmd = new PgSqlCommand(sQuery, PgCon);
                    PgCmd.CommandTimeout = 0;
                    PgCmd.ExecuteNonQuery();
                }
                else
                {
                    return ;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public enum VarType
        {
            Number,
            Text,
            Bool,
            Dec,
            Integer32,
            Integer64,
            Integer16,
            Date,
            DateTime
        }
        public dynamic DbNull(dynamic value, VarType t)
        {
            if (value.Equals(System.DBNull.Value))
            {
                switch (t)
                {
                    case VarType.Number:
                        return 0;
                    case VarType.Dec:
                        return 0.0;
                    case VarType.Integer16:
                        return 0;
                    case VarType.Integer32:
                        return 0;
                    case VarType.Integer64:
                        return 0;
                    case VarType.Text:
                        return "";
                    case VarType.Bool:
                        return false;
                    case VarType.Date:
                        return null;
                    case VarType.DateTime:
                        return null;
                }
            }

            switch (t)
            {
                case VarType.Number:
                case VarType.Dec:
                    return Convert.ToDecimal(value);
                case VarType.Integer16:
                    return Convert.ToInt16(value);
                case VarType.Integer32:
                    return Convert.ToInt32(value);
                case VarType.Integer64:
                    return Convert.ToInt64(value);
                case VarType.Text:
                    return Convert.ToString(value);
                case VarType.Bool:
                    return Convert.ToBoolean(value);
                case VarType.Date:
                    return value;
                case VarType.DateTime:
                    return Convert.ToDateTime(value);
            }
            return value;
        }
        public string FormatMyDate(object d, bool bApos = true, bool bWithTime = false)
        {
            if (d == null || d == DBNull.Value)
                return bApos ? "''" : "";

            if (!DateTime.TryParse(d.ToString(), out DateTime dt))
                return bApos ? "''" : "";

            string timeFormat = bWithTime ? " hh:mm:ss tt" : "";
            string formattedDate = dt.ToString("dd-MMM-yyyy" + timeFormat);

            return bApos ? $"'{formattedDate}'" : formattedDate;
        }
        public object CheckNull(object value, Type valueType, bool bExistCheckBox = false)
        {
            if (value == DBNull.Value || Convert.ToString(value) == "")
            {
                switch (valueType.Name)
                {
                    case "String":
                        return "";

                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Double":
                    case "Long":
                    case "Decimal":
                        return 0;

                    case "Boolean":
                        return false;

                    case "DateTime":
                    case "Date": // Note: "Date" is not a valid .NET type name, you may remove this case
                        if (bExistCheckBox)
                            return null;
                        else
                            return FormatMyDate(DateTime.Today, false);

                    default:
                        return null;
                }
            }
            else
            {
                return value;
            }
        }


    }
}




