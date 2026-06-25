using AxiDataPackages.References;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Data;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace AxiDataPackages.Services.Helpers
{
    public class PackageHelperService
    {
        private readonly AxDBService _axDBService;

        private const string QUERY_SEPARATOR = "$D#";

        private readonly ILogger<PackageHelperService> _logger;

        public static Dictionary<string, List<string>> _customPageFiles = new Dictionary<string, List<string>>();

        private const string CUSTOM_PAGE_DELIMITER = "$D#$";


        public PackageHelperService(AxDBService axDBService, ILogger<PackageHelperService> logger)
        {
            _axDBService = axDBService;
            _logger = logger;
        }

        public async Task<(bool success, string[] recordId, string[] tableName)> ExportNotification(string transId)
        {

            _logger.LogInformation("ExportNotification Started : {transId}", transId);
            //string sql = $@"
            //    SELECT *
            //    FROM ax_configure_fast_prints 
            //    WHERE form_name = '{transId}'
            //";

            //string sql = "select * from ax_configure_fast_prints where form_name='incdv'";

            //ax_configure_fast_prints,form_name

            //string sql = "SELECT * FROM ax_configure_fast_prints WHERE form_name = @transID";
            //string[] paramNames = { "@transID" };
            //DbType[] paramTypes = { DbType.String };
            //object[] paramValues = { transId };

            //DataTable result = await _axDBService.ExecuteSelectQuery(sql,paramNames,paramTypes,paramValues);

            //string recordId = "";

            //string tableName = "";

            //if (result.Rows.Count > 0)
            //{
            //    DataRow row = result.Rows[0];

            //    recordId = row["ax_configure_fast_printsid"].ToString();

            //    tableName = row["form_name"].ToString();
            //}

            //return (recordId, tableName);

            //try
            //{
            //    bool success = false;

            //_logger.LogInformation("No Records Found");
            //    string[] recordId = new string[1];

            //    string[] tableName = new string[1];

            //    string sql = "SELECT * FROM axformnotify WHERE stransid = @transId";
            //    string[] paramNames = { "@transID" };
            //    DbType[] paramTypes = { DbType.String };
            //    object[] paramValues = { transId };

            //    _logger.LogInformation("Executing Query");

            //DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);


            //    if (result.Rows.Count > 0)
            //    {
            //        success = true;

            //        DataRow row = result.Rows[0];

            //        recordId[0] = row["axformnotifyid"].ToString();

            //        tableName[0] = "a__fn";
            //    }
            //    else
            //    {
            //        success = false;

            //_logger.LogInformation("No Records Found");
            //    }

            //    return (success, recordId, tableName);
            //}

            try
            {
                bool success = false;

                List<string> recordId = new List<string>();

                List<string> tableName = new List<string>();

                string sql = "SELECT * FROM axformnotify WHERE stransid = @transId  order by createdon asc";

                string[] paramNames = { "@transId" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { transId };

                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string ids = row["axformnotifyid"].ToString();

                        if (!string.IsNullOrWhiteSpace(ids))
                        {
                            recordId.Add(ids.Trim());

                            tableName.Add("a__fn");
                        }

                        //    if (!string.IsNullOrWhiteSpace(ids))
                        //{
                        //    string[] splitIds = ids.Split(',');

                        //    for (int i = 0; i < splitIds.Length; i++)
                        //    {
                        //        recordId.Add(splitIds[i].Trim());

                        //        tableName.Add("a__fn");
                        //    }
                        //}
                    }
                }
                else
                {
                    success = false;

                    _logger.LogInformation("No Records Found");
                }

                /*
              sql = "";

              paramNames = new string[] { };

              paramTypes = new DbType[] { };

              paramValues = new object[] { };

              result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

              if (result.Rows.Count > 0)
              {
                  DataRow row = result.Rows[0];

                  recordId[1] = row["columnname"].ToString();

                  tableName[1] = row["columnname"].ToString();
              }
              */

                _logger.LogInformation("Method Completed Successfully");

                return (success, recordId.ToArray(), tableName.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification Method Failed");
                throw;
            }
        }

        //permissions
        public async Task<(bool success, string[] recordId, string[] tableName)> ExportUserPermissions(string transID)
        {

            _logger.LogInformation("ExportUserPermissions Started : {transID}", transID);
            //a__up
            //select axpermissionsid from axpermissions where formtransid = 'slord
            //axpermissionsid,formtransid
            try
            {
                bool success = false;

                List<string> recordId = new List<string>();

                List<string> tableName = new List<string>();

                string sql = "SELECT axpermissionsid FROM axpermissions WHERE formtransid = @transID order by createdon asc";

                string[] paramNames = { "@transID" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { transID };

                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string ids = row["axpermissionsid"].ToString();

                        if (!string.IsNullOrWhiteSpace(ids))
                        {
                            recordId.Add(ids.Trim());
                            tableName.Add("a__up");
                        }

                        //if (!string.IsNullOrWhiteSpace(ids))
                        //{
                        //    string[] splitIds = ids.Split(',');

                        //    for (int i = 0; i < splitIds.Length; i++)
                        //    {
                        //        recordId.Add(splitIds[i].Trim());

                        //        tableName.Add("ad_ur");
                        //    }
                        //}
                    }
                }
                else
                {
                    success = false;

                    _logger.LogInformation("No Records Found");
                }

                _logger.LogInformation("Method Completed Successfully");

                return (success, recordId.ToArray(), tableName.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserPermissions Method Failed");
                throw;
            }
        }

        //peg
        public async Task<(bool success, string[] recordId, string[] tableName)> ExportPEG(string transID)
        {

            _logger.LogInformation("ExportPEG Started : {transID}", transID);
            // -- transid : ad_pm and --transid : pgv2m
            //select axprocessdefv2id,processname from axprocessdefv2 a where transid = 'slord'
            //select axpdef_peg_processmasterid from axpdef_peg_processmaster where caption = 'My PEG'
            //axprocessdefv2id,processname
            //axpdef_peg_processmasterid
            try
            {
                bool success = false;

                List<string> recordId = new List<string>();

                List<string> tableName = new List<string>();

                string sql = "SELECT axprocessdefv2id,processname,tasktype FROM axprocessdefv2 WHERE transid = @transID order by createdon asc";

                string[] paramNames = { "@transID" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { transID };

                List<string> processNames = new List<string>();

                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string ids = row["axprocessdefv2id"].ToString();

                        if (!string.IsNullOrWhiteSpace(ids))
                        {
                            recordId.Add(ids.Trim());

                            string tasktypefromquery = row["tasktype"].ToString().ToLower();
                            //tasktype
                            //make
                            if (tasktypefromquery.Equals("make"))
                                tableName.Add("pgv2m");
                            else
                                tableName.Add("pgv2a");

                            string processName = row["processname"].ToString();

                            if (!processNames.Contains(processName))
                            {
                                processNames.Add(processName);
                            }

                            //processNames.Add(row["processname"].ToString());
                        }
                    }
                }
                else
                {
                    success = false;

                    _logger.LogInformation("No Records Found");
                }

                if (success && processNames.Count > 0)
                {

                    // -- transid : ad_pm and --transid : pgv2m
                    //select axprocessdefv2id,processname from axprocessdefv2 a where transid = 'slord'
                    //select axpdef_peg_processmasterid from axpdef_peg_processmaster where caption = 'My PEG'
                    //axprocessdefv2id,processname
                    //axpdef_peg_processmasterid
                    //foreach (string processName in processNames)
                    //{
                    //    sql = "select axpdef_peg_processmasterid from axpdef_peg_processmaster where caption = @processName";

                    string processName = "'" + string.Join("','", processNames) + "'";

                    sql = $"select axpdef_peg_processmasterid from axpdef_peg_processmaster where caption in ({processName}) order by createdon asc";

                    //paramNames = new string[] { "@processName" };

                    //paramTypes = new DbType[] { DbType.String };

                    //paramValues = new object[] { processName };


                    DataTable resultSecondQuery = await _axDBService.ExecuteSelectQuery(sql, null, null, null);

                    if (resultSecondQuery.Rows.Count > 0)
                    {
                        success = true;

                        foreach (DataRow row in resultSecondQuery.Rows)
                        {
                            string ids = row["axpdef_peg_processmasterid"].ToString();

                            if (!string.IsNullOrWhiteSpace(ids))
                            {
                                recordId.Add(ids.Trim());
                                tableName.Add("ad_pm");
                            }
                        }
                    }
                    else
                    {
                        success = false;
                    }
                    //}
                }
                else
                {
                    success = false;
                }
                _logger.LogInformation("Method Completed Successfully");

                if (success)
                {
                    List<string> finalRecordId = new List<string>();
                    List<string> finalTableName = new List<string>();

                    for (int i = 0; i < tableName.Count; i++)
                    {
                        if (tableName[i] == "ad_pm")
                        {
                            finalRecordId.Add(recordId[i]);
                            finalTableName.Add(tableName[i]);
                        }
                    }

                    for (int i = 0; i < tableName.Count; i++)
                    {
                        if (tableName[i] == "pgv2m" || tableName[i] == "pgv2a")
                        {
                            finalRecordId.Add(recordId[i]);
                            finalTableName.Add(tableName[i]);
                        }
                    }
                    return (success, finalRecordId.ToArray(), finalTableName.ToArray());
                }

                return (success, recordId.ToArray(), tableName.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PEG Method Failed");
                throw;
            }
        }

        //printform
        public async Task<(bool success, bool flatQuery, string query)> ExportPrintForm(string transID)
        {

            _logger.LogInformation("ExportPrintForm Started : {transID}", transID);
            //flattable
            //select * from axfastlink where transid = 'slord'
            //select * from axpertreports where caption = 'temp1'

            try
            {
                bool success = false;

                bool flatQuery = true;

                List<string> insertQueries = new List<string>();


                //no order id here as there is no such column
                string sql = "SELECT * FROM axfastlink WHERE transid = @transID";

                string[] paramNames = { "@transID" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { transID };


                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);


                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string caption = row["caption"]?.ToString();

                        List<string> columns = new List<string>();

                        List<string> values = new List<string>();


                        foreach (DataColumn column in result.Columns)
                        {
                            string columnName = column.ColumnName;

                            string value = row[columnName]?.ToString()?.Replace("'", "''");

                            columns.Add(columnName);

                            values.Add(value == null ? "NULL" : $"'{value}'");
                        }


                        string insertQuery = $"INSERT INTO axfastlink ({string.Join(",", columns)}) VALUES ({string.Join(",", values)});";

                        insertQueries.Add(insertQuery);


                        if (!string.IsNullOrWhiteSpace(caption))
                        {
                            //no createdon such column in this table
                            sql = "SELECT * FROM axpertreports WHERE caption = @caption";

                            paramNames = new string[] { "@caption" };

                            paramTypes = new DbType[] { DbType.String };

                            paramValues = new object[] { caption };


                            DataTable secondResult = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);


                            if (secondResult.Rows.Count > 0)
                            {
                                foreach (DataRow secondRow in secondResult.Rows)
                                {
                                    columns = new List<string>();

                                    values = new List<string>();


                                    //foreach (DataColumn column in secondResult.Columns)
                                    //{
                                    //    string columnName = column.ColumnName;

                                    //    string value = secondRow[columnName]?.ToString()?.Replace("'", "''");

                                    //    columns.Add(columnName);

                                    //    values.Add(value == null ? "NULL" : $"'{value}'");
                                    //}
                                    foreach (DataColumn column in secondResult.Columns)
                                    {
                                        string columnName = column.ColumnName;

                                        object value = secondRow[columnName];

                                        columns.Add(columnName);

                                        if (value == null || value == DBNull.Value)
                                        {
                                            values.Add("NULL");
                                        }
                                        else if
                                        (
                                            value is byte ||
                                            value is sbyte ||
                                            value is short ||
                                            value is ushort ||
                                            value is int ||
                                            value is uint ||
                                            value is long ||
                                            value is ulong ||
                                            value is float ||
                                            value is double ||
                                            value is decimal
                                        )
                                        {
                                            values.Add(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                                        }
                                        else if (value is bool boolValue)
                                        {
                                            values.Add(boolValue ? "1" : "0");
                                        }
                                        else if (value is DateTime dateValue)
                                        {
                                            values.Add($"'{dateValue:yyyy-MM-dd HH:mm:ss.fff}'");
                                        }
                                        else if (value is DateTimeOffset dateTimeOffsetValue)
                                        {
                                            values.Add($"'{dateTimeOffsetValue:yyyy-MM-dd HH:mm:ss.fff zzz}'");
                                        }
                                        else if (value is Guid)
                                        {
                                            values.Add($"'{value}'");
                                        }
                                        else if (value is char charValue)
                                        {
                                            values.Add($"'{charValue.ToString().Replace("'", "''")}'");
                                        }
                                        else if (value is byte[])
                                        {
                                            byte[] bytes = (byte[])value;

                                            values.Add($"E'\\\\x{BitConverter.ToString(bytes).Replace("-", "")}'");
                                        }
                                        else if (value is string || value is Enum)
                                        {
                                            values.Add($"'{value.ToString().Replace("'", "''")}'");
                                        }
                                        else
                                        {
                                            values.Add($"'{value.ToString().Replace("'", "''")}'");
                                        }
                                    }


                                    insertQuery = $"INSERT INTO axpertreports ({string.Join(",", columns)}) VALUES ({string.Join(",", values)});";

                                    insertQueries.Add(insertQuery);
                                }
                            }
                            else
                            {
                                success = false;
                            }
                        }
                    }
                }
                else
                {
                    success = false;
                }


                _logger.LogInformation("Method Completed Successfully");

                return (success, flatQuery, string.Join(Environment.NewLine + QUERY_SEPARATOR, insertQueries));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrintForm Method Failed");
                throw;
            }
        }

        ///user roles
        public async Task<(bool success, string[] recordId, string[] tableName)> ExportUserRoles(string userGroup)
        {

            _logger.LogInformation("ExportUserRoles Started : {userGroup}", userGroup);
            //user roles --transid : ad_ur
            //select* from axpdef_userroles
            //axpdef_userrolesid,userGroup
            try
            {
                bool success = false;

                List<string> recordId = new List<string>();

                List<string> tableName = new List<string>();

                //string sql = "SELECT axpdef_userrolesid FROM axpdef_userroles WHERE username = @userGroup";
                string sql = "SELECT axpdef_userrolesid FROM axpdef_userroles WHERE axusergroup = @userGroup order by createdon asc";

                string[] paramNames = { "@userGroup" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { userGroup };

                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string ids = row["axpdef_userrolesid"].ToString();

                        if (!string.IsNullOrWhiteSpace(ids))
                        {
                            recordId.Add(ids.Trim());

                            tableName.Add("ad_ur");
                        }
                        //if (!string.IsNullOrWhiteSpace(ids))
                        //{
                        //    string[] splitIds = ids.Split(',');

                        //    for (int i = 0; i < splitIds.Length; i++)
                        //    {
                        //        recordId.Add(splitIds[i].Trim());

                        //        tableName.Add("ad_ur");
                        //    }
                        //}
                    }
                }
                else
                {
                    success = false;
                }

                _logger.LogInformation("Method Completed Successfully");

                return (success, recordId.ToArray(), tableName.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserRoles Method Failed");
                throw;
            }
        }

        /////user groups
        public async Task<(bool success, string[] recordId, string[] tableName)> ExportUserGroups(string userGroupName)
        {

            _logger.LogInformation("ExportUserGroups Started : {userGroupName}", userGroupName);
            //user groups --transid : a__ug
            //select* from axpdef_usergroups;
            //axpdef_usergroupsid,username
            try
            {
                bool success = false;

                List<string> recordId = new List<string>();

                List<string> tableName = new List<string>();

                string sql = "SELECT axpdef_usergroupsid FROM axpdef_usergroups WHERE users_group_name = @userGroupName order by createdon asc";

                string[] paramNames = { "@userGroupName" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { userGroupName };

                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string ids = row["axpdef_usergroupsid"].ToString();

                        if (!string.IsNullOrWhiteSpace(ids))
                        {
                            recordId.Add(ids.Trim());

                            tableName.Add("a__ug");
                        }

                        //if (!string.IsNullOrWhiteSpace(ids))
                        //{
                        //    string[] splitIds = ids.Split(',');

                        //    for (int i = 0; i < splitIds.Length; i++)
                        //    {
                        //        recordId.Add(splitIds[i].Trim());

                        //        tableName.Add("a__ug");
                        //    }
                        //}
                    }
                }
                else
                {
                    success = false;
                }

                _logger.LogInformation("Method Completed Successfully");

                return (success, recordId.ToArray(), tableName.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserGroups Method Failed");
                throw;
            }
        }


        //////////////custompage
        public async Task<(bool success, bool flatQuery, string[] recordId, string[] tableName, string query)> ExportCustomPages(string captionName)
        {

            _logger.LogInformation("ExportCustomPages Started : {captionName}", captionName);
            //custom page --transid : sect
            //select htmlsectionsid,pageno from HTMLSECTIONS where caption = 'test'
            //select * from axpages where name = 'HP' + pageno

            try
            {
                bool success = false;

                bool flatQuery = true;

                List<string> recordId = new List<string>();

                List<string> tableName = new List<string>();

                List<string> insertQueries = new List<string>();

                List<string> customPageFiles = new List<string>();


                string sql = "SELECT htmlsectionsid,pageno FROM HTMLSECTIONS WHERE caption = @captionName order by createdon asc";

                string[] paramNames = { "@captionName" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { captionName };


                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);


                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string ids = row["htmlsectionsid"].ToString();

                        string pageNo = row["pageno"]?.ToString();


                        if (!string.IsNullOrWhiteSpace(ids))
                        {
                            recordId.Add(ids.Trim());

                            tableName.Add("sect");
                        }


                        if (!string.IsNullOrWhiteSpace(pageNo))
                        {

                            customPageFiles.Add($"{captionName}_{pageNo}{CUSTOM_PAGE_DELIMITER}html");
                            
                            sql = "SELECT * FROM axpages WHERE name = @pageName order by createdon asc";

                            paramNames = new string[] { "@pageName" };

                            paramTypes = new DbType[] { DbType.String };

                            paramValues = new object[] { "HP" + pageNo };


                            DataTable secondResult = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);


                            if (secondResult.Rows.Count > 0)
                            {
                                foreach (DataRow secondRow in secondResult.Rows)
                                {
                                    List<string> columns = new List<string>();

                                    List<string> values = new List<string>();


                                    //foreach (DataColumn column in secondResult.Columns)
                                    //{
                                    //    string columnName = column.ColumnName;

                                    //    string value = secondRow[columnName]?.ToString()?.Replace("'", "''");

                                    //    columns.Add(columnName);

                                    //    values.Add(value == null ? "NULL" : $"'{value}'");
                                    //}
                                    foreach (DataColumn column in secondResult.Columns)
                                    {
                                        string columnName = column.ColumnName;

                                        object value = secondRow[columnName];

                                        columns.Add(columnName);

                                        if (columnName.Equals("ordno", StringComparison.OrdinalIgnoreCase))
                                        {
                                            values.Add("(SELECT COALESCE(MAX(ordno),0) + 1 FROM axpages)");
                                            continue;
                                        }

                                        if (columnName.Equals("levelno", StringComparison.OrdinalIgnoreCase))
                                        {
                                            values.Add("0");
                                            continue;
                                        }

                                        if (value == null || value == DBNull.Value)
                                        {
                                            values.Add("NULL");
                                        }
                                        else if
                                        (
                                            value is byte ||
                                            value is sbyte ||
                                            value is short ||
                                            value is ushort ||
                                            value is int ||
                                            value is uint ||
                                            value is long ||
                                            value is ulong ||
                                            value is float ||
                                            value is double ||
                                            value is decimal
                                        )
                                        {
                                            values.Add(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                                        }
                                        else if (value is bool boolValue)
                                        {
                                            values.Add(boolValue ? "1" : "0");
                                        }
                                        else if (value is DateTime dateValue)
                                        {
                                            values.Add($"'{dateValue:yyyy-MM-dd HH:mm:ss.fff}'");
                                        }
                                        else if (value is DateTimeOffset dateTimeOffsetValue)
                                        {
                                            values.Add($"'{dateTimeOffsetValue:yyyy-MM-dd HH:mm:ss.fff zzz}'");
                                        }
                                        else if (value is Guid)
                                        {
                                            values.Add($"'{value}'");
                                        }
                                        else if (value is char charValue)
                                        {
                                            values.Add($"'{charValue.ToString().Replace("'", "''")}'");
                                        }
                                        else if (value is byte[])
                                        {
                                            byte[] bytes = (byte[])value;

                                            values.Add($"E'\\\\x{BitConverter.ToString(bytes).Replace("-", "")}'");
                                        }
                                        else if (value is string || value is Enum)
                                        {
                                            values.Add($"'{value.ToString().Replace("'", "''")}'");
                                        }
                                        else
                                        {
                                            values.Add($"'{value.ToString().Replace("'", "''")}'");
                                        }
                                    }


                                    string insertQuery = $"INSERT INTO axpages ({string.Join(",", columns)}) VALUES ({string.Join(",", values)});";

                                    insertQueries.Add(insertQuery);
                                }
                            }
                            else
                            {
                                success = false;
                            }


                            ////file query
                            ///

                            sql = "SELECT filename,filetype FROM sect4 WHERE htmlsectionsid = @htmlsectionsid";


                            paramNames = new string[] { "@htmlsectionsid" };

                            paramTypes = new DbType[] { DbType.Decimal };

                            paramValues = new object[] { Convert.ToDecimal(ids) };

                            DataTable sect4Result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

                            foreach (DataRow sect4Row in sect4Result.Rows)
                            {
                                string fileName = sect4Row["filename"]?.ToString();

                                string fileType = sect4Row["filetype"]?.ToString();

                                if (string.IsNullOrWhiteSpace(fileName))
                                    continue;

                                customPageFiles.Add($"{fileName}_{pageNo}{CUSTOM_PAGE_DELIMITER}{fileType?.ToLower()}");
                            }

                            //////
                        }
                    }
                }
                else
                {
                    success = false;
                }

                if (customPageFiles.Count > 0)
                {
                    if (!_customPageFiles.ContainsKey(captionName))
                        _customPageFiles.Add(captionName, customPageFiles);
                    else
                        _customPageFiles[captionName].AddRange(customPageFiles);
                }


                return
                (
                    success,
                    flatQuery,
                    recordId.ToArray(),
                    tableName.ToArray(),
                    string.Join(QUERY_SEPARATOR, insertQueries)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CustomPage Method Failed");
                throw;
            }
        }


        ////DeveloperOptions
        public async Task<(bool success, string[] recordId, string[] tableName)> ExportDeveloperOptions(string developerOptionName)
        {

            _logger.LogInformation("ExportDeveloperOptions Started : {developerOptionName}", developerOptionName);
            //developer options --transid : axstc & astcp
            //select* from axpstructconfig;
            //select* from axpstructconfigprops; --may not be needed
            try
            {
                bool success = false;

                List<string> recordId = new List<string>();

                List<string> tableName = new List<string>();

                string sql = "SELECT axpstructconfigid FROM axpstructconfig WHERE asprops  = @developerOptionName order by createdon asc";

                string[] paramNames = { "@developerOptionName" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { developerOptionName };

                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string ids = row["axpstructconfigid"].ToString();

                        if (!string.IsNullOrWhiteSpace(ids))
                        {
                            recordId.Add(ids.Trim());

                            tableName.Add("axstc");


                            //needs to be discussed

                            //recordId.Add(ids.Trim());

                            //tableName.Add("astcp");
                        }

                        //if (!string.IsNullOrWhiteSpace(ids))
                        //{
                        //    string[] splitIds = ids.Split(',');

                        //    for (int i = 0; i < splitIds.Length; i++)
                        //    {
                        //        recordId.Add(splitIds[i].Trim());

                        //        tableName.Add("a__ug");
                        //    }
                        //}
                    }
                }
                else
                {
                    success = false;
                }

                _logger.LogInformation("Method Completed Successfully");

                return (success, recordId.ToArray(), tableName.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeveloperOptions Method Failed");
                throw;
            }
        }


        ///axvars

        public async Task<(bool success, string[] recordId, string[] tableName)> ExportAxVars(string axvarName)
        {

            _logger.LogInformation("ExportAxVars Started : {axvarName}", axvarName);

            //SELECT axp_vpid AS id,'axvar' AS transid FROM axp_vp WHERE vpname = 'branch'
            //UNION ALL
            //SELECT axpdef_axvarsid AS id,'ad_db' AS transid FROM axpdef_axvars WHERE db_function = 'branch';

            try
            {
                bool success = false;

                List<string> recordId = new List<string>();

                List<string> tableName = new List<string>();

                string sql = "SELECT axp_vpid AS id,'axvar' AS transid FROM axp_vp WHERE vpname = @axvarName UNION ALL SELECT axpdef_axvarsid AS id,'ad_db' AS transid FROM axpdef_axvars WHERE db_function = @axvarName";

                string[] paramNames = { "@axvarName" };

                DbType[] paramTypes = { DbType.String };

                object[] paramValues = { axvarName };

                _logger.LogInformation("Executing Query");

                DataTable result = await _axDBService.ExecuteSelectQuery(sql, paramNames, paramTypes, paramValues);

                if (result.Rows.Count > 0)
                {
                    success = true;

                    foreach (DataRow row in result.Rows)
                    {
                        string id = row[0]?.ToString();
                        string tblName = row[1]?.ToString();

                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            recordId.Add(id.Trim());

                            tableName.Add(tblName.Trim());


                            //needs to be discussed

                            //recordId.Add(ids.Trim());

                            //tableName.Add("astcp");
                        }

                        //if (!string.IsNullOrWhiteSpace(ids))
                        //{
                        //    string[] splitIds = ids.Split(',');

                        //    for (int i = 0; i < splitIds.Length; i++)
                        //    {
                        //        recordId.Add(splitIds[i].Trim());

                        //        tableName.Add("a__ug");
                        //    }
                        //}
                    }
                }
                else
                {
                    success = false;
                }

                _logger.LogInformation("Method Completed Successfully");

                return (success, recordId.ToArray(), tableName.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AxVars Method Failed");
                throw;
            }
        }

    }
}