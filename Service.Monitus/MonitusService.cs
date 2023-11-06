using Service.Monitus.Core.Components;
using Service.Monitus.Core.Notifiers;
using Service.Monitus.Core.SLIs;
using Service.Monitus.Core.SLOs;
using System;
using System.ServiceProcess;
using System.Timers;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using Service.Monitus.Core.Reporters;

namespace Service.Monitus
{
	public partial class MonitusService : ServiceBase
	{        
         // Command to create local service
         //   sc create Monitus binpath={{path to Service.Monitus.exe on host machine}} start=demand DisplayName=Monitus    
        private static Dictionary<string, DateTime> _failures = new Dictionary<string, DateTime>();
		private Timer _timer;
        private int _sloStaleTimeInMinutes = 15;
        private int _runIntervalInSeconds = 120;

		public MonitusService()
		{
			InitializeComponent();
            EventLog.Source = "MonitusService";
        }
		protected override void OnStart(string[] args)
		{
			EventLog.WriteEntry("Service.Monitus: Starting");

			var intervalInMilliseconds = _runIntervalInSeconds * 1000;

			_timer = new Timer(intervalInMilliseconds);
			_timer.Elapsed += Timer_Elapsed;
			_timer.Start();
            // ^ Runs immediately on service start
		}

		protected override void OnStop()
		{
			EventLog.WriteEntry("Service.Monitus: Stopping");
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs args)
		{
			try
			{
                var componentList = InitializeComponentList();

                // if any SLOs failed more than _sloStaleTime minutes ago, remove them from the collection
                //  so we get another failure notification (we don't want silence too long)
                _failures = _failures.Where(x => x.Value.AddMinutes(_sloStaleTimeInMinutes) > DateTime.UtcNow)
                                     .ToDictionary(t => t.Key, t => t.Value);

                foreach (var component in componentList)
                {
                    foreach (var slo in component.SLOs)
                    {
                        // always report the results of the SLO
                        ReportResults(slo);

                        // make sure we aren't in the ignore notification period of the SLO before proceeding                        
                        if (HasIgnorePeriod(slo) && InIgnorePeriod(slo))
                        {
                            continue;
                        }
                        else if (!slo.ObjectiveMet() && !_failures.ContainsKey(slo.Name))
                        {
                            // if the SLO failed and it is not in the Failures collection then add it and notify
                            _failures.Add(slo.Name, DateTime.UtcNow);
                            NotifyFailure(slo, component.Name);
                        }
                        else if (slo.ObjectiveMet() && _failures.ContainsKey(slo.Name))
                        {
                            // if the SLO passed this time and is in the Failures collection from a previous
                            //  failed run then remove it and notify that it has passed
                            _failures.Remove(slo.Name);
                            NotifySuccess(slo, component.Name);
                        }
                    }
                }
            }
			catch (Exception ex)
			{
				EventLog.WriteEntry("Service.Monitus: " + ex.Message + " ---\n" + ex.StackTrace);
			}
		}

        private static List<Component> InitializeComponentList()
        {
            var components = new List<Component>();
            var currentComponent = -1;
            var currentSLO = -1;

            // Needs to reset directory to directory I'm running from or else it remains as- c:\windows\system32
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            // Load up the ComponentConfig.xml file and walk through the nodes
            var reader = new XmlTextReader("ComponentConfig.xml");

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "Component")
                        {
                            components.Add(new Component(reader));
                            currentComponent++;
                            currentSLO = -1;
                        }
                        else if (reader.Name == "SLO")
                        {
                            AddSLO(components[currentComponent], reader);
                            currentSLO++;
                        }
                        else if (reader.Name == "Notifier")
                        {
                            AddNotifier(components[currentComponent].SLOs[currentSLO], reader);
                        }
                        else if (reader.Name == "Reporter")
                        {
                            AddReporter(components[currentComponent].SLOs[currentSLO], reader);
                        }
                        else if (reader.Name == "SLI")
                        {
                            SetSLI(components[currentComponent].SLOs[currentSLO], reader);
                        }
                        break;
                }
            }

            return components;
        }

        private static void SetSLI(SLO slo, XmlTextReader reader)
        {
            reader.MoveToAttribute("type");
            var sliType = reader.Value;

            var sliToCreate = Type.GetType($"Service.Monitus.Core.SLIs.{sliType}SLI");
            var sli = Activator.CreateInstance(sliToCreate, reader) as ISLI;

            slo.SLI = sli;
        }

        private static void AddNotifier(SLO slo, XmlTextReader reader)
        {
            reader.MoveToAttribute("type");
            var notifierType = reader.Value;

            var notifierToCreate = Type.GetType($"Service.Monitus.Core.Notifiers.{notifierType}Notifier");
            var notifier = Activator.CreateInstance(notifierToCreate) as INotifier;

            slo.Notifiers.Add(notifier);
        }

        private static void AddReporter(SLO slo, XmlTextReader reader)
        {
            reader.MoveToAttribute("type");
            var reporterType = reader.Value;

            var reporterToCreate = Type.GetType($"Service.Monitus.Core.Reporters.{reporterType}Reporter");
            var reporter = Activator.CreateInstance(reporterToCreate, reader) as Reporter;

            slo.Reporters.Add(reporter);
        }

        private static void AddSLO(Component component, XmlTextReader reader)
        {
            reader.MoveToAttribute("type");
            var sloType = reader.Value;

            var sloToCreate = Type.GetType($"Service.Monitus.Core.SLOs.{sloType}SLO");
            var slo = Activator.CreateInstance(sloToCreate, reader) as SLO;

            component.SLOs.Add(slo);
        }

        private static void NotifyFailure(SLO slo, string componentName)
        {
            var failureMessage = slo.BuildFailureMessage(componentName);
            foreach (var notifier in slo.Notifiers)
            {
                notifier.Notify(failureMessage);
            }
        }

        private static void NotifySuccess(SLO slo, string componentName)
        {
            var successMessage = slo.BuildSuccessMessage(componentName);
            foreach (var notifier in slo.Notifiers)
            {
                notifier.Notify(successMessage);
            }
        }

        private bool HasIgnorePeriod(SLO slo)
        {
            return slo.StartIgnore.HasValue && slo.EndIgnore.HasValue;
        }

        private static bool InIgnorePeriod(SLO slo)
        {
            var currentHour = DateTime.UtcNow.Hour;
            var inIgnorePeriod = false;

            if (slo.StartIgnore > slo.EndIgnore)
            {
                // if the start hour is greater than the end hour then this spans two days
                // ex: currentHour = 20, StartIgnore = 18, EndIgnore = 6 => true
                //     currentHour = 12, StartIgnore = 20, EndIgnore = 7 => false
                inIgnorePeriod = (currentHour >= slo.StartIgnore) || (currentHour < slo.EndIgnore);
            }
            else
            {
                // otherwise the window is in a single day
                // ex: currentHour = 8, StartIgnore = 2, EndIgnore = 10 => true
                //     currentHour = 16, StartIgnore = 1, EndIgnore = 8 => false
                inIgnorePeriod = (currentHour >= slo.StartIgnore) && (currentHour < slo.EndIgnore);
            }

            return inIgnorePeriod;
        }

        private static void ReportResults(SLO slo)
        {
            foreach (var reporter in slo.Reporters)
            {
                reporter.Report(slo.SLI.GetIndicator());
            }
        }
    }
}
