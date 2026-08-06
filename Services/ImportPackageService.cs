using ARMCommon.Model;
using AxiDataPackages.DTO;
using AxiDataPackages.References;
using AxiDataPackages.Services.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using static ARMCommon.Helpers.Constants;

namespace AxiDataPackages.Services
{
    public class ImportPackageService
    {
        private readonly ILogger<ImportPackageService> _logger;

        private readonly ImportPackageHelperService _importPackageHelperService;

        private readonly AxDBService _axDBService;

        private readonly IConfiguration _configuration;


        public ImportPackageService
        (
            ILogger<ImportPackageService> logger,
            ImportPackageHelperService importPackageHelperService,
            AxDBService axDBService,IConfiguration  configuration
        )
        {
            _logger = logger;
            _importPackageHelperService = importPackageHelperService;
            _axDBService = axDBService;
            _configuration = configuration;
        }

        public async Task<object> ImportPackage(ImportPackageDTO payload)
        {
            try
            {
                _logger.LogInformation("================================================");
                _logger.LogInformation("ImportPackage Service Started");

                _logger.LogDebug("Current Input Payload : {result}", JsonConvert.SerializeObject(payload, Formatting.Indented));

                _logger.LogInformation("Validating AppName");

                if (string.IsNullOrWhiteSpace(payload.AppName))
                {
                    _logger.LogError("Validation Failed - AppName is required");

                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "AppName is required"
                    };
                }

                _logger.LogInformation("Validating RequestedBy");

                if (string.IsNullOrWhiteSpace(payload.RequestedBy))
                {
                    _logger.LogError("Validation Failed - RequestedBy is required");

                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "RequestedBy is required"
                    };
                }

                _logger.LogInformation("Validating Password");

                if (string.IsNullOrWhiteSpace(payload.Password))
                {
                    _logger.LogError("Validation Failed - Password is required");

                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "Password is required"
                    };
                }

                _logger.LogInformation("Validating PackageName");

                if (string.IsNullOrWhiteSpace(payload.PackageName))
                {
                    _logger.LogError("Validation Failed - PackageName is required");

                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "PackageName is required"
                    };
                }

                _logger.LogInformation("Validating PackageVersion");

                if (string.IsNullOrWhiteSpace(payload.PackageVersion))
                {
                    _logger.LogError("Validation Failed - PackageVersion is required");

                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "PackageVersion is required"
                    };
                }

                _logger.LogInformation("Validating ImportDir");

                if (string.IsNullOrWhiteSpace(payload.ImportDir))
                {
                    _logger.LogError("Validation Failed - ImportDir is required");

                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "ImportDir is required"
                    };
                }

                string appName = payload.AppName;
                string fileName = payload.PackageName + "_" + payload.PackageVersion + ".axidata";
                string password = payload.Password;
                string importDir = payload.ImportDir;
                string userName = payload.RequestedBy;
                string axpertWebURL = _configuration["AxpertURL:AxpertWebURL"];

                // _logger.LogInformation("Input Values Stored Successfully");

                string path = importDir.Replace("\\\\", "\\");

                try
                {
                    if (!Path.IsPathRooted(path))
                    {
                        _logger.LogError("Invalid Import Directory Path");

                        return new
                        {
                            success = false,
                            message = "Import Package Failed",
                            errormessage = "Invalid import directory path"
                        };
                    }

                    if (!Directory.Exists(path))
                    {
                        _logger.LogError("Import Directory Does Not Exist : {path}", path);

                        return new
                        {
                            success = false,
                            message = "Import Package Failed",
                            errormessage = "Import directory does not exist"
                        };
                    }

                    string filePathCheck = Path.Combine(path, fileName);

                    _logger.LogInformation("Checking Package File Exists : {filePath}", filePathCheck);

                    if (!File.Exists(filePathCheck))
                    {
                        _logger.LogError("Package File Not Found : {filePath}", filePathCheck);

                        return new
                        {
                            success = false,
                            message = "Import Package Failed",
                            errormessage = "Package file not found"
                        };
                    }

                    _logger.LogInformation("Checking Package File Read Access");

                    using (FileStream stream = File.Open(filePathCheck, FileMode.Open, FileAccess.Read))
                    {
                    }

                    _logger.LogInformation("Package File Read Access Success");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Package File Access Denied");

                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "No read permission for package file"
                    };
                }

                try
                {
                    if (string.IsNullOrWhiteSpace(axpertWebURL))
                    {
                        return new
                        {
                            success = false,
                            message = "Import Package Failed",
                            errormessage = "AxpertWebURL is empty"
                        };
                    }

                    if (!Directory.Exists(axpertWebURL))
                    {
                        return new
                        {
                            success = false,
                            message = "Import Package Failed",
                            errormessage = "AxpertWebURL path does not exist"
                        };
                    }

                    string testFile = Path.Combine(axpertWebURL, Guid.NewGuid() + ".tmp");

                    using (File.Create(testFile))
                    {
                    }

                    System.IO.File.Delete(testFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AxpertWebURL Write Access Validation Failed");

                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "No write permission for AxpertWebURL path"
                    };
                }

                string appNameForImport = payload.AppName;

                string userNameForImport = payload.RequestedBy;

                string userPassword = payload.Password;


                _logger.LogInformation("Connecting DB : {appName}", appName);

                bool isConnected = await _axDBService.ConnectDB(appName);

                _logger.LogInformation("DB Connection Result : {result}", isConnected);

                if (!isConnected)
                {
                    return new
                    {
                        success = false,
                        message = "Import Package Failed",
                        errormessage = "DB Connection Failed"
                    };
                }

                string filePath = Path.Combine(path, fileName);

                ///flatqueries

                List<string> sqlCommandsDMLndDDL = new List<string>();

                var PrepareDMLScriptsPayloadData = await _importPackageHelperService.PrepareDMLScripts(importDir, filePath);

                bool prepareDMLScriptssucesscheck = false;

                string prepareDMlScritpsMessage = string.Empty;

                bool dmlScripts = false;

                if (PrepareDMLScriptsPayloadData.success)
                {
                    var prepareDMLScriptsresultMessage = PrepareDMLScriptsPayloadData.queries;

                    _logger.LogDebug("PrepareDML Result : {result}", JsonConvert.SerializeObject(prepareDMLScriptsresultMessage, Formatting.Indented));

                    //var FinalResultExecuteDML = await _importPackageHelperService.ExecuteDMLScripts(prepareDMLScriptsresultMessage);

                    //prepareDMLScriptssucesscheck = FinalResultExecuteDML.success;

                    ////prepareDMlScritpsMessage = string.Join(Environment.NewLine, FinalResultExecuteDML.results);

                    //_logger.LogDebug("Execute DML Scripts result :  {Result}", JsonConvert.SerializeObject(FinalResultExecuteDML.results, Formatting.Indented));

                    //prepareDMlScritpsMessage = $"Executed {FinalResultExecuteDML.results.Count} DML queries successfully";

                    dmlScripts = true;


                    foreach (string query in prepareDMLScriptsresultMessage)
                    {
                        sqlCommandsDMLndDDL.Add(query);
                    }

                    _logger.LogInformation
                    (
                        "Added {count} DML Queries To SQLCommandBuilder List. Total Commands Count : {totalCount}",
                        prepareDMLScriptsresultMessage.Count,
                        sqlCommandsDMLndDDL.Count
                    );

                    prepareDMlScritpsMessage = prepareDMLScriptsresultMessage.Count.ToString();

                    //_logger.LogInformation(prepareDMLScriptsresultMessage);
                }
                else
                {
                    prepareDMLScriptssucesscheck = false;
                    prepareDMlScritpsMessage = PrepareDMLScriptsPayloadData.message;
                }

                /// DDL Queries

                var PrepareDDLScriptsPayloadData = await _importPackageHelperService.PrepareDDLScripts(importDir, filePath);

                bool prepareDDLScriptssucesscheck = false;

                string prepareDDlScritpsMessage = string.Empty;

                bool ddlscripts = false;

                if (PrepareDDLScriptsPayloadData.success)
                {
                    ddlscripts = true;

                    var prepareDDLScriptsresultMessage = PrepareDDLScriptsPayloadData.queries;

                    _logger.LogDebug("PrepareDDL Result : {result}", JsonConvert.SerializeObject(prepareDDLScriptsresultMessage, Formatting.Indented));

                    //var FinalResultExecuteDDL = await _importPackageHelperService.ExecuteDDLScripts(prepareDDLScriptsresultMessage);

                    //prepareDDLScriptssucesscheck = FinalResultExecuteDDL.success;

                    //_logger.LogDebug("Execute DDL Scripts result :  {Result}",JsonConvert.SerializeObject(FinalResultExecuteDDL.results, Formatting.Indented));

                    //prepareDDlScritpsMessage = $"Executed {FinalResultExecuteDDL.results.Count} DDL queries successfully";

                    foreach (string query in prepareDDLScriptsresultMessage)
                    {
                        sqlCommandsDMLndDDL.Add(query);
                    }

                    _logger.LogInformation
                    (
                        "Added {count} DDL Queries To SQLCommandBuilder List. Total Commands Count : {totalCount}",
                        prepareDDLScriptsresultMessage.Count,
                        sqlCommandsDMLndDDL.Count
                    );

                    prepareDDlScritpsMessage = prepareDDLScriptsresultMessage.Count.ToString();
                }
                else
                {
                    prepareDDLScriptssucesscheck = false;

                    prepareDDlScritpsMessage = PrepareDDLScriptsPayloadData.message;
                }

                //return new
                //{
                //    success = axputsuccessCheck,
                //    message = axputsuccessCheck ? "Import Package Success" : "Import Package Failed",
                //    resultMessage = axputresultMessage
                //};

                //Final query executions
                string finalExecuteResults = null;
                bool finalExecuteResultsSuccessCheck = false;

                if (dmlScripts || ddlscripts)
                {
                    _logger.LogInformation("Executing DML and DDL Scripts. Total Queries Count : {count}", sqlCommandsDMLndDDL.Count);

                    var finalResult = await _importPackageHelperService.ExecuteScripts(sqlCommandsDMLndDDL);

                    finalExecuteResultsSuccessCheck = finalResult.success;

                    _logger.LogDebug("ExecuteScripts Result : {result}", JsonConvert.SerializeObject(finalResult.results, Formatting.Indented));

                    finalExecuteResults = string.Join(Environment.NewLine, finalResult.results);
                }
                else
                {
                    _logger.LogInformation("No DML or DDL Scripts were executed.");
                }

                /////Import File Process
                string customPagesPath = Path.Combine(importDir, "CustomPages");

                if (!Directory.Exists(customPagesPath))
                {
                    _logger.LogInformation("No CustomPages Folder Found. HTML File Copy Skipped.");
                }
                else
                {
                    _logger.LogInformation("CustomPages Folder Found. Starting HTML File Copy.");

                    var customPageResult =
                        await _importPackageHelperService.TargetCustomPagesFileCP(appName, importDir);

                    if (!customPageResult.success)
                    {
                        throw new Exception(customPageResult.errorMessage);
                    }

                    _logger.LogInformation("CustomPages Files Copied Successfully");
                }


                ///axput-allpayloads

                var preparePayloadData = await _importPackageHelperService.PreparePayloads(importDir, filePath);

                bool axputsuccessCheck = false;

                string axputresultMessage = string.Empty;

                if (preparePayloadData.success)
                {
                    string prepareresultMessage = preparePayloadData.payload.ToString();

                    _logger.LogDebug("PreparePayload Result : {result}", prepareresultMessage);

                    JObject resultPayload = preparePayloadData.payload;

                    var FinalResultAxPut = await _importPackageHelperService.ExecuteAxPut(appName, userName, password, resultPayload);

                    axputsuccessCheck = FinalResultAxPut.success;

                    axputresultMessage = FinalResultAxPut.message;
                }
                else
                {
                    axputsuccessCheck = false;
                    axputresultMessage = "AllPayloads Section is empty";
                }                

                _logger.LogInformation("Import Package Completed Successfully");

                _logger.LogDebug("AxPut Result : {result} \n DMLScripts Result : \n {result1} \n DDLScripts Result : \n{result2}",axputresultMessage,prepareDMlScritpsMessage,prepareDDlScritpsMessage);
                

                var result = new Dictionary<string, string>();

                if (axputsuccessCheck)
                    result.Add("AxPutPayloads", axputresultMessage);

                if (finalExecuteResultsSuccessCheck)
                {
                    if (dmlScripts)
                        result.Add("DMLScripts", $"Imported {prepareDMlScritpsMessage} DML Queries Successfully");

                    if (ddlscripts)
                        result.Add("DDLScripts", $"Imported {prepareDDlScritpsMessage} DDL Queries Successfully");

                    result.Add("TotalImportedQueries", finalExecuteResults);
                }

                //if (prepareDMLScriptssucesscheck)
                //    result.Add("DMLScripts", prepareDMlScritpsMessage);

                //if (prepareDDLScriptssucesscheck)
                //    result.Add("DDLScripts", prepareDDlScritpsMessage);

                _logger.LogDebug("Import Package Result : {result}",
                    JsonConvert.SerializeObject(result, Formatting.Indented));

                return new
                {
                    success = true,
                    message = "Import Package Completed Successfully",
                    result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportPackage Service Failed");

                return new
                {
                    success = false,
                    message = "Import Package Failed",
                    errormessage = ex.Message
                };
            }
            finally
            {
                _logger.LogInformation("Closing DB Connection");
                 await _axDBService.CloseConnection();
                _logger.LogInformation("ImportPackage Service Ended");
                _logger.LogInformation("================================================");
            }
        }
    }
}