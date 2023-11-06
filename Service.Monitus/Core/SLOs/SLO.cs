using Service.Monitus.Core.Notifiers;
using Service.Monitus.Core.Reporters;
using Service.Monitus.Core.SLIs;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Service.Monitus.Core.SLOs
{
    /// <summary>
    /// The abstract class for Service Level Objectives, where common logic is defined.
    /// All SLOs have and Id and a Name, and can have a StartIgnore and EndIgnore time.
    /// </summary>
    public abstract class SLO
    {
        public SLO(XmlTextReader reader)
        {
            Notifiers = new List<INotifier>();
            Reporters = new List<Reporter>();
            reader.MoveToAttribute("id");
            Id = Convert.ToInt32(reader.Value);
            reader.MoveToAttribute("name");
            Name = reader.Value;
            if (reader.MoveToAttribute("startIgnore"))
                StartIgnore = Convert.ToInt32(reader.Value);
            if (reader.MoveToAttribute("endIgnore"))
                EndIgnore = Convert.ToInt32(reader.Value);
        }
        public List<INotifier> Notifiers { get; set; }
        public List<Reporter> Reporters { get; set; }
        public ISLI SLI { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public int? StartIgnore { get; set; }
        public int? EndIgnore { get; set; }
        public abstract bool ObjectiveMet();
        public abstract string BuildFailureMessage(string componentName);
        public virtual string BuildSuccessMessage(string componentName)
        {
            return $":heavy_check_mark: Component *{componentName}* has now successfully passed Service Level Objective *{Name}*";
        }
    }
}
