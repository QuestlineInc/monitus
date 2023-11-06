using System;
using System.Xml;

namespace Service.Monitus.Core.SLOs
{
    /// <summary>
    /// Used to determine if the returned SLI indicator is within a specified range.
    /// Notification will be sent if the indicator is outside of the range.
    /// </summary>
    public class RangedSLO : SLO
    {
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }

        public RangedSLO(XmlTextReader reader) : base(reader)
        {
            reader.MoveToAttribute("lowerBound");
            LowerBound = double.Parse(reader.Value);
            reader.MoveToAttribute("upperBound");
            UpperBound = double.Parse(reader.Value);
        }

        public override string BuildFailureMessage(string componentName)
        {
            var failureInfo = $":x: Component *{componentName}* failed Service Level Objective ({Id}) *{Name}*: {SLI.GetIndicatorMessage()},";
            var tooLow = SLI.GetIndicator() < LowerBound;
            if (SLI.GetIndicator() == -1)
            {
                failureInfo += $" an exception was thrown: {SLI.GetMessage()}";
            }
            else if (tooLow)
            {
                failureInfo += $" {Math.Abs(SLI.GetIndicator() - LowerBound)} below desired range *{LowerBound}* - *{UpperBound}*";
            }
            else
            {
                failureInfo += $" {Math.Abs(SLI.GetIndicator() - UpperBound)} above desired range *{LowerBound}* - *{UpperBound}*";
            }
            return failureInfo;
        }

        public override bool ObjectiveMet()
        {
            if (SLI == null)
            {
                return true;
            }

            var indicator = SLI.GetIndicator();
            return indicator > LowerBound && indicator < UpperBound;
        }
    }
}

