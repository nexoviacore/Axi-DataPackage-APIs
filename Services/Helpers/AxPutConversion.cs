using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AxiDataPackages.Services.Helpers
{
    public class AxPutConversion
    {
        private readonly ILogger<AxPutConversion> _logger;

        public AxPutConversion(ILogger<AxPutConversion> logger)
        {
            _logger = logger;
        }
        public bool ValidateAxGetPayload(string sessionId, string transId, JObject payload)
        {
            try
            {
                _logger.LogInformation("ValidateAxGetPayload Started : {transId}", transId);
                _logger.LogInformation("Validating SessionId");
                _logger.LogDebug("CurrentInput Payload : {result}", JsonConvert.SerializeObject(payload, Formatting.Indented));

                if (string.IsNullOrWhiteSpace(sessionId))
                    throw new Exception("SessionId is required");

                _logger.LogInformation("Validating TransId");

                if (string.IsNullOrWhiteSpace(transId))
                    throw new Exception("TransId is required");

                _logger.LogInformation("Validating Payload");

                if (payload == null)
                    throw new Exception("Payload is empty");


                JObject resultObject = payload["result"] as JObject;

                if (resultObject == null)
                    throw new Exception("Result object is missing");


                JToken recordId = resultObject["recordid"];

                if (recordId == null || string.IsNullOrWhiteSpace(recordId.ToString()))
                    throw new Exception("RecordId is missing");


                JArray dataArray = resultObject["data"] as JArray;

                if (dataArray == null || dataArray.Count == 0)
                    throw new Exception("Result data array is empty");


                _logger.LogInformation("ValidateAxGetPayload Completed Successfully");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateAxGetPayload Failed");
                throw new Exception($"AxGet payload validation failed - {ex.Message}");
            }
        }



        public object AxGetToPutConversion(string sessionId, string transId, JObject payload,string masterFormName = "", bool FormNameAdd = false)
        {
            try
            {
                _logger.LogInformation("AxGetToPutConversion Started : {transId}", transId);
                _logger.LogInformation("Calling ValidateAxGetPayload");

                bool isValid = ValidateAxGetPayload(sessionId, transId, payload);

                _logger.LogInformation("ValidateAxGetPayload Completed");

                if (!isValid)
                {
                    return new
                    {
                        success = false,
                        message = "Invalid AxGet payload"
                    };
                }


                JObject resultObject = payload["result"] as JObject;

                JArray dataArray = resultObject["data"] as JArray;


                JObject submitData = new JObject();

                JObject currentDC = null;

                string currentDCName = string.Empty;


                foreach (JObject item in dataArray)
                {
                    string fieldName = item["n"]?.ToString();

                    //string fieldValue = item["v"]?.ToString();

                    JToken fieldValue = item["v"];

                    string fieldType = item["t"]?.ToString();


                    if (string.IsNullOrWhiteSpace(fieldName))
                        continue;


                    // DC CREATION
                    if (fieldType == "dc")
                    {
                        _logger.LogInformation("Creating DC : {dcName}", fieldName);

                        currentDCName = fieldName.ToLower();

                        currentDC = new JObject();

                        submitData[currentDCName] = currentDC;

                        continue;
                    }


                    // IGNORE SYSTEM FIELDS
                    if (fieldName.StartsWith("axp_recid"))
                        continue;


                    if (currentDC == null)
                        continue;


                    // ROW CREATION
                    string rowValue = item["r"]?.ToString();

                    int rowNumber = 1;

                    if (!string.IsNullOrWhiteSpace(rowValue))
                    {
                        int parsedRowNumber = Convert.ToInt32(rowValue);

                        rowNumber = parsedRowNumber == 0 ? 1 : parsedRowNumber;
                    }


                    _logger.LogInformation("Processing Row : {rowNumber}", rowNumber);

                    string rowName = "row" + rowNumber;


                    if (currentDC[rowName] == null)
                    {
                        currentDC[rowName] = new JObject();
                    }


                    JObject rowObject = currentDC[rowName] as JObject;


                    // FIELD ASSIGNMENT
                    _logger.LogInformation("Assigning Field : {fieldName}", fieldName);

                    rowObject[fieldName] = fieldValue;
                }


                _logger.LogInformation("Preparing Transaction Object");

                // TRANSACTION OBJECT
                JObject transactionObject = new JObject
                {
                    ["transid"] = transId,

                    ["action"] = "create",

                   // ["submitdata"] = submitData

                };



                _logger.LogInformation("MasterObject boolean is set to true for this payload : "+transId);



                _logger.LogInformation("Master Object Name is  : " + masterFormName);

                if (FormNameAdd)
                {
                    transactionObject["masterObject"] = masterFormName;
                }

                transactionObject["submitdata"] = submitData;


                //// DATA ARRAY
                //JArray finalDataArray = new JArray
                //{
                //    transactionObject
                //};


                //// FINAL PAYLOAD
                //JObject finalPayload = new JObject
                //{
                //    ["ARMSessionId"] = sessionId,

                //    ["trace"] = false,

                //    ["Data"] = finalDataArray
                //};


                _logger.LogInformation("AxGetToPutConversion Completed Successfully");

                _logger.LogDebug("AxGetToPut converted payload : {result}",JsonConvert.SerializeObject(transactionObject, Formatting.Indented));

                return new
                {
                    success = true,
                    message = transactionObject
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AxGetToPutConversion Failed");
                throw new Exception($"AxGetToPutConversion failed - {ex.Message}");
            }
        }
    }
}