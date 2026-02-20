using Devart.Data.PostgreSql;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Drawing;
using System.Text;

namespace EazyPOS.Common
{
    public class BaseDBConnect
    {
        EncryptDecrypt_v1 _EncryptDecrypt_V1;
        private readonly IConfiguration _config;

        public PgSqlConnection PgCon_V1;
        private PgSqlCommand PgCmd_V1;
        private PgSqlDataAdapter PgAdp_V1;
        private PgSqlScript PgScr_V1;
        private PgSqlTransaction PgTran_V1;
        private string PGConStr_V1;

        DataTable DT = null;
        public bool UNICODEEnabled526 = false;
        EncryptDecrypt_v1 _EncryptDecrypt_v1;

        public BaseDBConnect(IConfiguration config) 
        {
            _config = config;
            _EncryptDecrypt_V1 = new EncryptDecrypt_v1();
            PGConStr_V1 = BuildConnString();
        }

        //Only for base database  
        public string BuildConnString()
        {
            try
            {
                StringBuilder connString = new StringBuilder();
                connString.Append("Database =" + _EncryptDecrypt_V1.DecryptStringFromBytes_Aes(_config.GetConnectionString("DBName")) + ";");
                connString.Append("Host = " + _EncryptDecrypt_V1.DecryptStringFromBytes_Aes(_config.GetConnectionString("Server")) + ";");
                connString.Append("port = " + _EncryptDecrypt_V1.DecryptStringFromBytes_Aes(_config.GetConnectionString("Port")) + ";");
                connString.Append("User Id= " + _EncryptDecrypt_V1.DecryptStringFromBytes_Aes(_config.GetConnectionString("UID")) + ";");
                connString.Append("Password= " + _EncryptDecrypt_V1.DecryptStringFromBytes_Aes(_config.GetConnectionString("Pwd")) + ";");
                connString.Append("Pooling= true; Min Pool Size= 0; Max Pool Size= 1000;");
                connString.Append("ValidateConnection=true;");
                connString.Append("License Key= " + _config.GetConnectionString("DevartKeyPath") + ";");
                return connString.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool PGConnection_V1()
        {
           
            try
            {
                if (PGConStr_V1 != String.Empty)
                {
                    if (PgCon_V1 == null)
                    {
                        PgCon_V1 = new PgSqlConnection(PGConStr_V1);
                        PgCon_V1.Open();
                        if (UNICODEEnabled526 == true)
                        {
                            PgCon_V1.Unicode = true;
                        }
                    }
                    else if (PgCon_V1.State == ConnectionState.Closed)
                    {
                        PgCon_V1.Open();
                        if (UNICODEEnabled526 == true)
                        {
                            PgCon_V1.Unicode = true;
                        }
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

        public DataTable PgGetDetDT(string StrQuery)
        {
            try
            {
                if (PGConnection_V1() == true && StrQuery != "")
                {
                    if (StrQuery.Contains("$~"))
                    {
                        StrQuery = SplitQuery(StrQuery);
                    }
                    PgAdp_V1 = new PgSqlDataAdapter(StrQuery, PgCon_V1);
                    DT = new DataTable();
                    PgAdp_V1.Fill(DT);
                    return DT;
                }
                return null;
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
                if (PGConnection_V1() == true)
                {
                    PgCmd_V1 = new PgSqlCommand(StrQry, PgCon_V1);
                    PgCmd_V1.CommandTimeout = 0;
                    return PgCmd_V1.ExecuteNonQuery();
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

        public long PgExecute(string StrQry, bool IsPgTran)
        {
            try
            {
                if (PGConnection_V1() == true)
                {
                    PgCmd_V1 = new PgSqlCommand(StrQry, PgCon_V1);
                    PgCon_V1.BeginTransaction();
                    PgCmd_V1.CommandTimeout = 50;
                    return PgCmd_V1.ExecuteNonQuery();
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

        public bool PgExecuteScript(string StrQry)
        {
            try
            {
                if (PGConnection_V1() == true)
                {
                    PgScr_V1 = new PgSqlScript(StrQry, PgCon_V1);
                    PgScr_V1.CommandTimeout = 50;
                    PgScr_V1.Execute();
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

        public DataTable ReturnDataTable(string query, ref PgSqlDataAdapter dataAdapter, ref DataTable dt)
        {
            using (PgSqlConnection conn = new PgSqlConnection(PGConStr_V1))
            {
                conn.Open();
                using (PgSqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    dataAdapter = new PgSqlDataAdapter(cmd);
                    dt.Clear();
                    dataAdapter.Fill(dt);
                    return dt;
                }
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

        
    }
}
