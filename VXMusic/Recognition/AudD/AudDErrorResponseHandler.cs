using Newtonsoft.Json;
using System.Diagnostics;

namespace VXMusic.AudD;

public class AudDErrorResponseHandler
{
    public static void HandleAudDErrorResponse(string errorResponseString)
    {
        // {"status":"error","error":{"error_code":901,"error_message":"Recognition failed: authorization failed: no api_token passed and the limit was reached. Get an api_token from dashboard.audd.io."},"request_params":{"json":"{\"api_token\":\"f0c51b25ecd8068fce82a5bcfd9f5b6e\",\"return\":\"spotify\"}"},"request_api_method":"recognize","request_http_method":"POST","see api documentation":"https://docs.audd.io","contact us":"api@audd.io"}
        AudDError? errorResponse = JsonConvert.DeserializeObject<AudDError>(errorResponseString);

        Trace.WriteLine($"[AudD API] Status: {errorResponse.error.error_code}");
        Trace.WriteLine(errorResponse.error.error_message);
        
        // if (errorResponsePayload.TryGetValue("status", out string status))
        // {
        //     if (errorResponsePayload.TryGetValue("error", out string errorPayloadString))
        //     {
        //         Dictionary<string, string>? errorPayload = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorPayloadString);
        //         string errorCode = errorPayload["error_code"];
        //         string errorMessage = errorPayload["error_message"];
        //         
        //         Console.WriteLine($"[AudD API ERROR] {errorCode}");
        //         Console.WriteLine(errorMessage);
        //     }
        // }
    }
}