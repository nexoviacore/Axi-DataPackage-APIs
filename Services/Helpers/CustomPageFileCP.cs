using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AxiDataPackages.Services.Helpers
{
    public class CustomPageFileCP
    {
        private readonly ILogger<CustomPageFileCP> _logger;

        private readonly IConfiguration _configuration;

        public CustomPageFileCP
        (
            ILogger<CustomPageFileCP> logger,
            IConfiguration configuration
        )
        {
            _logger = logger;

            _configuration = configuration;
        }

        public async Task<(bool success, string errorMessage)> CopyCustomPageFiles
        (
            string appName,
            string exportDir
        )
        {
            try
            {
                _logger.LogInformation("CustomPageFileCP Started");

                string axpertWebUrl = _configuration["AxpertURL:AxpertWebURL"];

                string sourceHtmlPath = Path.Combine(axpertWebUrl, appName, "HTMLPages");

                string targetHtmlPath = Path.Combine(exportDir, "CustomPages", "HTMLPages");


                if (PackageHelperService._customPageFiles.Count > 0)
                {

                    _logger.LogInformation("CustomPageFileCP Started - The Dict Has data");

                    foreach (var item in PackageHelperService._customPageFiles)
                    {
                        foreach (string fileDetail in item.Value)
                        {
                            string[] split = fileDetail.Split("$D#$");

                            if (split.Length != 2)
                            {
                                continue;
                            }

                            string fileName = split[0];

                            string fileType = split[1].ToLower();

                            string sourceFile = string.Empty;

                            string targetFile = string.Empty;

                            if (fileType == "html")
                            {
                                if (!Directory.Exists(targetHtmlPath))
                                {
                                    Directory.CreateDirectory(targetHtmlPath);
                                }

                                sourceFile = Path.Combine(sourceHtmlPath, fileName + ".html");

                                targetFile = Path.Combine(targetHtmlPath, fileName + ".html");
                            }
                            else if (fileType == "css")
                            {
                                string targetCssPath = Path.Combine(targetHtmlPath, "css");

                                if (!Directory.Exists(targetCssPath))
                                {
                                    Directory.CreateDirectory(targetCssPath);
                                }

                                sourceFile = Path.Combine(sourceHtmlPath, "css", fileName + ".css");

                                targetFile = Path.Combine(targetCssPath, fileName + ".css");
                            }
                            else if (fileType == "js")
                            {
                                string targetJsPath = Path.Combine(targetHtmlPath, "js");

                                if (!Directory.Exists(targetJsPath))
                                {
                                    Directory.CreateDirectory(targetJsPath);
                                }

                                sourceFile = Path.Combine(sourceHtmlPath, "js", fileName + ".js");

                                targetFile = Path.Combine(targetJsPath, fileName + ".js");
                            }

                            if (string.IsNullOrWhiteSpace(sourceFile))
                            {
                                throw new Exception($"Source file path is empty for {fileName}.{fileType}");
                            }

                            if (!System.IO.File.Exists(sourceFile))
                            {
                                throw new Exception($"Custom page file not found : {sourceFile}");
                            }

                            System.IO.File.Copy(sourceFile, targetFile, true);

                            _logger.LogInformation("Custom Page File Copied : {file}", targetFile);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("CustomPageFileCP - The Dict is Empty so no Custom Pages Copy Occur.");
                }



                    _logger.LogInformation("CustomPageFileCP Completed");

                PackageHelperService._customPageFiles.Clear();

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CustomPageFileCP Failed");

                try
                {
                    string customPagePath = Path.Combine(exportDir, "CustomPages");

                    if (Directory.Exists(customPagePath))
                    {
                        Directory.Delete(customPagePath, true);

                        _logger.LogInformation("CustomPages Directory Deleted");
                    }
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(deleteEx, "CustomPages Cleanup Failed");
                }

                return (false, ex.Message);
            }
        }


        public async Task DeleteFiles(List<string> filePaths)
        {
            try
            {
                _logger.LogInformation("DeleteFiles Started");


                if (filePaths.Count > 0)
                {

                    foreach (string filePath in filePaths)
                    {
                        if (string.IsNullOrWhiteSpace(filePath))
                        {
                            continue;
                        }

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);

                            _logger.LogInformation("File Deleted : {filePath}", filePath);
                        }
                        else
                        {
                            _logger.LogInformation("File Not Found : {filePath}", filePath);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Custom File Delete - The list is empty the Deletion Part is Skipped");
                }

                _logger.LogInformation("DeleteFiles Completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteFiles Failed");
                throw;
            }
        }
    }
}