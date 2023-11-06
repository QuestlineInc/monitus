using System;
using System.Diagnostics;
using System.Xml;

namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// Used to determine the memory usage of a particular machine.
    /// </summary>
    public class MemoryUsageSLI : ISLI
    {
        private double? _indicator { get; set; }
        private double _totalMemoryInMB { get; set; }
        private string _message { get; set; }
        private string _machineName { get; set; }

        public double GetIndicator()
        {
            if (_indicator.HasValue)
                return _indicator.Value;

            try 
            {
                _indicator = 0;

                // get % memory usage of services box
                using (var perfCounter = new PerformanceCounter("Memory", "Available MBytes", string.Empty, _machineName))
                {
                    var availableMB = perfCounter.NextValue();

                    _indicator = Math.Round((_totalMemoryInMB - availableMB) / _totalMemoryInMB * 100, 2);
                }
            }
            catch (Exception ex) 
            {
                _indicator = -1.00;
                _message = ex.Message;
            }

            return _indicator.Value;
        }

        public string GetIndicatorMessage()
        {
            return $"Memory usage is at {GetIndicator()}%";
        }

        public string GetMessage()
        {
            return _message;
        }

        public MemoryUsageSLI(XmlTextReader reader)
        {
            reader.MoveToAttribute("machineName");
            _machineName = reader.Value;
            reader.MoveToAttribute("totalMemoryInMB");
            _totalMemoryInMB = double.Parse(reader.Value);
        }
    }
}
