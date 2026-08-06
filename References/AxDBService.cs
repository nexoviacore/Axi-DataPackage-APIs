using ARMCommon.Interface;
using ARMCommon.Model;
using AxExtend.Interface;
using AxiDataPackages.Services.Helpers;
using Microsoft.Extensions.Logging;
using System.Data;
using static ARMCommon.Helpers.Constants;

namespace AxiDataPackages.References
{
    public class AxDBService
    {
        private readonly IAxExtend _axExtend;

        private readonly ILogger<AxDBService> _logger;

        private ARMCommon.Interface.IDbHelper _insertDb;

        private readonly CustomPageFileCP _customPageFileCP;

        public AxDBService(IAxExtend axExtend,ILogger<AxDBService> logger,CustomPageFileCP customPageFileCP)
        {
            _axExtend = axExtend;

            _logger = logger;

            _customPageFileCP = customPageFileCP;


        }

        public async Task<bool> ConnectDB(string appName)
        {
            try
            {
                _logger.LogInformation("ConnectDB Started : {appName}", appName);

                bool result = await _axExtend.OpenDBConnectionAsync(appName);

                _logger.LogInformation("ConnectDB Result : {result}", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConnectDB Failed");

                throw;
            }
        }

        public async Task CreateDbConnectionInsert()
        {
            _logger.LogInformation("CreateDbConnectionInsert Started");

            if (_insertDb != null)
            {
               await _insertDb.CloseDBConnectionAsync();
            }

            _insertDb = await _axExtend.GetDB();

            _logger.LogInformation("CreateDbConnectionInsert Completed");
        }


        public async Task<object> GetDbDetails()
        {
            var dbdetails =await _axExtend.GetDB();
            return dbdetails;
        }

        public async Task<DataTable> ExecuteSelectQuery(string sql, string[] paramNames, DbType[] paramTypes, object[] paramValues)
        {

            try
            {
                _logger.LogInformation("ExecuteSelectQuery Started");

                _logger.LogInformation("Getting DB Instance");

                var db = await _axExtend.GetDB();


                _logger.LogInformation("Executing SQL Query");

                //Console.WriteLine(db.GetType().FullName);

                //var result = await db.ExecuteSQLAsync(sql, paramNames, paramTypes, paramValues);
                
                var result = await db.ExecuteSQLAsync(sql, paramNames, paramTypes, paramValues);

                _logger.LogInformation("SQL Query Execution Completed");

                var temp = result;

                if (string.IsNullOrEmpty(result.error))
                {
                    _logger.LogInformation("ExecuteSelectQuery Completed Successfully");

                    return result.data;
                }

                _logger.LogError("SQL Query Failed : {error}", result.error);

                throw new Exception(result.error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteSelectQuery Failed");

                throw;
            }
            finally
            {
                if (_insertDb != null)
                {
                    await _insertDb.CloseDBConnectionAsync();
                }
            }
        }

        public async Task CloseDbConnectionInsert(bool commit)
        {
            try
            {
                _logger.LogInformation("CloseDbConnectionInsert Started");

                if (_insertDb != null)
                {
                    if (commit)
                    {
                        _insertDb.CommitTransaction();

                        _logger.LogInformation("Transaction Committed");
                    }
                    else
                    {
                        _insertDb.RollBackTransaction();

                        _logger.LogInformation("Transaction Rolled Back");
                    }

                    await _insertDb.CloseDBConnectionAsync();

                    _insertDb = null;
                }

                _logger.LogInformation("CloseDbConnectionInsert Completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CloseDbConnectionInsert Failed");
                _logger.LogError(ex, "Here we can't rollback AxPut transaction but files and queries can be rollbacked");

                await _customPageFileCP.DeleteFiles(ImportPackageHelperService.customPagesFiles);
                throw;
            }
        }

        //public async Task<(bool success, List<string> results)> ExecuteInsertOrCreateAsync(List<string> queries)
        //{
        //    try
        //    {
        //        _logger.LogInformation("ExecuteInsertOrCreateAsync Started");

        //        List<string> results = new List<string>();

        //        bool success = true;

        //        _logger.LogInformation("Getting DB Instance");

        //        var db = await _axExtend.GetDB();


        //        foreach (string query in queries)
        //        {
        //            try
        //            {
        //                _logger.LogInformation("Executing Query");

        //                string[] paramNames = { };

        //                DbType[] paramTypes = { };

        //                object[] paramValues = { };

        //                var affectedRows = await db.ExecuteNonQueryAsync(query, paramNames, paramTypes, paramValues);

        //                if (string.IsNullOrEmpty(affectedRows.error))
        //                {
        //                    string result = "Success : Affected Rows = " + affectedRows.count;

        //                    results.Add(result);

        //                    _logger.LogInformation(result);
        //                }
        //                else
        //                {
        //                    //success = false;

        //                    string result = "Failed : " + affectedRows.error;

        //                    //results.Add(result);

        //                    _logger.LogError(result);

        //                    throw new Exception(result);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //success = false;

        //                string result = "Failed : " + ex.Message;

        //               // results.Add(result);

        //                _logger.LogError(ex, "Query Execution Failed : {message}", ex.Message);

        //                throw;
        //            }
        //        }

        //        _logger.LogInformation("ExecuteInsertOrCreateAsync Completed");

        //        return (success, results);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "ExecuteInsertOrCreateAsync Failed : {message}", ex.Message);

        //        throw;
        //    }
        //}
        public async Task<(bool success, List<string> results)> ExecuteInsertOrCreateAsync(List<string> queries)
        {
            try
            {
                _logger.LogInformation("ExecuteInsertOrCreateAsync Started");

                List<string> results = new List<string>();

                bool success = true;

                _logger.LogInformation("Getting DB Instance");

                //var db = await _axExtend.GetDB();
                //_insertDb = await CreateDbConnectionInsert();

                if(_insertDb == null)
                {
                    _logger.LogInformation("DB Connection for Insert is null");

                    await CreateDbConnectionInsert();
                }

                try
                {
                    List<SQLCommandBuilder> sqlCommands = new List<SQLCommandBuilder>();

                    foreach (string query in queries)
                    {
                        _logger.LogInformation("Adding Query To Transaction : {query}", query);

                        sqlCommands.Add
                        (
                            new SQLCommandBuilder
                            (
                                query,
                                new string[] { },
                                new DbType[] { },
                                new object[] { },
                                DATA_ACTION.INSERT
                            )
                        );
                    }

                    _logger.LogInformation("Executing Queries In Transaction");

                    //var transactionResult = await db.ExecuteSQLsInTransactionAsync(sqlCommands);
                    var transactionResult = await _insertDb.ExecuteSQLsInTransactionAsync(sqlCommands,false);

                    if (transactionResult.success)
                    {
                        string result =
                            "Success : Total Statements executed = " +
                            transactionResult.TotalStatements;

                        results.Add(result);

                        _logger.LogInformation(result);
                    }
                    else
                    {
                        string result = "Failed : " + transactionResult.error;

                        _logger.LogError(result);

                        throw new Exception(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Query Execution Failed : {message}", ex.Message);

                    throw;
                }

                _logger.LogInformation("ExecuteInsertOrCreateAsync Completed");

                return (success, results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteInsertOrCreateAsync Failed : {message}", ex.Message);

                throw;
            }
        }

        public async Task CloseConnection()
       {
            try
            {
                _logger.LogInformation("CloseConnection Started");

                await _axExtend.CloseDBConnectionAsync();

                _logger.LogInformation("CloseConnection Completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CloseConnection Failed");

                throw;
            }
        }

        //public async Task dummyMethod()
        //{
        //    var db = await _axExtend.GetDB();

        //    List<SQLCommandBuilder> cmds = new();

        //    cmds.Add
        //    (
        //        new SQLCommandBuilder
        //        (
        //            "insert into testtransactiondum(id,name) values(999,'test')",
        //            new string[] { },
        //            new DbType[] { },
        //            new object[] { },
        //            DATA_ACTION.INSERT
        //        )
        //    );

        //    var result = await db.ExecuteSQLsInTransactionAsync(cmds, false);

        //    Console.WriteLine(result.success);

    

        //    //db.RollBackTransaction();

        //    db.CommitTransaction();

        //    await db.CloseDBConnectionAsync();

        //    await _axExtend.CloseDBConnectionAsync();

        //    //var db1 = await _axExtend.GetDB();

        //    //List<SQLCommandBuilder> cmds1 = new();

        //    //cmds1.Add
        //    //(
        //    //    new SQLCommandBuilder
        //    //    (
        //    //        "insert into testtransactiondum(id,name) values(555,'test')",
        //    //        new string[] { },
        //    //        new DbType[] { },
        //    //        new object[] { },
        //    //        DATA_ACTION.INSERT
        //    //    )
        //    //);

        //    //var result1 = await db1.ExecuteSQLsInTransactionAsync(cmds1, false);

        //    //Console.WriteLine(result1.success);

        //    //// db.CommitTransaction();

        //    //db1.CommitTransaction();

        //    //db.CommitTransaction();
        //}
    }
}
