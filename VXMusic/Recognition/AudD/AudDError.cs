namespace VXMusic.AudD;

// {
//     "status": "error",
//     "error": {
//         "error_code": 901,
//         "error_message": "Recognition failed: authorization failed: no api_token passed and the limit was reached. Get an api_token from dashboard.audd.io."
//     },
//     "request_params": {
//         "json": "{\"api_token\":\"caught_you_snoopin\",\"return\":\"spotify\"}"
//     },
//     "request_api_method": "recognize",
//     "request_http_method": "POST",
//     "see api documentation": "https://docs.audd.io",
//     "contact us": "api@audd.io"
// }

public class AudDError
{
    public string status { get; set; }
    public Error error { get; set; }
    public RequestParams request_params { get; set; }
    public string request_api_method { get; set; }
    public string request_http_method { get; set; }
    public string see_api_documentation { get; set; }
    public string contact_us { get; set; }
}

public class Error
{
    public int error_code { get; set; }
    public string? error_message { get; set; }
}

public class RequestParams
{
    public string json { get; set; }
}