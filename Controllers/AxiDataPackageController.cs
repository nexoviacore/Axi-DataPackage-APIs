using AxiDataPackages.DTO;
using AxiDataPackages.Services;
using AxiDataPackages.Services.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AxiDataPackages.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AxiDataPackageController : ControllerBase
    {
        private readonly ExportPackageService _ExportPackageService;

        private readonly ImportPackageService _importPackageService;

        private readonly ILogger<AxiDataPackageController> _logger;

        private readonly EncryptDecrypt _encryptDecrypt;

        private readonly IConfiguration _configuration;

        private readonly CustomPageFileCP _customPageFileCP;

        public AxiDataPackageController
        (
            ExportPackageService ExportPackageService,
            ILogger<AxiDataPackageController> logger,
            ImportPackageService importPackageService,
            EncryptDecrypt encryptDecrypt,
            IConfiguration configuration,
            CustomPageFileCP customPageFileCP

        )
        {
            _ExportPackageService = ExportPackageService;

            _logger = logger;

            _importPackageService = importPackageService;

            _encryptDecrypt = encryptDecrypt;

            _configuration = configuration;

            _customPageFileCP = customPageFileCP;
        }

        [HttpPost("ExportData")]
        public async Task<IActionResult> ExportPackage
        (
            ExportPackageDTO payload
        )
        {
            try
            {
                _logger.LogInformation("================================================");

                _logger.LogInformation("AxiDataPackageController Started");

                _logger.LogDebug("Current Input Payload  : {result}",JsonConvert.SerializeObject(payload,Formatting.Indented));


                if (string.IsNullOrWhiteSpace(payload.ExportDir))
                {
                    _logger.LogError("Export Directory Validation Failed");

                    return BadRequest(new
                    {
                        success = false,
                        message = "Export Package Failed.",
                        errormessage = "Export directory is required"
                    });
                }

                string path = payload.ExportDir.Replace("\\\\", "\\");


                try
                {
                    if (!Path.IsPathRooted(path))
                    {
                        _logger.LogError("Invalid Export Directory Path");

                        return BadRequest(new
                        {
                            success = false,
                            message = "Export Package Failed.",
                            errormessage = "Invalid export directory path"
                        });
                    }

                    if (!Directory.Exists(path))
                    {
                        _logger.LogInformation("Directory Not Exists Creating Directory : {path}", path);

                        Directory.CreateDirectory(path);

                        _logger.LogInformation("Directory Created Successfully");
                    }

                    string testFilePath = Path.Combine(path, "test_access.tmp");

                    _logger.LogInformation("Checking Directory Write Access");

                    System.IO.File.WriteAllText(testFilePath, "test");

                    System.IO.File.Delete(testFilePath);

                    _logger.LogInformation("Directory Write Access Success");
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Directory Access Denied");

                    return BadRequest(new
                    {
                        success = false,
                        message = "Export Package Failed.",
                        errormessage = "No write permission for export directory"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Export Directory Validation Failed");

                    return BadRequest(new
                    {
                        success = false,
                        message = "Export Package Failed.",
                        errormessage = ex.Message
                    });
                }

                string axpertWebUrl = _configuration["AxpertURL:AxpertWebURL"];

                try
                {
                    string appPath = Path.Combine(axpertWebUrl, payload.AppName);

                    if (string.IsNullOrWhiteSpace(axpertWebUrl))
                    {
                        throw new Exception("AxpertWebURL configuration is missing");
                    }

                    if (!Directory.Exists(appPath))
                    {
                        throw new Exception($"Application directory not found : {appPath}");
                    }

                    Directory.GetFiles(appPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "AxpertWebURL Access Denied");

                    return BadRequest(new
                    {
                        success = false,
                        message = "Export Package Failed.",
                        errormessage = "No read permission for AxpertWebURL application directory"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AxpertWebURL Validation Failed");

                    return BadRequest(new
                    {
                        success = false,
                        message = "Export Package Failed.",
                        errormessage = ex.Message
                    });
                }

                //sample payload//
                //{
                //    "requestId": "PKG-20260513-001",
                //    "appName": "pgbase114",
                //    "packageName": "sales_package",
                //    "packageVersion": "1.0.0",
                //    "requestedBy": "admin",
                //     "password" : "22723bbd4217a0abf6d3e68073c7603d",
                //    "exportDir": "D:\\AxiPackages\\",
                //    "objects": {
                //        "tstruct": [
                //            "slord",
                //            "puord",
                //            "cumst"
                //         ],
                //        "iview": [
                //            "slreport",
                //            "invreport"
                //        ],
                //        "custompage": [
                //            "customer_dashboard"
                //        ],
                //        "userrole": [
                //            "sales_manager",
                //            "purchase_manager"
                //        ],
                //        "usergroup": [
                //            "sales_team"
                //        ],
                //        "developeroption": [
                //            "enable_debug_mode"
                //        ],
                //        "uiplugin": [
                //            "custom_chart_plugin"
                //        ]
                //    }
                //}

                _logger.LogInformation("Calling ExportPackage Service");

                var result = await _ExportPackageService.ExportPackage(payload);

                _logger.LogInformation("ExportPackage Service Completed");

                dynamic response = result;

               if (response.success && response.hasData)
                    {
                    try
                    {
                        string filePath = Path.Combine(path, payload.PackageName+"_"+payload.PackageVersion + ".axidata");

                        _logger.LogInformation("Creating Export File : {filePath}", filePath);

                        if (!System.IO.File.Exists(filePath))
                        {
                            using (System.IO.File.Create(filePath))
                            {
                            }

                            _logger.LogInformation("Export File Created Successfully");
                        }

                        //string finalResult = JsonConvert.SerializeObject(result,Formatting.Indented);

                        //_logger.LogInformation("Final Export Result : {result}",finalResult);

                        //System.IO.File.WriteAllText(filePath,finalResult);

                        //_logger.LogInformation("Export Json Written Successfully");

                        string finalResult = JsonConvert.SerializeObject(result, Formatting.Indented);

                        _logger.LogInformation("Final Export Result : {result}", finalResult);

                        string encryptedResult = _encryptDecrypt.EncryptImplementation(finalResult);

                        _logger.LogInformation("Export Result Encrypted Successfully");

                        System.IO.File.WriteAllText(filePath, encryptedResult);

                        _logger.LogInformation("Encrypted Export Json Written Successfully");

                        ///file copy process
                        ///

                        var customPageResult = await _customPageFileCP.CopyCustomPageFiles(payload.AppName, path);

                        if (!customPageResult.success)
                        {
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);

                                _logger.LogInformation("Export File Deleted Due To Custom Page Copy Failure");
                            }

                            return BadRequest(new
                            {
                                success = false,
                                message = "Export Package Failed.",
                                errormessage = customPageResult.errorMessage
                            });
                        }

                        /////
                        ///
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Export File Write Failed");

                        return BadRequest(new
                        {
                            success = false,
                            message = "Export Package Failed.",
                            errormessage = "Failed to write export file : " + ex.Message
                        });
                    }
                }
                else
                {
                    _logger.LogInformation("No Export Data Found. File Creation Skipped");
                }

                _logger.LogInformation("Returning Success Response");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AxiDataPackageController Failed");

                return BadRequest(new
                {
                    success = false,
                    message = "Export Package Failed.",
                    errormessage = ex.Message
                });
            }
            finally
            {
                _logger.LogInformation("AxiDataPackageController Ended");

                _logger.LogInformation("================================================");
            }
        }


        [HttpPost("ImportData")]
        public async Task<IActionResult> ImportPackage(ImportPackageDTO payload)
        {
            try
            {
                _logger.LogInformation("================================================");

                _logger.LogInformation("AxiDataPackageController Started");

                _logger.LogDebug("Current Input Payload : {result}", JsonConvert.SerializeObject(payload, Formatting.Indented));

                _logger.LogInformation("Calling ImportPackage Service");

                var result = await _importPackageService.ImportPackage(payload);

                _logger.LogInformation("ImportPackage Service Completed");

                _logger.LogInformation("Returning Success Response");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AxiDataPackageController Failed");

                return BadRequest(new
                {
                    success = false,
                    message = "Import Package Failed.",
                    errormessage = ex.Message
                });
            }
            finally
            {
                _logger.LogInformation("AxiDataPackageController Ended");

                _logger.LogInformation("================================================");
            }
        }
    }
}
