using AxiDataPackages.References;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AxiDataPackages.Services.Helpers
{
    public class ImportPackageHelperService
    {
        private readonly ILogger<ImportPackageHelperService> _logger;

        private readonly AxARMApiHelperService _axARMApiHelperService;

        private readonly AxDBService _axDBService;

        private readonly EncryptDecrypt _encryptDecrypt;

        private readonly IConfiguration _configuration;

        public static List<string> customPagesFiles = new List<string>();

        private readonly CustomPageFileCP _customPageFileCP;

        public ImportPackageHelperService
        (
            ILogger<ImportPackageHelperService> logger,
            AxARMApiHelperService axARMApiHelperService,
            AxDBService axDBService,
            EncryptDecrypt encryptDecrypt,
            CustomPageFileCP customPageFileCP,
            IConfiguration configuration

        )
        {
            _logger = logger;
            _axARMApiHelperService = axARMApiHelperService;
            _axDBService = axDBService;
            _encryptDecrypt = encryptDecrypt;
            _configuration = configuration;
            _customPageFileCP = customPageFileCP;
        }

        public async Task<(bool success, JObject payload)> PreparePayloads(string importDir,string fileName)
        {
            try
            {
                _logger.LogInformation("PreparePayloads Started");

                string filePath = Path.Combine(importDir, fileName);

                _logger.LogInformation("Reading File : {filePath}", filePath);

                //string fileContent = await File.ReadAllTextAsync(filePath);

                //if (string.IsNullOrWhiteSpace(fileContent))
                //{
                //    _logger.LogError("Import File Is Empty");

                //    throw new Exception("Import file is empty");
                //}

                //JObject jsonData = JObject.Parse(fileContent);

                string encryptedContent = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(encryptedContent))
                {
                    _logger.LogError("Import File Is Empty");

                    throw new Exception("Import file is empty");
                }

                string fileContent = _encryptDecrypt.DecryptImplementation(encryptedContent);

                JObject jsonData = JObject.Parse(fileContent);


                JObject? payloadObject = jsonData["resultpayload"] as JObject;

                if (payloadObject == null)
                {
                    _logger.LogError("Payload Section Missing");

                    throw new Exception("Payload section not found");
                }

                JObject? allPayloads = payloadObject["axputpayloads"] as JObject;

                if (allPayloads == null)
                {
                    _logger.LogError("AxPutPayload Section is empty");

                    await _axDBService.CloseDbConnectionInsert(true);

                    //throw new Exception("AllPayloads section not found");
                    return (false,allPayloads);
                }
                else
                {
                    _logger.LogInformation("AxPutPayload Retrieved Successfully");

                    return (true, allPayloads);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PreparePayloads Failed : {message}", ex.Message);

                await _axDBService.CloseDbConnectionInsert(false);

                await _customPageFileCP.DeleteFiles(customPagesFiles);

                throw;
            }
        }

        public async Task<(bool success, string message)> ExecuteAxPut(string appName, string userName, string password, JObject payload)
        {
            try
            {
                _logger.LogInformation("ExecuteAxPut Started");

                var result = await _axARMApiHelperService.AxPutCall(appName, userName, password, payload);

                _logger.LogInformation("ExecuteAxPut Completed");

                if (result.success)
                {
                    await _axDBService.CloseDbConnectionInsert(true);
                }
                else
                {
                    await _axDBService.CloseDbConnectionInsert(false);

                    await _customPageFileCP.DeleteFiles(customPagesFiles);
                }

                    return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteAxPut Failed : {message}", ex.Message);

                await _axDBService.CloseDbConnectionInsert(false);

                await _customPageFileCP.DeleteFiles(customPagesFiles);

                throw;
            }
        }

        public async Task<(bool success, string message, List<string> queries)> PrepareDMLScripts(string importDir, string fileName)
        {
            try
            {
                _logger.LogInformation("PrepareDMLScripts Started");

                string filePath = Path.Combine(importDir, fileName);

                _logger.LogInformation("Reading File : {filePath}", filePath);

                //string fileContent = await File.ReadAllTextAsync(filePath);

                //if (string.IsNullOrWhiteSpace(fileContent))
                //{
                //    _logger.LogError("Import File Is Empty");

                //    throw new Exception("Import file is empty");
                //}

                //JObject jsonData = JObject.Parse(fileContent);

                string encryptedContent = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(encryptedContent))
                {
                    _logger.LogError("Import File Is Empty");

                    throw new Exception("Import file is empty");
                }

                string fileContent = _encryptDecrypt.DecryptImplementation(encryptedContent);

                JObject jsonData = JObject.Parse(fileContent);

                JObject? payloadObject = jsonData["resultpayload"] as JObject;

                if (payloadObject == null)
                {
                    _logger.LogError("ResultPayload Section Missing");

                    throw new Exception("ResultPayload section not found");
                }

                JArray? dmlScripts = payloadObject["dmlscripts"] as JArray;

                if (dmlScripts == null || dmlScripts.Count == 0)
                {
                    _logger.LogInformation("No DML Queries Found");

                    return (false, "No DML Queries Found", new List<string>());
                }

                List<string> queries = new List<string>();

                dynamic dbdetails =await _axDBService.GetDbDetails();

                var connectionField = dbdetails.GetType().GetField(
                        "_connection",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                var connection = connectionField?.GetValue(dbdetails);

                var userNameProperty = connection?.GetType().GetProperty("UserName");

                string schemaNameFromDb = userNameProperty?.GetValue(connection)?.ToString();

                //var schemaNameFromDbDetails = dbdetails._connection.UserName.ToString();

                foreach (string script in dmlScripts)
                {
                    string[] splitQueries = script.Split("$D#", StringSplitOptions.RemoveEmptyEntries);

                    foreach (string query in splitQueries)
                    {
                        string finalQuery = query.Trim();

                        finalQuery = finalQuery.Replace("{{schemaName}}", schemaNameFromDb);

                        _logger.LogDebug("Adding PreparedQuerytoList : {result}",finalQuery);

                        if (!string.IsNullOrWhiteSpace(finalQuery))
                        {
                            queries.Add(finalQuery);
                        }
                    }
                }

                _logger.LogInformation("DML Queries Prepared Successfully. Count : {count}", queries.Count);

                _logger.LogDebug("Prepared DML Queries : {result}", JsonConvert.SerializeObject(queries, Formatting.Indented));

                return (true, "DML Queries Prepared Successfully", queries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrepareDMLScripts Failed : {message}", ex.Message);
                throw;
            }
        }

        public async Task<(bool success, List<string> results)> ExecuteDMLScripts(List<string> queries)
        {
            try
            {
                _logger.LogInformation("ExecuteDMLScripts Started");

                var result = await _axDBService.ExecuteInsertOrCreateAsync(queries);

                _logger.LogInformation("ExecuteDMLScripts Completed");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteDMLScripts Failed : {message}", ex.Message);
                throw;
            }
        }

        public async Task<(bool success, string message, List<string> queries)> PrepareDDLScripts(string importDir, string fileName)
        {
            try
            {
                _logger.LogInformation("PrepareDDLScripts Started");

                string filePath = Path.Combine(importDir, fileName);

                _logger.LogInformation("Reading File : {filePath}", filePath);

                //string fileContent = await File.ReadAllTextAsync(filePath);

                //if (string.IsNullOrWhiteSpace(fileContent))
                //{
                //    _logger.LogError("Import File Is Empty");

                //    throw new Exception("Import file is empty");
                //}

                //JObject jsonData = JObject.Parse(fileContent);

                string encryptedContent = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(encryptedContent))
                {
                    _logger.LogError("Import File Is Empty");

                    throw new Exception("Import file is empty");
                }

                string fileContent = _encryptDecrypt.DecryptImplementation(encryptedContent);

                JObject jsonData = JObject.Parse(fileContent);

                JObject? payloadObject = jsonData["resultpayload"] as JObject;

                if (payloadObject == null)
                {
                    _logger.LogError("ResultPayload Section Missing");

                    throw new Exception("ResultPayload section not found");
                }

                JArray? ddlScripts = payloadObject["ddlscritps"] as JArray;

                if (ddlScripts == null || ddlScripts.Count == 0)
                {
                    _logger.LogInformation("No DDL Queries Found");

                    return (false, "No DDL Queries Found", new List<string>());
                }

                List<string> queries = new List<string>();

                dynamic dbdetails =await _axDBService.GetDbDetails();

                var connectionField = dbdetails.GetType().GetField(
                        "_connection",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                var connection = connectionField?.GetValue(dbdetails);

                var userNameProperty = connection?.GetType().GetProperty("UserName");

                string schemaNameFromDb = userNameProperty?.GetValue(connection)?.ToString();

                //var schemaNameFromDbDetails = dbdetails._connection.UserName.ToString();

                foreach (string script in ddlScripts)
                {
                    string[] splitQueries = script.Split("$D#", StringSplitOptions.RemoveEmptyEntries);

                    foreach (string query in splitQueries)
                    {
                        string finalQuery = query.Trim();

                        finalQuery = finalQuery.Replace("{{schemaName}}", schemaNameFromDb);

                        _logger.LogDebug("Adding PreparedQuerytoList : {result}", finalQuery);

                        if (!string.IsNullOrWhiteSpace(finalQuery))
                        {
                            queries.Add(finalQuery);
                        }
                    }
                }

                _logger.LogInformation("DDL Queries Prepared Successfully. Count : {count}", queries.Count);

                _logger.LogDebug("Prepared DDL Queries : {result}", JsonConvert.SerializeObject(queries, Formatting.Indented));

                return (true, "DDL Queries Prepared Successfully", queries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrepareDDLScripts Failed : {message}", ex.Message);
                throw;
            }
        }
        public async Task<(bool success, List<string> results)> ExecuteDDLScripts(List<string> queries)
        {
            try
            {
                _logger.LogInformation("ExecuteDDLScripts Started");

                var result = await _axDBService.ExecuteInsertOrCreateAsync(queries);

                _logger.LogInformation("ExecuteDDLScripts Completed");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteDDLScripts Failed : {message}", ex.Message);
                throw;
            }
        }

        public async Task<(bool success, List<string> results)> ExecuteScripts(List<string> queries)
        {
            try
            {
                _logger.LogInformation("ExecuteScripts Started");


                // await _axDBService.dummyMethod();

                await _axDBService.CreateDbConnectionInsert();

                var result = await _axDBService.ExecuteInsertOrCreateAsync(queries);

                _logger.LogInformation("ExecuteScripts Completed");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteScripts Failed : {message}", ex.Message);
                await _axDBService.CloseDbConnectionInsert(false);
                throw;
            }
        }

        public async Task<(bool success, string errorMessage)> TargetCustomPagesFileCP(string appName,string importDir)
        {
            try
            {
                _logger.LogInformation("TargetCustomPagesFileCP Started");

                string axpertWebUrl = _configuration["AxpertURL:AxpertWebURL"];

                string sourceHtmlPath = Path.Combine(importDir, "CustomPages", "HTMLPages");

                string targetHtmlPath = Path.Combine(axpertWebUrl, appName, "HTMLPages");

                if (!Directory.Exists(targetHtmlPath))
                {
                    Directory.CreateDirectory(targetHtmlPath);

                    _logger.LogInformation("HTMLPages Directory Created : {path}", targetHtmlPath);
                }

                foreach (string file in Directory.GetFiles(sourceHtmlPath))
                {
                    string targetFile = Path.Combine(targetHtmlPath, Path.GetFileName(file));

                    System.IO.File.Copy(file, targetFile, true);

                    customPagesFiles.Add(targetFile);

                    _logger.LogInformation("HTML File Copied : {file}", targetFile);
                }

                string sourceCssPath = Path.Combine(sourceHtmlPath, "css");

                if (Directory.Exists(sourceCssPath))
                {
                    string targetCssPath = Path.Combine(targetHtmlPath, "css");

                    if (!Directory.Exists(targetCssPath))
                    {
                        Directory.CreateDirectory(targetCssPath);
                    }

                    foreach (string file in Directory.GetFiles(sourceCssPath))
                    {
                        string targetFile = Path.Combine(targetCssPath, Path.GetFileName(file));

                        System.IO.File.Copy(file, targetFile, true);

                        customPagesFiles.Add(targetFile);

                        _logger.LogInformation("CSS File Copied : {file}", targetFile);
                    }
                }

                string sourceJsPath = Path.Combine(sourceHtmlPath, "js");

                if (Directory.Exists(sourceJsPath))
                {
                    string targetJsPath = Path.Combine(targetHtmlPath, "js");

                    if (!Directory.Exists(targetJsPath))
                    {
                        Directory.CreateDirectory(targetJsPath);
                    }

                    foreach (string file in Directory.GetFiles(sourceJsPath))
                    {
                        string targetFile = Path.Combine(targetJsPath, Path.GetFileName(file));

                        System.IO.File.Copy(file, targetFile, true);

                        customPagesFiles.Add(targetFile);

                        _logger.LogInformation("JS File Copied : {file}", targetFile);
                    }
                }

                _logger.LogInformation("TargetCustomPagesFileCP Completed");

                return (true, "Files Imported Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TargetCustomPagesFileCP Failed");

                await _axDBService.CloseDbConnectionInsert(false);

                await _customPageFileCP.DeleteFiles(customPagesFiles);

                return (false, ex.Message);
            }
        }
    }
}