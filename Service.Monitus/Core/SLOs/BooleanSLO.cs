using System.Xml;

namespace Service.Monitus.Core.SLOs
{
    /// <summary>
    /// A specific SLO that is a boolean value.  It is either true or false.
    /// For example, whether a service is up or down.
    /// </summary>
    public class BooleanSLO : SLO
    {
        public BooleanSLO(XmlTextReader reader) : base(reader)
        {
        }

        public override string BuildFailureMessage(string componentName)
        {
            return $":x: Component *{componentName}* failed Service Level Objective ({Id}) *{Name}*: {SLI.GetIndicatorMessage()}";
        }

        public override bool ObjectiveMet()
        {
            if (SLI == null)
            {
                return true;
            }

            return SLI.GetIndicator() == 0;
        }
    }
}
