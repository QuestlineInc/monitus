using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Xml;

namespace Service.Monitus.Core.Reporters
{
    /// <summary>
    /// Used to update Statuspage.io with the current status of an SLO.
    /// </summary>
    public class StatuspageReporter : Reporter
    {
        private string _statuspageMetricId;

        public StatuspageReporter(XmlTextReader reader) : base(reader)
        {
            reader.MoveToAttribute("statuspageMetricId");
            _statuspageMetricId = reader.Value;
        }

        public override void Report(double indicator)
        {
            // replace these with your own values
            var apiKey = "YOUR_STATUSPAGE_API_KEY";
            var pageId = "YOUR_PAGE_ID";

            throw new NotImplementedException();

            var metricId = _statuspageMetricId;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var value = indicator;
            var serializeMe = new StatuspageData
            {
                JsonData = new StatuspageData.Data
                {
                    Timestamp = timestamp,
                    Value = value
                }
            };
            var payloadJson = JsonConvert.SerializeObject(serializeMe);
            var bytes = Encoding.ASCII.GetBytes(payloadJson);
            var statuspageApiUrl = $"https://api.statuspage.io/v1/pages/{pageId}/metrics/{metricId}/data?api_key={apiKey}";

            var request = (HttpWebRequest)WebRequest.Create(statuspageApiUrl);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.AllowAutoRedirect = false;
            request.KeepAlive = false;

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch { }
        }
    }

    public class StatuspageData
    {
        [JsonProperty("data")]
        public Data JsonData { get; set; }
        public class Data
        {
            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }
            [JsonProperty("value")]
            public double Value { get; set; }
        }
    }
}
