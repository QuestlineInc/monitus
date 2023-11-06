using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// Used to determine the CPU usage of a particular machine.
    /// </summary>
    public class CPUUsageSLI : ISLI
    {
        private double? _indicator { get; set; }
        private string _message { get; set; }
        private string _machineName { get; set; }

        public double GetIndicator()
        {
            if (_indicator.HasValue)
                return _indicator.Value;

            try 
            {
                _indicator = 0;

                // get % CPU usage over a short time span (a single check can be misleading, this averages it out)
                using (var perfCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total", _machineName))
                {
                    var nums = new List<float>();
                    for (int i = 0; i < 10; i++)
                    {
                        nums.Add(perfCounter.NextValue());
                        Thread.Sleep(1200);
                    }
                    _indicator = Math.Round(nums.Sum() / nums.Count, 2);
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
            return $"CPU usage is at {GetIndicator()}%";
        }

        public string GetMessage()
        {
            return _message;
        }

        public CPUUsageSLI(XmlTextReader reader)
        {
            reader.MoveToAttribute("machineName");
            _machineName = reader.Value;
        }
    }
}
