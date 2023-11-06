using System;
using System.Linq;
using System.ServiceProcess;
using System.Xml;

namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// Used to determine if a particular Windows service is running.
    /// </summary>
    public class ServiceStartedSLI : ISLI
    {
        private string _message { get; set; }
        private double? _indicator { get; set; }
        public string ServiceName { get; set; }
        public string ServerName { get; set; }

        public ServiceStartedSLI(XmlTextReader reader)
        {
            reader.MoveToAttribute("serviceName");
            ServiceName = reader.Value;
            reader.MoveToAttribute("serverName");
            ServerName = reader.Value;
        }

        public double GetIndicator()
        {
            if (_indicator.HasValue)
                return _indicator.Value;

            try
            {
                _indicator = 0;
                var serviceArray = ServiceController.GetServices(ServerName);
                var service = serviceArray.Where(s => s.ServiceName.Contains(ServiceName)).FirstOrDefault();
                
                if(service == null || service.Status != ServiceControllerStatus.Running)
                {
                    //Service is null, or is not running : then return -1 (note this doesn't include in the process of starting up)
                    //  NOTE: this only checks for the first service that contains the configured name...
                    //  So if only one service with ServiceName in its name is running
                    //  Then this SLI will still return true (be specific in your configured names!)
                    _indicator = -1;
                }
            }
            catch (Exception ex)
            {
                _message = ex.Message;
                _indicator = -1;
            }

            return _indicator.Value;
        }

        public string GetMessage()
        {
            return _message;
        }

        public string GetIndicatorMessage()
        {
            return $"service *{ServiceName}* is null or not running on server *{ServerName}*";
        }
    }
}
