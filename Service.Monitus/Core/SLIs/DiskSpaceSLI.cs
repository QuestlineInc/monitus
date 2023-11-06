using System;
using System.Diagnostics;
using System.Xml;

namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// Used the determine the disk space usage of a particular machine.
    /// </summary>
    public class DiskSpaceSLI : ISLI
    {
        private double? _indicator { get; set; }
        private double _totalDiskSpaceInMB { get; set; }
        private string _message { get; set; }
        // should be a string like "C:"
        private string _mainDriveName { get; set; }

        public double GetIndicator()
        {
            if (_indicator.HasValue)
                return _indicator.Value;

            try
            {
                _indicator = 0;

                using (var perfCounter = new PerformanceCounter("LogicalDisk", "Free Megabytes", _mainDriveName))
                {
                    _indicator = Math.Round((_totalDiskSpaceInMB - perfCounter.NextValue()) / _totalDiskSpaceInMB * 100, 2);
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
            return $"Disk usage is at {GetIndicator()}%";
        }

        public string GetMessage()
        {
            return _message;
        }

        public DiskSpaceSLI(XmlTextReader reader)
        {
            reader.MoveToAttribute("mainDriveName");
            _mainDriveName = reader.Value;
            reader.MoveToAttribute("totalDiskSpaceInMB");
            _totalDiskSpaceInMB = double.Parse(reader.Value);
        }
    }
}
