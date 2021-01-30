using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using WorkRewards.Data.Interface;
using WorkRewards.Data.Utility;
using WorkRewards.DTO.Request;
using WorkRewards.DTO.Response;
using System.Linq;
namespace WorkRewards.Data
{
    public class LoginData : ILoginData
    {
        private readonly string CbmsConnectionString;
        SummitDBUtils dbUtil;//= new SummitDBUtils();
        ILogger<LoginData> logger;
        IConfiguration config;
        public LoginData(IConfiguration _config)
        {
           
            config = _config;
            this.CbmsConnectionString = config.GetSection("AppSettings").GetSection("ConnectionString").Value;
            dbUtil = new SummitDBUtils(_config, logger);
            dbUtil.ConnectionString = this.CbmsConnectionString;
        }
        public User Register(UserRequest userRequest)
        {
            DataSet dsResult = new DataSet();
            try
            {
                var spParams = new SqlParameter[] {
                    new SqlParameter("@First_Name", userRequest.FirstName),
                    new SqlParameter("@Last_Name", userRequest.LastName),
                    new SqlParameter("@Middle_Name", userRequest.MiddleName),
                    new SqlParameter("@Username", userRequest.UserName),
                    new SqlParameter("@Password", userRequest.Password),
                    new SqlParameter("@Email", userRequest.Email),
                    new SqlParameter("@Mobile_No", userRequest.MobileNo),
                    new SqlParameter("@Role_Id", userRequest.RoleId)
                };
                dsResult = dbUtil.ExecuteSQLQuery("User_Details_Insert", spParams);
                if (dsResult != null && dsResult.Tables.Count > 0)
                {


                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("LoadDivision" + " " + ex.Message.ToString());
            }
            return new User();
        }
    }
}
