using Service.Monitus.Core.SLOs;
using System.Collections.Generic;
using System.Xml;

namespace Service.Monitus.Core.Components
{
    /// <summary>
    /// A Component represents a piece of any system that you want to monitor for status.
    /// It is used to group related SLOs together.
    /// For example, a component might be concerned with the status of a particular server or database.
    /// </summary>
    public class Component
    {
        public List<SLO> SLOs { get; set; }
        public string Name { get; set; }

        public Component(XmlTextReader reader)
        {
            SLOs = new List<SLO>();
            reader.MoveToAttribute("name");
            Name = reader.Value;
        }
    }
}
