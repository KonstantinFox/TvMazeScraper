namespace DataAccess.Configuration
{
    public class ScraperOptions
    {
        public string BaseUri { get; set; }

        public string ShowsUri { get; set; }

        public string CastUri { get; set; }

        public int RepeatTaskEveryHours { get; set; }

        public int MazeApiMaxPageSize { get; set; }

        public int HttpClientRetryCount { get; set; }

        public int HttpClientTimeoutSeconds { get; set; }
    }
}