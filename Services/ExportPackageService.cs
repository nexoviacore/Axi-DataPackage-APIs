using ARMCommon.Interface;
using AxiDataPackages.DTO;
using AxiDataPackages.References;
using AxiDataPackages.Services.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AxiDataPackages.Services
{
    public class ExportPackageService
    {
        private readonly AxDBService _axDBService;

        private readonly PackageHelperService _packageHelperService;

        private readonly AxARMApiHelperService _axARMApiHelperService;

        private readonly AxPutConversion _axPutConversion;

        private readonly ILogger<ExportPackageService> _logger;

        private readonly PostgresScriptsLogic _postgresScriptsLogic;

        public ExportPackageService
        (
            AxDBService axDBService,
            PackageHelperService packageHelperService, AxARMApiHelperService axARMApiHelperService,
            AxPutConversion axPutConversion,
            ILogger<ExportPackageService> logger, PostgresScriptsLogic postgresScriptsLogic

        )
        {
            _axDBService = axDBService;

            _packageHelperService = packageHelperService;

            _axARMApiHelperService = axARMApiHelperService;

            _axPutConversion = axPutConversion;

            _logger = logger;

            _postgresScriptsLogic = postgresScriptsLogic;


        }

        //public async Task<object> ExportPackage(ExportPackageDTO payload)
        //{
        //    try
        //    {
        //        //_aRMLogTraceService.EnableTracing(true);
        //        //if (string.IsNullOrWhiteSpace(payload.TransId))
        //        //{
        //        //    return new
        //        //    {
        //        //        success = false,
        //        //        message = "TransId is required"
        //        //    };
        //        //}

        //        if (string.IsNullOrWhiteSpace(payload.ProjectName))
        //        {
        //            return new
        //            {
        //                success = false,
        //                message = "ProjectName is required"
        //            };
        //        }

        //        if (string.IsNullOrWhiteSpace(payload.UserName))
        //        {
        //            return new
        //            {
        //                success = false,
        //                message = "UserName is required"
        //            };
        //        }

        //        if (string.IsNullOrWhiteSpace(payload.Password))
        //        {
        //            return new
        //            {
        //                success = false,
        //                message = "Password is required"
        //            };
        //        }

        //        string appName = payload.ProjectName;
        //        string userName = payload.UserName;
        //        string userPassword = payload.Password;


        //        bool isConnected = await _axDBService.ConnectDB(appName);

        //        if (!isConnected)
        //        {
        //            return new
        //            {
        //                success = false,
        //                message = "DB Connection Failed"
        //            };
        //        }

        //        //var data = await _packageHelperService.ExportPrintForm(payload.TransId);

        //        //string[] recordId = data.recordId;
        //        //string[] tableName = data.tableName;

        //        //for (int i = 0; i < recordId.Length; i++)
        //        //{
        //        //    var AxGetdata = await _axARMApiHelperService.GetAxData(appName, tableName, recordId, "recordid", userName, userPassword);
        //        //}

        //        //return new
        //        //{
        //        //    success = true,
        //        //    message = "Data fetched successfully",
        //        //    payload = AxGetdata
        //        //};

        //        List<object> notificationPayloads = new List<object>();


        //        string[] recordId;

        //        string[] tableName;

        //        bool sucessCheck = false;

        //        try
        //        {
        //            var data = await _packageHelperService.ExportNotification(payload.TransId);

        //            sucessCheck = data.success;

        //            recordId = data.recordId;

        //            tableName = data.tableName;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception($"ExportNotification failed - {ex.Message}");
        //        }


        //        if (sucessCheck)
        //        {
        //            for (int i = 0; i < recordId.Length; i++)
        //            {
        //                try
        //                {
        //                    string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[i], recordId[i], "recordid", userName, userPassword);

        //                    notificationPayloads.Add(JsonConvert.DeserializeObject(axGetData));


        //                    //var jsonObject = JObject.Parse(axGetData);

        //                    //notificationPayloads.Add(jsonObject);

        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception($"GetAxData failed for table : {tableName[i]} and recordId : {recordId[i]} - {ex.Message}");
        //                }
        //            }
        //        }


        //        //List<object> customPagePayloads =new List<object>();


        //        //userroles
        //        List<object> userRolesPayloads =new List<object>();

        //        sucessCheck = false;

        //        try
        //        {
        //            var data = await _packageHelperService.ExportUserRoles(payload.UserName);

        //            sucessCheck = data.success;

        //            recordId = data.recordId;

        //            tableName = data.tableName;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception($"ExportUserRoles failed - {ex.Message}");
        //        }


        //        if (sucessCheck)
        //        {
        //            for (int i = 0; i < recordId.Length; i++)
        //            {
        //                try
        //                {
        //                    string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[i], recordId[i], "recordid", userName, userPassword);

        //                    userRolesPayloads.Add(JsonConvert.DeserializeObject(axGetData));

        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception($"GetAxData failed for table : {tableName[i]} and recordId : {recordId[i]} - {ex.Message}");
        //                }
        //            }
        //        }


        //        //usergrps
        //        List<object> userGroupsPayloads =new List<object>();

        //        sucessCheck = false;

        //        try
        //        {
        //            var data = await _packageHelperService.ExportUserGroups(payload.UserName);

        //            sucessCheck = data.success;

        //            recordId = data.recordId;

        //            tableName = data.tableName;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception($"ExportUserGroups failed - {ex.Message}");
        //        }


        //        if (sucessCheck)
        //        {
        //            for (int i = 0; i < recordId.Length; i++)
        //            {
        //                try
        //                {
        //                    string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[i], recordId[i], "recordid", userName, userPassword);

        //                    userGroupsPayloads.Add(JsonConvert.DeserializeObject(axGetData));

        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception($"GetAxData failed for table : {tableName[i]} and recordId : {recordId[i]} - {ex.Message}");
        //                }
        //            }
        //        }

        //        //List<object> permissionsPayloads =new List<object>();

        //        //List<object> pegPayloads =new List<object>();

        //        //List<object> printFormPayloads =new List<object>();

        //        //List<object> developerOptionsPayloads = new List<object>();

        //        //List<object> uiPluginPayloads =new List<object>();


        //        return new
        //        {
        //            success = true,

        //            message = "Data fetched successfully",

        //            payload = new
        //            {

        //                //customPage = customPagePayloads,

        //                userRoles = userRolesPayloads,

        //                userGroups = userGroupsPayloads,

        //                //permissions = permissionsPayloads,

        //                //peg = pegPayloads,


        //                notification = notificationPayloads,


        //                //printForm = printFormPayloads

        //                /*
        //                developerOptions = developerOptionsPayloads,

        //                uiPlugin = uiPluginPayloads
        //                */
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new
        //        {
        //            success = false,
        //            message = ex.Message
        //        };
        //    }
        //    finally
        //    {
        //        await _axDBService.CloseConnection();
        //    }
        //}
        public async Task<object> ExportPackage(ExportPackageDTO payload)
        {
            try
            {
                _logger.LogInformation("================================================");
                _logger.LogInformation("ExportPackage Service Started");
                _logger.LogDebug("Current Input Payload  : {result}", JsonConvert.SerializeObject(payload, Formatting.Indented));
                _logger.LogInformation("Validating AppName");

                if (string.IsNullOrWhiteSpace(payload.AppName))
                {
                    _logger.LogError("Validation Failed - AppName is required");

                    return new
                    {
                        success = false,
                        message = "Export Package Failed",
                        errormessage = "AppName is required"
                    };
                }

                if (string.IsNullOrWhiteSpace(payload.RequestedBy))
                {
                    _logger.LogError("Validation Failed - RequestedBy is required");

                    return new
                    {
                        success = false,
                        message = "Export Package Failed",
                        errormessage = "RequestedBy is required"
                    };
                }

                if (string.IsNullOrWhiteSpace(payload.Password))
                {
                    _logger.LogError("Validation Failed - Password is required");

                    return new
                    {
                        success = false,
                        message = "Export Package Failed",
                        errormessage = "Password is required"
                    };
                }

                if (payload.Objects == null)
                {
                    _logger.LogError("Validation Failed - Objects section is empty");

                    return new
                    {
                        success = false,
                        message = "Export Package Failed",
                        errormessage = "Objects section is empty"
                    };
                }

                if (payload.Objects == null || (payload.Objects.tstruct == null || payload.Objects.tstruct.Length == 0) &&
                //(payload.Objects.iview == null || payload.Objects.iview.Length == 0) &&
                (payload.Objects.cards == null || payload.Objects.cards.Length == 0) &&
                (payload.Objects.custompage == null || payload.Objects.custompage.Length == 0) &&
                (payload.Objects.userrole == null || payload.Objects.userrole.Length == 0) &&
                (payload.Objects.usergroup == null || payload.Objects.usergroup.Length == 0) &&
                (payload.Objects.developeroption == null || payload.Objects.developeroption.Length == 0) &&
                //(payload.Objects.uiplugin == null || payload.Objects.uiplugin.Length == 0) &&
                (payload.Objects.axvars == null || payload.Objects.axvars.Length == 0) &&
                (payload.Objects.ads == null || payload.Objects.ads.Length == 0) &&
                (payload.Objects.tables == null || payload.Objects.tables.Length == 0) &&
                (payload.Objects.views == null || payload.Objects.views.Length == 0) &&
                 (payload.Objects.materializedview == null || payload.Objects.materializedview.Length == 0) &&
                (payload.Objects.functions == null || payload.Objects.functions.Length == 0) &&
                (payload.Objects.constraint == null || payload.Objects.constraint.Length == 0) &&
                (payload.Objects.index == null || payload.Objects.index.Length == 0) &&
                (payload.Objects.triggers == null || payload.Objects.triggers.Length == 0) &&
                (payload.Objects.sequence == null || payload.Objects.sequence.Length == 0))
                //&&(payload.Objects.procedures == null || payload.Objects.procedures.Length == 0)))
                {
                    _logger.LogInformation("Objects Data Missing");

                    return new
                    {
                        success = false,
                        message = "Export Package Failed.",
                        errormessage = "Objects Data Missing : At least one object is required"
                    };
                }

                string appName = payload.AppName;

                string userName = payload.RequestedBy;

                string userPassword = payload.Password;


                _logger.LogInformation("Connecting DB : {appName}", appName);

                bool isConnected = await _axDBService.ConnectDB(appName);

                _logger.LogInformation("DB Connection Result : {result}", isConnected);

                if (!isConnected)
                {
                    return new
                    {
                        success = false,
                        message = "Export Package Failed",
                        errormessage = "DB Connection Failed"
                    };
                }

                //  "objects": {
                //      "tstruct": [
                //        "autst"
                //      ],
                //  "iview": [],
                //  "custompage": [],
                //  "ads": [],
                //  "userrole": [],
                //  "tables": [],
                //  "functions": [],
                //  "triggers": [],
                //  "index": [],
                //  "sequence": [],
                //  "constraint": []
                //},


                //string[] objectTypes =
                //{
                //    "tstruct",//"iview",
                //    "cards","custompage","userrole","usergroup",
                //    "developeroption",/*"uiplugin",*/"axvars","ads",
                //    "sequence","tables","constraint",
                //    "index","functions",//"procedures",
                //    "triggers","views","materializedview"
                //};


                string[] objectTypes =
                {
                    "axvars","ads","tstruct",//"iview",
                    "cards","custompage","userrole","usergroup",
                    "developeroption",//"uiplugin",
                    "sequence","tables","constraint",
                    "index","functions",//"procedures",
                    "triggers","views","materializedview"
                };


                List<object> AllPayloads = new List<object>();

                List<object> flatQueryPayloads = new List<object>();

                List<object> postgresDBObjects = new List<object>();

                List<object> separatePayloads = new List<object>();


                string[] recordId;

                string[] tableName;

                bool successCheck = false;



                _logger.LogInformation("Checking Payload Dependencies");

                _logger.LogDebug("Payload Before Dependency Check : {payload}",
                    JsonConvert.SerializeObject(payload, Formatting.Indented));

                payload = await PayloadDependencyCheck(payload);

                _logger.LogDebug("Payload After Dependency Check : {payload}",
                                JsonConvert.SerializeObject(payload, Formatting.Indented));

                _logger.LogInformation("Payload Dependency Check Completed");


                foreach (string objectType in objectTypes)
                {
                    _logger.LogInformation("Processing Object Type : {objectType}", objectType);
                    string[] objectValues = null;

                    List<object> objectWisePayloads = new List<object>();


                    switch (objectType.ToLower())
                    {
                        case "tstruct":

                            objectValues = payload.Objects.tstruct;

                            break;

                        case "iview":

                            objectValues = payload.Objects.iview;

                            break;


                        case "materializedview":

                            objectValues = payload.Objects.materializedview;

                            break;


                        case "cards":

                            objectValues = payload.Objects.cards;

                            break;

                        case "custompage":

                            objectValues = payload.Objects.custompage;

                            break;

                        case "userrole":

                            objectValues = payload.Objects.userrole;

                            break;

                        case "usergroup":

                            objectValues = payload.Objects.usergroup;

                            break;

                        case "developeroption":

                            objectValues = payload.Objects.developeroption;

                            break;

                        case "uiplugin":

                            objectValues = payload.Objects.uiplugin;

                            break;

                        case "axvars":

                            objectValues = payload.Objects.axvars;

                            break;

                        case "ads":

                            objectValues = payload.Objects.ads;

                            break;

                        case "tables":

                            objectValues = payload.Objects.tables;

                            break;

                        case "views":

                            objectValues = payload.Objects.views;

                            break;

                        case "functions":

                            objectValues = payload.Objects.functions;

                            break;

                        case "triggers":

                            objectValues = payload.Objects.triggers;

                            break;

                        case "procedures":

                            objectValues = payload.Objects.procedures;

                            break;

                        case "index":

                            objectValues = payload.Objects.index;

                            break;


                        case "sequence":

                            objectValues = payload.Objects.sequence;

                            break;


                        case "constraint":

                            objectValues = payload.Objects.constraint;

                            break;


                    }


                    if (objectValues == null || objectValues.Length == 0)
                    {
                        _logger.LogInformation("No Objects Found For Type : {objectType}", objectType);
                        continue;
                    }


                    for (int i = 0; i < objectValues.Length; i++)
                    {
                        _logger.LogInformation("Processing Object : {objectName}", objectValues[i]);
                        successCheck = false;

                        List<object> currentPayloads = new List<object>();


                        try
                        {
                            switch (objectType)
                            {
                                case "tstruct":

                                    //notification
                                    _logger.LogInformation("Calling ExportNotification");

                                    var notificationData = await _packageHelperService.ExportNotification(objectValues[i]);

                                    _logger.LogInformation("ExportNotification Completed");

                                    successCheck = notificationData.success;

                                    recordId = notificationData.recordId;

                                    tableName = notificationData.tableName;

                                    _logger.LogDebug("Export Notification for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(notificationData, Formatting.Indented));



                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {
                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet, objectValues[i],true);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }


                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            //catch (Exception ex)
                                            //{
                                            //    throw new Exception($"GetAxData failed for table : {tableName[j]} and recordId : {recordId[j]} - {ex.Message}");
                                            //}
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    //permission
                                    _logger.LogInformation("Calling ExportUserPermissions");

                                    var permissionData = await _packageHelperService.ExportUserPermissions(objectValues[i]);

                                    _logger.LogInformation("ExportUserPermissions Completed");

                                    _logger.LogDebug("Export Permission for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(permissionData, Formatting.Indented));

                                    successCheck = permissionData.success;

                                    recordId = permissionData.recordId;

                                    tableName = permissionData.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {
                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {
                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet, objectValues[i],true);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }


                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    //peg
                                    _logger.LogInformation("Calling ExportPEG");

                                    var pegData = await _packageHelperService.ExportPEG(objectValues[i]);

                                    _logger.LogInformation("ExportPEG Completed");

                                    _logger.LogDebug("Export PEG for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(pegData, Formatting.Indented));

                                    successCheck = pegData.success;

                                    recordId = pegData.recordId;

                                    tableName = pegData.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {
                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet, objectValues[i], true);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }


                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                            }
                                        }
                                    }


                                    //printform
                                    //try
                                    //{
                                    //    _logger.LogInformation("Calling ExportPrintForm");

                                    //    var printFormData = await _packageHelperService.ExportPrintForm(objectValues[i]);

                                    //    _logger.LogInformation("ExportPrintForm Completed");

                                    //    _logger.LogDebug("Export PrintForm for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(printFormData, Formatting.Indented));

                                    //    successCheck = printFormData.success;

                                    //    //recordId = printFormData.recordId;

                                    //    //tableName = printFormData.tableName;


                                    //    if (successCheck && printFormData.flatQuery)
                                    //    {
                                    //        _logger.LogInformation("Adding Flat Query Payload");

                                    //        currentPayloads.Add(printFormData.query);
                                    //        flatQueryPayloads.Add(printFormData.query);
                                    //    }

                                    //}
                                    //catch (Exception ex)
                                    //{
                                    //    throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                    //}

                                    break;


                                case "userrole":

                                    _logger.LogInformation("Calling ExportUserRoles");

                                    var userRoleData = await _packageHelperService.ExportUserRoles(objectValues[i]);

                                    _logger.LogInformation("ExportUserRoles Completed");

                                    _logger.LogDebug("Export UserRoles for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(userRoleData, Formatting.Indented));

                                    successCheck = userRoleData.success;

                                    bool flatqueriesSuccessCheck = userRoleData.flatqueries;

                                    recordId = userRoleData.recordId;

                                    tableName = userRoleData.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {
                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }


                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success && flatqueriesSuccessCheck)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);


                                                    _logger.LogInformation("Adding Flatqueries from userroles To CurrentPayloads");

                                                    currentPayloads.Add(userRoleData.query);

                                                    _logger.LogInformation("Adding Flatqueries from userroles To FlatqueryPayload");

                                                    if (flatQueryPayloads.Count == 0)
                                                    {
                                                        flatQueryPayloads.Add(userRoleData.query);
                                                    }
                                                    else
                                                    {
                                                        flatQueryPayloads.Add(_packageHelperService.QUERY_SEPARATOR + userRoleData.query);
                                                    }

                                                    //flatQueryPayloads.Add(userRoleData.query);

                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    break;


                                case "usergroup":

                                    _logger.LogInformation("Calling ExportUserGroups");

                                    var userGroupData = await _packageHelperService.ExportUserGroups(objectValues[i]);

                                    _logger.LogInformation("ExportUserGroups Completed");

                                    _logger.LogDebug("Export UserGroup for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(userGroupData, Formatting.Indented));

                                    successCheck = userGroupData.success;

                                    recordId = userGroupData.recordId;

                                    tableName = userGroupData.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {

                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }


                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    break;

                                case "cards":

                                    _logger.LogInformation("Calling ExportCards");

                                    var uCards = await _packageHelperService.ExportCards(objectValues[i]);

                                    _logger.LogInformation("ExportCards Completed");

                                    _logger.LogDebug("Export Cards for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(uCards, Formatting.Indented));

                                    successCheck = uCards.success;

                                    recordId = uCards.recordId;

                                    tableName = uCards.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {

                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }


                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    break;

                                case "custompage":

                                    _logger.LogInformation("Calling ExportCustomPages");

                                    var customPageData = await _packageHelperService.ExportCustomPages(objectValues[i]);

                                    _logger.LogInformation("ExportCustomPages Completed");

                                    _logger.LogDebug("Export CustomPage for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(customPageData, Formatting.Indented));

                                    successCheck = customPageData.success;

                                    recordId = customPageData.recordId;

                                    tableName = customPageData.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {
                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }


                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }


                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    if (successCheck && customPageData.flatQuery)
                                    {
                                        _logger.LogInformation("Adding Flat Query Payload");

                                        currentPayloads.Add(customPageData.query);

                                        if (flatQueryPayloads.Count == 0)
                                        {
                                            flatQueryPayloads.Add(customPageData.query);
                                        }
                                        else
                                        {
                                            flatQueryPayloads.Add(_packageHelperService.QUERY_SEPARATOR + customPageData.query);
                                        }

                                        //flatQueryPayloads.Add(customPageData.query);
                                    }

                                    break;


                                case "developeroption":

                                    _logger.LogInformation("Calling ExportDeveloperOptions");

                                    var developerOptionDat = await _packageHelperService.ExportDeveloperOptions(objectValues[i]);

                                    _logger.LogInformation("ExportDeveloperOptions Completed");

                                    _logger.LogDebug("Export developerOption for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(developerOptionDat, Formatting.Indented));

                                    successCheck = developerOptionDat.success;

                                    recordId = developerOptionDat.recordId;

                                    tableName = developerOptionDat.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {
                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet);

                                                    _logger.LogInformation("AxPutConversion Completed");

                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }



                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} for table : {tableName[j]} and recordId : {recordId[j]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    break;

                                case "axvars":

                                    _logger.LogInformation("Calling AxVars");

                                    var axvarsData = await _packageHelperService.ExportAxVars(objectValues[i]);

                                    _logger.LogInformation("ExportAxVars Completed");

                                    _logger.LogDebug("Export AxVars for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(axvarsData, Formatting.Indented));

                                    successCheck = axvarsData.success;

                                    recordId = axvarsData.recordId;

                                    tableName = axvarsData.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {
                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed");

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }


                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} for table : {tableName[j]} and recordId : {recordId[j]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    break;

                                case "ads":

                                    _logger.LogInformation("Calling ExportADS");

                                    var uads = await _packageHelperService.ExportAds(objectValues[i]);

                                    _logger.LogInformation("ExportADS Completed");

                                    _logger.LogDebug("Export ADS for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(uads, Formatting.Indented));

                                    successCheck = uads.success;

                                    recordId = uads.recordId;

                                    tableName = uads.tableName;


                                    if (successCheck)
                                    {
                                        for (int j = 0; j < recordId.Length; j++)
                                        {
                                            try
                                            {

                                                _logger.LogInformation("Calling GetAxData Table : {tableName} RecordId : {recordId}", tableName[j], recordId[j]);

                                                string axGetData = await _axARMApiHelperService.GetAxData(appName, tableName[j], recordId[j], "recordid", userName, userPassword);

                                                _logger.LogInformation("GetAxData Completed\n" + axGetData);

                                                //var Sessionid = _axARMApiHelperService.GetStoredSessionID();

                                                string Sessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                                                //var jsonDataAxGet = JsonConvert.DeserializeObject(axGetData);

                                                JObject jsonDataAxGet = JObject.Parse(axGetData);

                                                ((JArray)jsonDataAxGet["result"]["data"])
                                                    .Children<JObject>()
                                                    .First(x => (string)x["n"] == "recid")["v"] = "0";

                                                object axputConvert;

                                                if (Sessionid != null)
                                                {

                                                    axputConvert = _axPutConversion.AxGetToPutConversion(Sessionid, tableName[j], jsonDataAxGet);

                                                    _logger.LogInformation("AxPutConversion Completed");
                                                }
                                                else
                                                {
                                                    throw new Exception($"GetAxData failed - sessionId is empty for AxPut Conversion{objectType},{objectValues[i]}");
                                                }

                                                ///// set RecId = 0



                                                dynamic convertResult = axputConvert;

                                                if (convertResult.success)
                                                {
                                                    _logger.LogInformation("Adding Payload To CurrentPayloads");

                                                    currentPayloads.Add(convertResult.message);

                                                    _logger.LogInformation("Adding Payload To AllPayloads");

                                                    AllPayloads.Add(convertResult.message);
                                                }
                                                else
                                                {
                                                    throw new Exception(convertResult.message.ToString());
                                                }

                                                //var jsonData = JsonConvert.DeserializeObject(axGetData);

                                                //currentPayloads.Add(jsonData);

                                                //AllPayloads.Add(jsonData);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "GetAxData Failed");

                                                throw new Exception($"GetAxData failed {objectType},{objectValues[i]} - {ex.Message}");
                                            }
                                        }
                                    }

                                    break;


                                //table
                                case "tables":

                                    _logger.LogInformation("Calling ExportTableAysnc");

                                    var table = await _postgresScriptsLogic.ExportTableAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportTableAysnc Completed");

                                    _logger.LogDebug("Export table for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(table, Formatting.Indented));

                                    successCheck = table.success;

                                    var data = table.data;


                                    if (successCheck)
                                    {
                                        postgresDBObjects.Add(data);
                                        currentPayloads.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;


                                //view
                                case "views":

                                    _logger.LogInformation("Calling ExportViewAsync");

                                    var views = await _postgresScriptsLogic.ExportViewAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportViewAsync Completed");

                                    _logger.LogDebug("Export views for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(views, Formatting.Indented));

                                    successCheck = views.success;

                                    data = views.data;


                                    if (successCheck)
                                    {
                                        postgresDBObjects.Add(data);
                                        currentPayloads.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;


                                //materializedview
                                case "materializedview":

                                    _logger.LogInformation("Calling ExportMtViewAsync");

                                    var materializedview = await _postgresScriptsLogic.ExportMtViewAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportMtViewAsync Completed");

                                    _logger.LogDebug("Export Mtviews for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(materializedview, Formatting.Indented));

                                    successCheck = materializedview.success;

                                    data = materializedview.data;


                                    if (successCheck)
                                    {
                                        postgresDBObjects.Add(data);
                                        currentPayloads.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;


                                //function
                                case "functions":

                                    _logger.LogInformation("Calling ExportFunctionAsync");

                                    var functions = await _postgresScriptsLogic.ExportFunctionAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportFunctionAsync Completed");

                                    _logger.LogDebug("Export Functions for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(functions, Formatting.Indented));

                                    successCheck = functions.success;

                                    data = functions.data;


                                    if (successCheck)
                                    {
                                        postgresDBObjects.Add(data);
                                        currentPayloads.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;


                                //procedure
                                case "procedures":

                                    _logger.LogInformation("Calling ExportProcedureAsync");

                                    var procedures = await _postgresScriptsLogic.ExportProcedureAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportProcedureAsync Completed");

                                    _logger.LogDebug("Export procedure for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(procedures, Formatting.Indented));

                                    successCheck = procedures.success;

                                    data = procedures.data;


                                    if (successCheck)
                                    {
                                        currentPayloads.Add(data);
                                        postgresDBObjects.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;

                                //sequence
                                case "sequence":

                                    _logger.LogInformation("Calling ExportSequenceAsync");

                                    var sequences = await _postgresScriptsLogic.ExportSequenceAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportSequenceAsync Completed");

                                    _logger.LogDebug("Export Sequences for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(sequences, Formatting.Indented));

                                    successCheck = sequences.success;

                                    data = sequences.data;


                                    if (successCheck)
                                    {
                                        postgresDBObjects.Add(data);
                                        currentPayloads.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;

                                //trigger
                                case "triggers":

                                    _logger.LogInformation("Calling ExportTriggerAsync");

                                    var triggers = await _postgresScriptsLogic.ExportTriggerAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportTriggerAsync Completed");

                                    _logger.LogDebug("Export Trigger for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(triggers, Formatting.Indented));

                                    successCheck = triggers.success;

                                    data = triggers.data;


                                    if (successCheck)
                                    {
                                        postgresDBObjects.Add(data);
                                        currentPayloads.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;


                                //constraints
                                case "constraint":

                                    _logger.LogInformation("Calling ExportConstraintAsync");

                                    var constraints = await _postgresScriptsLogic.ExportConstraintAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportConstraintAsync Completed");

                                    _logger.LogDebug("Export constraints for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(constraints, Formatting.Indented));

                                    successCheck = constraints.success;

                                    data = constraints.data;


                                    if (successCheck)
                                    {
                                        postgresDBObjects.Add(data);
                                        currentPayloads.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;


                                //index
                                case "index":

                                    _logger.LogInformation("Calling ExportIndexAsync");

                                    var index = await _postgresScriptsLogic.ExportIndexAsync(appName, objectValues[i]);

                                    _logger.LogInformation("ExportIndexAsync Completed");

                                    _logger.LogDebug("Export Index for Object : {object} \n {result}", objectValues[i], JsonConvert.SerializeObject(index, Formatting.Indented));

                                    successCheck = index.success;

                                    data = index.data;


                                    if (successCheck)
                                    {
                                        postgresDBObjects.Add(data);
                                        currentPayloads.Add(data);
                                    }
                                    else
                                    {
                                        throw new Exception(data);
                                    }
                                    break;


                            }


                            objectWisePayloads.Add(new
                            {
                                objectName = objectValues[i],
                                payloads = currentPayloads
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Object Processing Failed");

                            throw new Exception($"{objectType} export failed for object : {objectValues[i]} - {ex.Message}");
                        }
                    }


                    if (objectWisePayloads.Count > 0)
                    {
                        separatePayloads.Add(new
                        {
                            objectType,
                            objects = objectWisePayloads
                        });
                    }
                }

                _logger.LogInformation("Fetching Final SessionId");

                string finalSessionid = await _axARMApiHelperService.GetStoredSessionID(appName, userName, userPassword);

                _logger.LogInformation("Final SessionId Fetched");

                _logger.LogInformation("Preparing Final Response");

                _logger.LogInformation("Export Package Completed Successfully");

                _logger.LogDebug
                    ("Final Export Result : {result}", JsonConvert.SerializeObject(separatePayloads, Formatting.Indented));


                //object axputpayloads = new { };

                //if (AllPayloads != null && AllPayloads.Count > 0)
                //{
                //    axputpayloads = new
                //    {
                //        ARMSessionId = finalSessionid,

                //        trace = false,

                //        validateonly = false,

                //        axclient_dateformat = "yyyy-MM-dd",

                //        millisecsintimestamp = true,

                //        data = AllPayloads
                //    };
                //}

                //return new
                //{
                //    success = true,

                //    message = "Data fetched successfully",

                //    payload = new
                //    {
                //        axputpayloads,

                //        //allPayloads = new
                //        //{
                //        //    ARMSessionId = finalSessionid,

                //        //    trace = false,

                //        //    validateonly = false,

                //        //    axclient_dateformat = "yyyy-MM-dd",

                //        //    millisecsintimestamp = true,

                //        //    data = AllPayloads
                //        //},

                //        //separatePayloads = separatePayloads,

                //        DMLscripts = flatQueryPayloads,

                //        DDLScritps = postgresDBObjects


                //    }
                //};
                Dictionary<string, object> resultpayload = new();

                if (AllPayloads != null && AllPayloads.Count > 0)
                {
                    resultpayload.Add("axputpayloads", new
                    {
                        ARMSessionId = finalSessionid,
                        trace = true,
                        validateonly = false,
                        axclient_dateformat = "yyyy-MM-dd",
                        millisecsintimestamp = true,
                        data = AllPayloads
                    });
                }

                if (flatQueryPayloads != null && flatQueryPayloads.Count > 0)
                {
                    resultpayload.Add("dmlscripts", flatQueryPayloads);
                }

                if (postgresDBObjects != null && postgresDBObjects.Count > 0)
                {
                    resultpayload.Add("ddlscritps", postgresDBObjects);
                }

                return new
                {
                    success = true,
                    message = "Data fetched successfully",
                    hasData = resultpayload.Count > 0,
                    resultpayload
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export Package Failed");

                return new
                {
                    success = false,
                    message = "Export Package Failed",
                    errormessage = ex.Message
                };
            }
            finally
            {
                _logger.LogInformation("Closing DB Connection");

                await _axDBService.CloseConnection();

                _logger.LogInformation("ExportPackage Service Ended");
                _logger.LogInformation("================================================");
            }
        }

        private async Task<ExportPackageDTO> ProcessCardDependencies(ExportPackageDTO payload)
        {
            HashSet<string> adsList = payload.Objects.ads != null
                ? new HashSet<string>(payload.Objects.ads)
                : new HashSet<string>();

            for (int i = 0; i < payload.Objects.cards.Length; i++)
            {
                string cardName = payload.Objects.cards[i];

                _logger.LogInformation("Checking Dependencies For Card : {CardName}", cardName);

                string[] dependencies = await _packageHelperService.GetCardDependencies(cardName);

                if (dependencies == null || dependencies.Length == 0)
                {
                    _logger.LogInformation("No ADS Dependency Found For Card : {CardName}", cardName);
                    continue;
                }

                foreach (string datasource in dependencies)
                {
                    if (adsList.Add(datasource))
                    {
                        _logger.LogInformation("Added ADS Dependency : {ADS}", datasource);
                    }
                    else
                    {
                        _logger.LogInformation("ADS Already Exists : {ADS}", datasource);
                    }
                }
            }

            payload.Objects.ads = adsList.ToArray();

            return payload;
        }

        private async Task<ExportPackageDTO> ProcessTStructDependencies(ExportPackageDTO payload)
        {

            List<string> orderedTstructs = new List<string>();
            HashSet<string> addedTstructs = new HashSet<string>();

            foreach (string tstructName in payload.Objects.tstruct)
            {
                _logger.LogInformation("Checking Dependencies For tstruct : {tstruct}", tstructName);

                string[] dependencies = await _packageHelperService.GettstructDependencies(tstructName);

                //if (dependencies != null)
                if (dependencies != null && dependencies.Length > 0)
                {
                    // Dependencies are already returned as:
                    // Country, Address, Customer
                    foreach (string dependency in dependencies)
                    {
                        if (addedTstructs.Add(dependency))
                        {
                            orderedTstructs.Add(dependency);

                            _logger.LogInformation("Added Dependency : {tstruct}", dependency);
                        }
                        else
                        {
                            _logger.LogInformation("TStruct Already Exists : {tstruct}", dependency);
                        }
                    }
                }

                // Finally add the requested tstruct itself
                if (addedTstructs.Add(tstructName))
                {
                    orderedTstructs.Add(tstructName);

                    _logger.LogInformation("Added Tstruct : {tstruct}", tstructName);
                }
                else
                {
                    _logger.LogInformation("TStruct Already Exists : {tstruct}", tstructName);

                }
            }

            payload.Objects.tstruct = orderedTstructs.ToArray();

            return payload;
        }

        public async Task<ExportPackageDTO> PayloadDependencyCheck(ExportPackageDTO payload)
        {
            try
            {
                _logger.LogInformation("==============================================");
                _logger.LogInformation("PayloadDependencyCheck Started");

                string[] objectTypes ={"cards","tstruct"};

                foreach (string objectType in objectTypes)
                {
                    _logger.LogInformation("Checking Dependency For Object Type : {ObjectType}", objectType);

                    switch (objectType)
                    {
                        case "cards":

                            if (payload.Objects.cards == null || payload.Objects.cards.Length == 0)
                            {
                                _logger.LogInformation("Cards Not Found. Skipping Dependency Check.");
                                break;
                            }

                            //HashSet<string> adsList = payload.Objects.ads != null
                            //    ? new HashSet<string>(payload.Objects.ads, StringComparer.OrdinalIgnoreCase)
                            //    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                            //for (int i = 0; i < payload.Objects.cards.Length; i++)
                            //{
                            //    string cardName = payload.Objects.cards[i];

                            //    _logger.LogInformation("Checking Dependencies For Card : {CardName}", cardName);

                            //    string[] dependencies = await _packageHelperService.GetCardDependencies(cardName);

                            //    if (dependencies == null || dependencies.Length == 0)
                            //    {
                            //        _logger.LogInformation("No ADS Dependency Found For Card : {CardName}", cardName);
                            //        continue;
                            //    }

                            //    foreach (string datasource in dependencies)
                            //    {
                            //        if (adsList.Add(datasource))
                            //        {
                            //            _logger.LogInformation("Added ADS Dependency : {ADS}", datasource);
                            //        }
                            //        else
                            //        {
                            //            _logger.LogInformation("ADS Already Exists : {ADS}", datasource);
                            //        }
                            //    }
                            //}

                            //payload.Objects.ads = adsList.ToArray();

                            _logger.LogInformation("Started Processing Card Dependencies");

                             payload =  await ProcessCardDependencies(payload);

                            _logger.LogInformation("Completed Processing Card Dependencies");

                            break;

                        case "tstruct":

                            if (payload.Objects.tstruct == null || payload.Objects.tstruct.Length == 0)
                            {
                                _logger.LogInformation("Tstruct Not Found. Skipping Dependency Check.");
                                break;
                            }



                            //HashSet<string> tstrutlist = payload.Objects.tstruct != null
                            //    ? new HashSet<string>(payload.Objects.tstruct, StringComparer.OrdinalIgnoreCase)
                            //    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                            //for (int i = 0; i < payload.Objects.tstruct.Length; i++)
                            //{
                            //    string tstructName = payload.Objects.tstruct[i];

                            //    _logger.LogInformation("Checking Dependencies For tstruct : {tstruct}", tstructName);

                            //    string[] dependencies = await _packageHelperService.GettstructDependencies(tstructName);

                            //    if (dependencies == null || dependencies.Length == 0)
                            //    {
                            //        _logger.LogInformation("No tstruct Dependency Found For tsruct : {tstructName}", tstructName);
                            //        continue;
                            //    }

                            //    foreach (string tstructname in dependencies)
                            //    {
                            //        if (tstrutlist.Add(tstructname))
                            //        {
                            //            _logger.LogInformation("Added tstruct Dependency : {tstruct}", tstructname);
                            //        }
                            //        else
                            //        {
                            //            _logger.LogInformation("TStruct Already Exists : {tstruct}", tstructname);
                            //        }
                            //    }
                            //}

                            //payload.Objects.tstruct = tstrutlist.ToArray();

                            //break;

                            _logger.LogInformation("Started Processing TStruct Dependencies");

                            payload = await ProcessTStructDependencies(payload);

                            _logger.LogInformation("Completed Processing Tstruct Dependencies");

                            break;

                    }
                }

                _logger.LogInformation("PayloadDependencyCheck Completed Successfully");

                return payload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayloadDependencyCheck Failed");
                throw;
            }
        }
    }
}