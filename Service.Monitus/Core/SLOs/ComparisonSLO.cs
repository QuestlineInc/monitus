using System;
using System.Xml;

namespace Service.Monitus.Core.SLOs
{
    /// <summary>
    /// Used to define a Service Level Objective that is a comparison between the SLI and a target value.
    /// The comparison is either greater than or less than the target value, as configured in the XML.
    /// Getting back -1 from the SLI indicates an exception was thrown.
    /// </summary>
    public class ComparisonSLO : SLO
    {
        public double Target { get; set; }
        public bool IsGreaterThan { get; set; }

        public ComparisonSLO(XmlTextReader reader) : base(reader)
        {
            reader.MoveToAttribute("target");
            Target = double.Parse(reader.Value);
            reader.MoveToAttribute("isGreaterThan");
            IsGreaterThan = bool.Parse(reader.Value);
        }

        public override string BuildFailureMessage(string componentName)
        {
            var failureInfo = $":x: Component *{componentName}* failed Service Level Objective ({Id}) *{Name}*: {SLI.GetIndicatorMessage()},";
            var diff = Math.Abs(SLI.GetIndicator() - Target);
            if (SLI.GetIndicator() == -1)
            {
                failureInfo += $" an exception was thrown: {SLI.GetMessage()}";
            }
            else if (IsGreaterThan)
            {
                failureInfo += $" {diff} below target of *{Target}*";
            }
            else
            {
                failureInfo += $" {diff} above target of *{Target}*";
            }
            return failureInfo;
        }

        public override bool ObjectiveMet()
        {
            if (SLI == null)
            {
                return true;
            }
            if (SLI.GetIndicator() == -1)
            {
                return false;
            }

            if(IsGreaterThan)
            {
                return SLI.GetIndicator() > Target;
            }
            return SLI.GetIndicator() < Target;
        }
    }
}
