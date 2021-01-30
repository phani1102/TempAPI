using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace WorkRewards.Data.Utility
{
    public class SummitDBUtils
    {
        #region "Variable declaration"
        private SqlConnection m_sqlCon;
        private int m_sqlCommandTimeout = 0;
        private string m_strConnectionString = string.Empty;
        private bool _returnProviderSpecificTypes = false;

        private int _sqlCommandTimeout = 0;
        public int CommandTimeout
        {
            get
            {
                return _sqlCommandTimeout;
            }
            set
            {
                _sqlCommandTimeout = value;
            }
        }
        public string ConnectionString { get; set; } = string.Empty;
        public Boolean ReturnProviderSpecificTypes
        {
            get
            {
                return _returnProviderSpecificTypes;
            }
            set
            {
                _returnProviderSpecificTypes = value;
            }
        }

        public const string strSuccess = "success";
        #endregion

        #region "Transactional method"
        private SqlTransaction m_sqlTransaction;
        private ILogger logger;
        public SummitDBUtils()
        {

        }
        public SummitDBUtils(IConfiguration _config, ILogger logger)
        {
            this.logger = logger;
            this.ConnectionString = _config.GetSection("AppSettings").GetSection("ConnectionString").Value;
        }

        private void LogError(string error)
        {
            if (logger != null)
            {
                logger.LogError(error);
            }
        }
        public void BeginTransaction(string argStrDBConnName, IConfigurationRoot config)
        {
            try
            {

                // IConfigurationRoot config;
                m_strConnectionString = config.GetSection("AppSettings").GetSection(argStrDBConnName).Value;
                m_sqlCon = new SqlConnection(m_strConnectionString);
                m_sqlCon.Open();

                m_sqlTransaction = m_sqlCon.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
            }
        }
        public void EndTransaction(bool rollback = false)
        {
            try
            {

                if (m_sqlTransaction != null)
                {
                    if (rollback)
                        m_sqlTransaction.Rollback();
                    else
                        m_sqlTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                try
                {
                    m_sqlTransaction = null;
                    if ((m_sqlCon.State == System.Data.ConnectionState.Open))
                        m_sqlCon.Close();
                    m_sqlCon.Dispose();
                    m_strConnectionString = string.Empty;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public string ExecuteSQLNonQuery(string argStrCmdText, string argStrDBConnName, IConfigurationRoot config, SqlParameter[] argSpParameters = null)
        {
            string strStat = string.Empty;
            SqlCommand ScCommandToExecute;
            SqlConnection m_sqlConn = new SqlConnection();
            try
            {

                // Selecting the appropriate DB connection string from the configuration and opening the connection
                m_strConnectionString = config.GetSection("AppSettings").GetSection(argStrDBConnName).Value;
                m_sqlConn = new SqlConnection(m_strConnectionString);
                m_sqlConn.Open();

                // Creating the SQL Command object. If there are parameters, then the command object with parameters are 
                // created
                if ((!(argSpParameters == null)))
                    ScCommandToExecute = CreateSqlCommand(ref argStrCmdText, ref argSpParameters, ref m_sqlConn);
                else
                    ScCommandToExecute = new SqlCommand(argStrCmdText, m_sqlConn);

                ScCommandToExecute.CommandType = CommandType.StoredProcedure;

                // Setting the command time out, if given in the configuration
                try
                {
                    m_sqlCommandTimeout = Int32.Parse(config.GetSection("AppSettings").GetSection("sqlCommandTimeout").Value);
                }
                catch (Exception ex)
                {
                    m_sqlCommandTimeout = 0;
                    throw ex;
                }

                // If (m_sqlCommandTimeout > 0) Then
                ScCommandToExecute.CommandTimeout = m_sqlCommandTimeout;
                // End If

                //SummitDBTrace _objTrace = new SummitDBTrace(argSpParameters);
                //_objTrace.CommandText = argStrCmdText;

                //_objTrace.WriteBeforeExecuteSQL();

                ScCommandToExecute.ExecuteNonQuery();
                strStat = strSuccess;

                //_objTrace.WriteAfterExecuteSQL();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                try
                {
                    m_sqlConn.Close();
                    m_sqlConn.Dispose();
                    m_strConnectionString = string.Empty;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return strStat;
        }

        public DataSet ExecuteSQLQuery(string argStrCmdText, string argStrDBConnName, SqlParameter[] argSpParameters = null, string dbConnection = "DVServiceBaseURL")
        {
            DataSet ds = new DataSet();
            SqlCommand ScCommandToExecute;
            SqlConnection m_sqlConn = new SqlConnection();
            try
            {
                // Selecting the appropriate DB connection string from the configuration and opening the connection
                m_strConnectionString = dbConnection;
                m_sqlConn = new SqlConnection(m_strConnectionString);
                m_sqlConn.Open();

                // Creating the SQL Command object. If there are parameters, then the command object with parameters are 
                // created
                if ((!(argSpParameters == null)))
                    ScCommandToExecute = CreateSqlCommand(ref argStrCmdText, ref argSpParameters, ref m_sqlConn);
                else
                    ScCommandToExecute = new SqlCommand(argStrCmdText, m_sqlConn);

                ScCommandToExecute.CommandType = CommandType.StoredProcedure;

                // Setting the command time out, if given in the configuration
                try
                {
                    m_sqlCommandTimeout = 20;// Int32.Parse(config.GetSection("AppSettings").GetSection("sqlCommandTimeout").Value);
                }
                catch (Exception ex)
                {
                    m_sqlCommandTimeout = 0;
                    throw ex;
                }

                // If (m_sqlCommandTimeout > 0) Then
                ScCommandToExecute.CommandTimeout = m_sqlCommandTimeout;
                // End If

                SqlDataAdapter da = new SqlDataAdapter(ScCommandToExecute);
                da.SelectCommand = ScCommandToExecute;
                da.ReturnProviderSpecificTypes = ReturnProviderSpecificTypes;
                //  SummitDBTrace _objTrace = new SummitDBTrace(argSpParameters);
                // _objTrace.CommandText = argStrCmdText;

                //  _objTrace.WriteBeforeExecuteSQL();
                da.Fill(ds);
                // _objTrace.WriteAfterExecuteSQL();
            }

            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    m_sqlConn.Close();
                    m_sqlConn.Dispose();
                    m_strConnectionString = string.Empty;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return ds;
        }
        public T ExecuteSQLScalarQuery<T>(string cmdText, SqlParameter[] parameters = null)
        {
            SqlCommand cmd;
            T returnValue;

            SqlConnection m_sqlConn = new SqlConnection();
            try
            {
                // Selecting the appropriate DB connection string from the configuration and opening the connection
                m_sqlConn = new SqlConnection(ConnectionString);
                m_sqlConn.Open();

                cmd = parameters != null ? CreateSqlCommand(ref cmdText, ref parameters, ref m_sqlConn) : new SqlCommand(cmdText, m_sqlConn);
                cmd.CommandType = CommandType.StoredProcedure;

                m_sqlCommandTimeout = 20;// Int32.Parse(config.GetSection("AppSettings").GetSection("sqlCommandTimeout").Value);

                cmd.CommandTimeout = m_sqlCommandTimeout;
                var res = cmd.ExecuteScalar();

                returnValue = res != DBNull.Value ? (T)Convert.ChangeType(res, typeof(T)) : default(T);
            }

#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                string strTest = GetEXECParametersToString(parameters, cmdText);
                this.logger.LogError("SP Error :- " + strTest);
                throw ex;
            }
            finally
            {
                try
                {
                    m_sqlConn.Close();
                    m_sqlConn.Dispose();
                    m_strConnectionString = string.Empty;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return returnValue;
        }

        public string ExecuteSQLNonQuery(string argStrCmdText, SqlParameter[] argSpParameters = null)
        {
            string strStat = string.Empty;
            SqlCommand ScCommandToExecute;
            SqlConnection m_sqlConn = new SqlConnection();
            try
            {

                // Setting connection string and opening the connection
                m_strConnectionString = ConnectionString;
                m_sqlConn = new SqlConnection(m_strConnectionString);
                m_sqlConn.Open();

                // Creating the SQL Command object. If there are parameters, then the command object with parameters are 
                // created
                if ((!(argSpParameters == null)))
                    ScCommandToExecute = CreateSqlCommand(ref argStrCmdText, ref argSpParameters, ref m_sqlConn);
                else
                    ScCommandToExecute = new SqlCommand(argStrCmdText, m_sqlConn);

                ScCommandToExecute.CommandType = System.Data.CommandType.StoredProcedure;

                // Setting the command time out, if set
                // If (CommandTimeout > 0) Then
                ScCommandToExecute.CommandTimeout = CommandTimeout;
                // End If

                //SummitDBTrace _objTrace = new SummitDBTrace(argSpParameters);
                //_objTrace.CommandText = argStrCmdText;

                //_objTrace.WriteBeforeExecuteSQL();

                ScCommandToExecute.ExecuteNonQuery();
                strStat = strSuccess;

                //_objTrace.WriteAfterExecuteSQL();
            }
            catch (Exception ex)
            {
                string strTest = GetEXECParametersToString(argSpParameters, argStrCmdText);
                // LogError("Proc Error Testcase :- " + strTest);
                this.logger.LogInformation("SP Error :- " + strTest);
                throw ex;
            }

            finally
            {
                try
                {
                    m_sqlConn.Close();
                    m_sqlConn.Dispose();
                    m_strConnectionString = string.Empty;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return strStat;
        }

        public System.Data.DataSet ExecuteSQLQuery(string argStrCmdText, SqlParameter[] argSpParameters = null)
        {
            DataSet ds = new DataSet();
            SqlCommand ScCommandToExecute;
            SqlConnection m_sqlConn = new SqlConnection();
            try
            {
                // Setting connection string and opening the connection
                m_strConnectionString = ConnectionString;
                m_sqlConn = new SqlConnection(m_strConnectionString);
                m_sqlConn.Open();

                // Creating the SQL Command object. If there are parameters, then the command object with parameters are 
                // created
                if ((!(argSpParameters == null)))
                    ScCommandToExecute = CreateSqlCommand(ref argStrCmdText, ref argSpParameters, ref m_sqlConn);
                else
                    ScCommandToExecute = new SqlCommand(argStrCmdText, m_sqlConn);

                ScCommandToExecute.CommandType = CommandType.StoredProcedure;

                //Getting SP testcases in HTML format.
                // string strTest= GetEXECParametersToString(argSpParameters, argStrCmdText);

                // Setting the command time out, if set
                // If (CommandTimeout > 0) Then
                ScCommandToExecute.CommandTimeout = CommandTimeout;
                // End If

                SqlDataAdapter da = new SqlDataAdapter(ScCommandToExecute);
                da.SelectCommand = ScCommandToExecute;
                da.ReturnProviderSpecificTypes = ReturnProviderSpecificTypes;

                //SummitDBTrace _objTrace = new SummitDBTrace(argSpParameters);
                //_objTrace.CommandText = argStrCmdText;

                //_objTrace.WriteBeforeExecuteSQL();
                da.Fill(ds);
                //  _objTrace.WriteAfterExecuteSQL();
            }


            catch (Exception ex)
            {
                //Getting SP testcases in HTML format.
                string strTest = GetEXECParametersToString(argSpParameters, argStrCmdText);
                this.logger.LogInformation("SP Error :- " + strTest);
                throw ex;
            }
            finally
            {
                try
                {
                    m_sqlConn.Close();
                    m_sqlConn.Dispose();
                    m_strConnectionString = string.Empty;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return ds;
        }

        private SqlCommand CreateSqlCommand(ref string argStrCmdText, ref SqlParameter[] argSpParams, ref SqlConnection argSqlCon)
        {
            SqlCommand ScWithArgs = new SqlCommand(argStrCmdText, argSqlCon);
            int intCount = 0;
            while ((intCount < argSpParams.Length))
            {
                ScWithArgs.Parameters.Add(argSpParams[intCount]);
                intCount = intCount + 1;
            }

            return ScWithArgs;
        }

        public static List<T> ConvertDataTableToGenericList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                   .Select(c => String.Join("", c.ColumnName.Split('_')).ToLower())
                   .ToList();

            var properties = typeof(T).GetProperties();
            DataRow[] rows = dt.Select();
            return rows.Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                        pro.SetValue(objT, row[pro.Name.ToLower()]);
                }

                return objT;
            }).ToList();
        }
        #endregion

        #region SP Helper
        /// <summary>
        /// This Method used for Genarate SP Paramaters in HTML Table format
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="StoreProc"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetEXECParametersToString(SqlParameter[] parameters, string StoreProc, Exception ex = null)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append(" <table border='1' >");

            //if (ex != null)
            //{
            //    if (!string.IsNullOrEmpty(ex.Message))
            //        sb.Append("<tr><td colspan='2'> EXEC " + ex.Message + "  </td></tr>");
            //}

            sb.Append("EXEC " + StoreProc);

            int i = 0;
            foreach (SqlParameter item in parameters)
            {
                if (item.DbType == DbType.Object)
                {
                    string strTvp = GenarateTVPFormat((DataTable)item.Value);
                    sb.AppendFormat("  {2}{0} ={1} ", item.ParameterName, (strTvp != null) ? string.Format("{0}", strTvp) : "NULL", i > 0 ? "," : "");
                }
                else
                {
                    if (item.DbType == DbType.Int32 || item.DbType == DbType.Decimal || item.DbType == DbType.Int64)
                        sb.AppendFormat(" {2}{0} = {1}", item.ParameterName, (item.Value != null) ? string.Format("{0}", Convert.ToString(item.Value)) : "NULL", i > 0 ? "," : "");
                    else
                        sb.AppendFormat(" {2}{0} = {1}", item.ParameterName, (item.Value != null) ? string.Format("'{0}'", Convert.ToString(item.Value)) : "NULL", i > 0 ? "," : "");
                }
                i++;
            }
            if (ex != null)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                    sb.AppendFormat("Detail :-  {0}", (ex == null) ? "" : ex.ToString());
            }
            // sb.Append(" </table>");
            return sb.ToString();
        }

        private string GenarateTVPFormat(DataTable dt)
        {
            string tvpList = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                string s = string.Empty;
                for (int i = 0; row.ItemArray.Length > i; i++)
                {
                    if (dt.Columns[i].DataType == typeof(decimal) || dt.Columns[i].DataType == typeof(bool) || dt.Columns[i].DataType == typeof(int))
                        s += "," + ((row.ItemArray[i].ToString() == "") ? "null" : row.ItemArray[i].ToString());
                    else if (dt.Columns[i].DataType == typeof(DateTime))
                        s += "," + ((row.ItemArray[i].ToString() == "") ? "null" : "'" + row.ItemArray[i].ToString() + "'");
                    else
                        s += "," + ("'" + row.ItemArray[i].ToString() + "'");
                }
                s = s.TrimStart(',');
                tvpList += " (" + s + "), ";
            }
            return tvpList;
        }
        #endregion
    }
}
