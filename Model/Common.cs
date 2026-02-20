namespace EazyPOS.Model
{
	public class ResponseResult
	{
		public bool Status { get; set; }
		public int StatusCode { get; set; }
		public string Message { get; set; }
		public string Token { get; set; }
		public object Result { get; set; }
	}
	public class TokenModel
	{
		public string DatabaseName { get; set; }
		public string SchemaName { get; set; }
		public string DatabaseServer { get; set; }
		public string DatabasePort { get; set; }
		public string Origin { get; set; }
		public string UserID { get; set; }
		public string Emp_EMail { get; set; }
	}

}
