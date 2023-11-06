
using System.Xml;

namespace Service.Monitus.Core.Reporters
{
    /// <summary>
    /// Reporters are used to update things like status pages or other dashboards with the current status of an SLO.
    /// </summary>
    public abstract class Reporter
    {
        public abstract void Report(double indicator);
        public Reporter(XmlTextReader reader)
        {
        }
    }
}