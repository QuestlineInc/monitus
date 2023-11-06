using System;
using System.Net;
using System.Threading;
using System.Xml;

namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// Used to determine if a particular URL is accessible and responding without error.
    /// Will retry the configured number of times before failing.
    /// (GET requests only.)
    /// </summary>
    public class WebRequestSLI : ISLI
    {
        private double? _indicator { get; set; }
        private string _message { get; set; }
        public string URL { get; set; }
        public int RetryCount { get; set; }
        public double GetIndicator()
        {
            if (_indicator.HasValue)
                return _indicator.Value;

            _indicator = 0;

            var retries = 0;
            while (retries < RetryCount)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(URL);
                    request.Method = "GET";
                    request.AllowAutoRedirect = false;
                    request.KeepAlive = false;
                    request.Timeout = 20000; // 20 seconds, default is 100 seconds.
                    var response = request.GetResponse();
                    response.Dispose();
                    _indicator = 0;
                    break;
                }
                catch (Exception ex)
                {
                    _message = ex.Message;
                    _indicator = 1;
                    // if we fail try again after a brief delay, until we hit RetryCount
                    retries++;
                    Thread.Sleep(1000);
                }
            }

            return _indicator.Value;
        }

        public string GetMessage()
        {
            return _message;
        }

        public string GetIndicatorMessage()
        {
            return $"web request returned an error for URL {URL}: {GetMessage()}";
        }

        public WebRequestSLI(XmlTextReader reader)
        {
            reader.MoveToAttribute("url");
            URL = reader.Value;
            reader.MoveToAttribute("retryCount");
            RetryCount = Convert.ToInt32(reader.Value);
        }
    }
}
