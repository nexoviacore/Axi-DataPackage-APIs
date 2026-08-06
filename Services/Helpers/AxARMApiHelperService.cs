//using Newtonsoft.Json;
//using System.Net.Http.Headers;
//using System.Text;
//using Microsoft.Extensions.Logging;

//namespace AxiDataPackages.Services.Helpers
//{
//    public class AxARMApiHelperService
//    {
//        private readonly HttpClient _httpClient;

//        private readonly IConfiguration _configuration;

//        private string _token = "";

//        private string _sessionId = "";

//        public AxARMApiHelperService
//        (
//            HttpClient httpClient,
//            IConfiguration configuration
//        )
//        {
//            _httpClient = httpClient;

//            _configuration = configuration;
//        }

//        public async Task GetToken
//        (
//            string appName,
//            string userName,
//            string password
//        )
//        {
//            try
//            {
//                string urlPrefix = _configuration["armApi:armapiurl"];

//                string urlSuffix = "/api/v1/Signin";

//                string finalUrl = urlPrefix + urlSuffix;

//                var payload = new
//                {
//                    appname = appName,
//                    UserName = userName,
//                    password = password,
//                    Language = "English",
//                    SessionId = "12345",
//                    Globalvars = true,
//                    ClearPreviousSession = true,
//                    trace = false
//                };

//                var jsonPayload = JsonConvert.SerializeObject(payload);

//                var content =new StringContent
//                    (
//                        jsonPayload,
//                        Encoding.UTF8,
//                        "application/json"
//                    );

//                var response = await _httpClient.PostAsync
//                    (
//                        finalUrl,
//                        content
//                    );

//                var responseString = await response.Content.ReadAsStringAsync();

//                dynamic result = JsonConvert.DeserializeObject(responseString);

//                if (!response.IsSuccessStatusCode)
//                {
//                    string errorMessage =  result?.result?.message;

//                    throw new Exception
//                    (
//                        "Signin API Failed : " +
//                        errorMessage
//                    );
//                }

//                if (result == null)
//                {
//                    throw new Exception
//                    (
//                        "Signin API Failed : Invalid Response"
//                    );
//                }

//                _token = result.result.token;

//                _sessionId = result.result.ARMSessionId;

//                if (string.IsNullOrWhiteSpace(_token))
//                {
//                    throw new Exception
//                    (
//                        "Signin API Failed : Token Not Found"
//                    );
//                }

//                if (string.IsNullOrWhiteSpace(_sessionId))
//                {
//                    throw new Exception
//                    (
//                        "Signin API Failed : SessionId Not Found"
//                    );
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception
//                (
//                    "Signin API Failed : " +
//                    ex.Message
//                );
//            }
//        }

//        public async Task<string> AxGetCall
//        (
//            string transId,
//            string keyField,
//            string keyValue
//        )
//        {
//            try
//            {
//                string urlPrefix = _configuration["armApi:armapiurl"];

//                string urlSuffix = "/api/v1/AxGet";

//                string finalUrl = urlPrefix + urlSuffix;

//                var payload = new
//                {
//                    ARMSessionId = _sessionId,
//                    action = "view",
//                    trace = false,
//                    transid = transId,
//                    keyfield = keyField,
//                    CachePermissions = false,
//                    keyvalue = keyValue,
//                    AxpertWSFormat = true
//                };

//                var jsonPayload = JsonConvert.SerializeObject(payload);

//                var request = new HttpRequestMessage
//                    (
//                        HttpMethod.Post,
//                        finalUrl
//                    );

//                request.Headers.Authorization  =  new AuthenticationHeaderValue
//                    (
//                        "Bearer",
//                        _token
//                    );

//                request.Content = new StringContent
//                    (
//                        jsonPayload,
//                        Encoding.UTF8,
//                        "application/json"
//                    );

//                _logger.LogInformation("Calling AxGet API");

//                var response = await _httpClient.SendAsync(request);

//                _logger.LogInformation("AxGet API Response Received");

//                var responseString = await response.Content.ReadAsStringAsync();

//                if (!response.IsSuccessStatusCode)
//                {
//                    throw new Exception
//                    (
//                        "AxGet API Failed : " +
//                        responseString
//                    );
//                }

//                _logger.LogInformation("AxGetCall Completed Successfully");

//                return responseString;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception
//                (
//                    "AxGet API Failed : " +
//                    ex.Message
//                );
//            }
//        }

//        public async Task<string> GetAxData
//        (
//            string appName,
//            string transId,
//            string recordId,
//            string keyField,
//            string userName,
//            string password
//        )
//        {
//            try
//            {
//                await GetToken (appName,userName,password);

//                var result = await AxGetCall(transId,keyField,recordId);

//                _logger.LogInformation("GetAxData Completed Successfully");

//                   return result;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception
//                (
//                    ex.Message
//                );
//            }
//        }
//    }
//}
using ARMCommon.Model;
using AxiDataPackages.References;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace AxiDataPackages.Services.Helpers
{
    public class AxARMApiHelperService
    {
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;

        private SigninCredential _signinCredential = new SigninCredential();

        private double ExpiryTokenTime;

        private string sessionID;

        private readonly ILogger<AxARMApiHelperService> _logger;

        private readonly AxDBService _axDBService;

        public AxARMApiHelperService(HttpClient httpClient, IConfiguration configuration, ILogger<AxARMApiHelperService> logger,AxDBService axdbservice)
        {
            _httpClient = httpClient;

            _configuration = configuration;

            _logger = logger;

            _axDBService = axdbservice;

        }

        public string GetStoredToken()
        {
            return _signinCredential.Token;
        }

        public async Task<string> GetStoredSessionID(string appName, string userName, string password)
        {

            _logger.LogInformation("GetStoredSessionID Started : {appName}", appName);
            try
            {
                bool isTokenEmpty = string.IsNullOrWhiteSpace(_signinCredential.Token);

                bool isSessionEmpty = string.IsNullOrWhiteSpace(_signinCredential.SessionId);

                bool isSessionExpired =
                    DateTime.Now.Subtract(_signinCredential.StartTime).TotalMinutes > ExpiryTokenTime;


                if (isTokenEmpty || isSessionEmpty || isSessionExpired)
                {
                    _logger.LogInformation("Session Expired Or Empty");
                    _logger.LogInformation("Calling GetToken");

                    await GetToken(appName, userName, password);

                    _logger.LogInformation("GetToken Completed");
                }


                _logger.LogInformation("Returning Stored SessionId");

                return _signinCredential.SessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStoredSessionID Failed");
                throw new Exception("GetStoredSessionID Failed : " + ex.Message);
            }
        }
        public async Task<string> GetPassWord(string userName)
        {
            string sql = "SELECT password FROM axusers WHERE username = @userName";

            string[] paramNames = { "@userName" };

            DbType[] paramTypes = { DbType.String };

            object[] paramValues = { userName };

            _logger.LogInformation("Executing Query");

            DataTable result = await _axDBService.ExecuteSelectQuery(sql,paramNames,paramTypes,paramValues);

            if (result != null && result.Rows.Count > 0)
            {

               _logger.LogInformation("UserName is available");
                return result.Rows[0]["password"].ToString();
            }

            _logger.LogInformation("UserName is not available");
            return string.Empty; 
        }


        public async Task<string> GetSeed()
        {
           Random random = new Random();

           int seed = random.Next(100000, 1000000); // 100000 to 999999

           return seed.ToString();
        }

        public async Task<string> GetHashedPassWord(string password, string seed)
        {
            string combinedValue = seed + password;

            try
            {

                MD5 md5 = MD5.Create();

                byte[] inputBytes = Encoding.UTF8.GetBytes(combinedValue);

                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                md5.Dispose();

                return sb.ToString().ToLower();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task GetToken(string appName, string userName, string password)
        {

            _logger.LogInformation("GetToken Started : {appName}", appName);
            try
            {
                string urlPrefix = _configuration["armApi:armapiurl"];

                string urlSuffix = "/AxAuth/api/v1/Signin";
                //string urlSuffix = "/api/v1/Signin";

                string finalUrl = urlPrefix + urlSuffix;

                _logger.LogInformation("Username received is " + userName);

                string passwordTakenFromDb = await GetPassWord(userName);


                _logger.LogInformation("Password fetched is " + passwordTakenFromDb);

                if (string.IsNullOrWhiteSpace(passwordTakenFromDb))
                {
                    throw new Exception("GetPassword Failed either username is not available or something goes wrong - Signin Call Fails");
                }

                string seed = await GetSeed();

                if (string.IsNullOrWhiteSpace(seed))
                {
                    throw new Exception("GetSeed Failed - Signin Call Fails");
                }

                passwordTakenFromDb = await GetHashedPassWord(passwordTakenFromDb, seed);


                _logger.LogInformation("Password After Hashing is " + passwordTakenFromDb);

                if (string.IsNullOrWhiteSpace(passwordTakenFromDb))
                {
                    throw new Exception("GetPassword Failed while hashing - Signin Call Fails");
                }


                var payload = new
                {
                    appname = appName,
                    UserName = userName,
                    password = passwordTakenFromDb,
                    //UserName = "admin",
                    //password = "40e39203e4447a772d07710cf788eeb1", //22723bbd4217a0abf6d3e68073c7603d
                    Language = "English",
                    SessionId = "12345",
                    Globalvars = true,
                    ClearPreviousSession = true,
                    trace = true,
                    //seed = "123456"
                    seed = seed
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Signin API");

                _logger.LogInformation(finalUrl);

                _httpClient.Timeout = TimeSpan.FromMinutes(10);

                var response = await _httpClient.PostAsync(finalUrl, content);

                _logger.LogInformation("Signin API Response Received");

                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("SignInApi Result : {result}", responseString);

                dynamic result = JsonConvert.DeserializeObject(responseString);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = result?.result?.message;

                    throw new Exception("Signin API Failed : " + errorMessage);
                }

                if (result == null)
                {
                    throw new Exception("Signin API Failed : Invalid Response");
                }

                _signinCredential.Token = result.result.token;

                _signinCredential.SessionId = result.result.ARMSessionId;

                _signinCredential.StartTime = DateTime.Now;

                sessionID = result.result.ARMSessionId;

                ExpiryTokenTime = ((result.result.expiry) / 1000) / 60;

                if (string.IsNullOrWhiteSpace(_signinCredential.Token))
                {
                    throw new Exception("Signin API Failed : Token Not Found");
                }

                if (string.IsNullOrWhiteSpace(_signinCredential.SessionId))
                {
                    throw new Exception("Signin API Failed : SessionId Not Found");
                }

                _logger.LogInformation("Signin Completed Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signin API Failed");
                throw new Exception("Signin API Failed : " + ex.Message);
            }
        }

        public async Task<string> AxGetCall(string transId, string keyField, string keyValue)
        {

            _logger.LogInformation("AxGetCall Started : {transId}", transId);
            try
            {
                string urlPrefix = _configuration["armApi:armapiurl"];

                //https://agile.axi-global.com/axis/AxTstructData/api/v1/AxPut

                string urlSuffix = "/AxTstructData/api/v1/AxGet";
                //string urlSuffix = "/api/v1/AxGet";

                string finalUrl = urlPrefix + urlSuffix;

                var payload = new
                {
                    ARMSessionId = _signinCredential.SessionId,
                    action = "view",
                    trace = true,
                    transid = transId,
                    keyfield = keyField,
                    CachePermissions = false,
                    keyvalue = keyValue,
                    AxpertWSFormat = true
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);

                var request = new HttpRequestMessage(HttpMethod.Post, finalUrl);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _signinCredential.Token);

                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling AxGet API");

                _logger.LogInformation(finalUrl);

                var response = await _httpClient.SendAsync(request);

                _logger.LogInformation("AxGet API Response Received");

                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("AxGet Result : {result}", responseString);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("AxGet API Failed : " + responseString);
                }

                _logger.LogInformation("AxGetCall Completed Successfully");

                return responseString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AxGet API Failed");
                throw new Exception("AxGet API Failed : " + ex.Message);
            }
        }

        public async Task<string> GetAxData(string appName, string transId, string recordId, string keyField, string userName, string password)
        {

            _logger.LogInformation("GetAxData Started : {transId}", transId);
            try
            {
                bool isTokenEmpty = string.IsNullOrWhiteSpace(_signinCredential.Token);

                bool isSessionEmpty = string.IsNullOrWhiteSpace(_signinCredential.SessionId);

                bool isSessionExpired = DateTime.Now.Subtract(_signinCredential.StartTime).TotalMinutes > ExpiryTokenTime;

                if (isTokenEmpty || isSessionEmpty || isSessionExpired)
                {
                    _logger.LogInformation("Session Expired Or Empty");
                    _logger.LogInformation("Calling GetToken");

                    await GetToken(appName, userName, password);

                    _logger.LogInformation("GetToken Completed");
                }

                //var result = await AxGetCall(transId, keyField, recordId);

                // _logger.LogInformation("GetAxData Completed Successfully");

                //return result;

                try
                {
                    var result = await AxGetCall(transId, keyField, recordId);

                    _logger.LogInformation("GetAxData Completed Successfully");

                    _logger.LogDebug("AxGet Returned JSON : {result}",JsonConvert.SerializeObject(result, Formatting.Indented));

                    return result;
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message.ToLower();

                    bool isSessionError =
                        errorMessage.Contains("unauthorized") ||
                        errorMessage.Contains("invalid session") ||
                        errorMessage.Contains("token expired") ||
                        errorMessage.Contains("sessionid") ||
                        errorMessage.Contains("sessionid is not valid") ||
                        errorMessage.Contains("session not found");

                    if (isSessionError)
                    {
                        _logger.LogInformation("Calling GetToken");

                        await GetToken(appName, userName, password);

                        _logger.LogInformation("GetToken Completed");

                        var retryResult = await AxGetCall(transId, keyField, recordId);

                        _logger.LogInformation("Retry AxGetCall Completed Successfully");

                        return retryResult;
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAxData Failed");
                throw new Exception(ex.Message);
            }
        }

        public async Task<(bool success, string message)> AxPutCall(string appName,string userName,string password,JObject payload)
        {
            _logger.LogInformation("AxPutCall Started");
            try
            {
                string sessionId =await GetStoredSessionID
                    (
                        appName,
                        userName,
                        password
                    );

                string token = GetStoredToken();

                payload["ARMSessionId"] = sessionId;

                string urlPrefix = _configuration["armApi:armapiurl"];

                if (string.IsNullOrWhiteSpace(urlPrefix))
                {
                    throw new Exception("armApi:armapiurl is missing in appsettings");
                }

                string urlSuffix = "/AxTstructData/api/v1/AxPut";
                //string urlSuffix = "/api/v1/AxPut";

                string finalUrl = urlPrefix + urlSuffix;

                string jsonPayload = payload.ToString();

                try
                {
                    var request = new HttpRequestMessage
                    (
                        HttpMethod.Post,
                        finalUrl
                    );

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue
                        (
                            "Bearer",
                            token
                        );

                    request.Content = new StringContent
                    (
                        jsonPayload,
                        Encoding.UTF8,
                        "application/json"
                    );

                    _logger.LogInformation("Calling AxPut API");

                    _logger.LogInformation(finalUrl);

                    _httpClient.Timeout = TimeSpan.FromMinutes(10);

                    var response = await _httpClient.SendAsync(request);

                    _logger.LogInformation("AxPut API Response Received");

                    string responseString =
                        await response.Content.ReadAsStringAsync();

                    dynamic result =
                        JsonConvert.DeserializeObject(responseString);

                    string message =
                        result?.result?.message ??
                        result?.message ??
                        responseString;

                    _logger.LogDebug("Axput Result : {result}", responseString);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("AxPut Completed Successfully");

                        //if (result?.result?.message?.ToString().ToLower() == "success")
                        //{
                        //     message =
                        //         result?.result?.data?[0]?.message?.ToString() ??
                        //         result?.result?.message?.ToString() ??
                        //         result?.message?.ToString() ??
                        //         responseString;
                        //}
                        if (result?.result?.message?.ToString().ToLower() == "success")
                        {
                            if (result?.result?.data != null)
                            {
                                List<string> messages = new List<string>();

                                foreach (var item in result.result.data)
                                {
                                    messages.Add($"{item.message} - RecordId: {item.recordid}");
                                }

                                message = string.Join(Environment.NewLine, messages);
                            }
                            else
                            {
                                message =
                                    result?.result?.message?.ToString() ??
                                    result?.message?.ToString() ??
                                    responseString;
                            }
                        }

                        return (true, message);
                    }

                    _logger.LogWarning("AxPut Failed : {message}", message);

                    throw new Exception(message);

                    //return (false, message);
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message.ToLower();

                    bool isSessionError =
                        errorMessage.Contains("unauthorized") ||
                        errorMessage.Contains("invalid session") ||
                        errorMessage.Contains("token expired") ||
                        errorMessage.Contains("sessionid") ||
                        errorMessage.Contains("SessionId is not valid") ||
                        errorMessage.Contains("session not found");

                    if (isSessionError)
                    {
                        _logger.LogInformation("Session Error Found. Calling GetToken");

                        await GetToken
                        (
                            appName,
                            userName,
                            password
                        );

                        sessionId =
                            await GetStoredSessionID
                            (
                                appName,
                                userName,
                                password
                            );

                        token = GetStoredToken();

                        payload["ARMSessionId"] = sessionId;

                        var retryRequest = new HttpRequestMessage
                        (
                            HttpMethod.Post,
                            finalUrl
                        );

                        retryRequest.Headers.Authorization =
                            new AuthenticationHeaderValue
                            (
                                "Bearer",
                                token
                            );

                        retryRequest.Content = new StringContent
                        (
                            payload.ToString(),
                            Encoding.UTF8,
                            "application/json"
                        );

                        _logger.LogInformation("Retrying AxPut API");

                        var retryResponse =
                            await _httpClient.SendAsync(retryRequest);

                        string retryResponseString =
                            await retryResponse.Content.ReadAsStringAsync();

                        dynamic retryResult =
                            JsonConvert.DeserializeObject(retryResponseString);

                        string retryMessage =
                            retryResult?.result?.message ??
                            retryResult?.message ??
                            retryResponseString;

                        _logger.LogDebug("Axput Result : {result}", retryResponseString);

                        if (retryResponse.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Retry AxPut Completed Successfully");

                            if (retryResult?.result?.message?.ToString().ToLower() == "success")
                            {
                                retryMessage =
                                    retryResult?.result?.data?[0]?.message?.ToString() ??
                                    retryResult?.result?.message?.ToString() ??
                                    retryResult?.message?.ToString() ??
                                    retryResponseString; 
                            }

                            return (true, retryMessage);
                        }

                        _logger.LogWarning("Retry AxPut Failed : {message}", retryMessage);

                        return (false, retryMessage);
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AxPutCall Failed : {message}", ex.Message);

                throw;
            }
        }
    }

    public class SigninCredential
    {
        public string Token { get; set; } = "";

        public string SessionId { get; set; } = "";

        public DateTime StartTime { get; set; }
    }
}