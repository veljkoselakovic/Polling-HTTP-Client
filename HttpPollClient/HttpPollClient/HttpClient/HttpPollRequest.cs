namespace HttpPollClient.HttpClient
{
    public class HttpPollRequest
    {
        public string Url { get; set; }
        public string Method { get; set; } = "GET";
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; } = string.Empty;
        public int TimeoutInSeconds { get; set; } = 30;

        public HttpPollRequest(string url)
        {
            Url = url;
        }
    }
}
