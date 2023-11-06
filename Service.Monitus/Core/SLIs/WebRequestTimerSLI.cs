using System;
using System.Net;
using System.Diagnostics;
using System.Xml;

namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// Used to determine if a particular URL is responding in a timely manner.
    /// (GET requests only.)
    /// </summary>
    public class WebRequestTimerSLI : ISLI
    {
        public string URL { get; set; }
        private string _message { get; set; }
        private double? _indicator { get; set; }

        public WebRequestTimerSLI(XmlTextReader reader)
        {
            reader.MoveToAttribute("url");
            URL = reader.Value;
        }

        public double GetIndicator()
        {
            if (_indicator.HasValue) 
                return _indicator.Value;

            _indicator = 0;

            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                var request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";
                request.AllowAutoRedirect = false;
                request.KeepAlive = false;

                request.GetResponse();

                timer.Stop();
                _indicator = timer.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                _message = ex.Message;
                _indicator = -1.00;
            }
            return _indicator.Value;
        }             
        
        public string GetMessage()
        {
            return _message;
        }

        public string GetIndicatorMessage()
        {
            return $"web request took *{GetIndicator()} ms*";
        }
    }
}
