using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace EazyPOS
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsRemeber { get; set; }
        public string PosUserName { get; set; }
    }

    public class UserModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public bool issuperuser { get; set; }
        public string Emp_EMail { get; set; }
    }    

    public class LoginResponseModel
    {
        public UserModel UserDet { get; set; }
        public DataTable Branches { get; set; }
        public string ClientId { get; set; }
    }

    public class UserMaster 
    {
        public string UserName { get; set; }

        [MinLength(5, ErrorMessage = "Minimum Length Should be 5 characters")]
        [MaxLength(15, ErrorMessage = "Maximum 15 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\S)(?=.*[$@$!%*?&])[A-Za-z\d$#@$!%*?&]{8,15}$", ErrorMessage = "Password format is not valid")]
        public string Password { get; set; }
        public int EmployeeId { get; set; }
        public int UserId { get; set; }
        public int UserType { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
        public string EmailId { get; set; }
        public string Mobile { get; set; }
        public string OTP { get; set; }
        public int StatusCode { get; set; }
        public bool IsShowCaptcha { get; set; }
    }

    public static class UserInfo
    {
        public static string _DatabasServer { get; set; }
        public static string _DatabasePort { get; set; }
        public static string _DatabaseName { get; set; }
        public static string _SchemaName { get; set; }
        public static string _Origin { get; set; }
        public static string _UserID { get; set; }
    }

    public class GetParameterValue
    {
        public DateTime alteredon { get; set; }
        public int pageindexno { get; set; }
    }

}
