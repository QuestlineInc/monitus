Monitus is a Windows service that monitors various aspects of distributed systems and notifies when configured criteria are met.

The specifics of what is to be monitored and how it will handle notifications are all configurable

	(Configuration lives in the ComponentConfig.xml file)

New functionality can be added by implementing new classes using the SLO abstract class or the SLI interface (ISLI).

***
	Comments can be found throughout the code that explain much of this as you step through the various classes,
	interfaces, and the main logic of the Monitus loop
***

GENERAL OVERVIEW:

	Service Level Objectives (SLO) and Service Level Indicators (SLI)

	These are the high-level logical patterns used to determine whether a notification will be sent
	
	SLOs have an SLI
	
	The SLO will call GetIndicator() on its SLI and then interpret that response according to the logic specified in the class
	
	(All of the specific implementations of SLOs inherit from the SLO base class)
	
	For example, the BooleanSLO will take it's SLI and check whether:
		sli.GetIndicator() == 0
	
	If this comes back true, then the Objective is considered met
	
	If not, the objective was not met, and it's considered a failure
	
	When an SLO fails it will notify using the XML-configured Notifier (a class that implements INotifier)

	If the SLO had previously failed, and ObjectiveMet() comes back true
	
	Then there will be a notification sent out that the SLO has passed and is no longer in a failed state
	
	(So you know that it is now operational, even though it failed earlier)

CREATE THE SERVICE:

	To install the Monitus service run this command on the host machine:

		sc create Monitus binpath={{path to Service.Monitus.exe on host machine}} start=demand DisplayName=Monitus
   
	Example:
		sc create Monitus binpath=C:\PublicMonitus\bin\Release\Service.Monitus.exe start=demand DisplayName=Monitus

	(Ensure whatever user is running the service can access the path provided)

EXAMPLE NOTIFICATIONS:
	If you hook up a CPU usage notification with a low value to test that the notifications are working	
	
	The message will look something like this:
		Component Server Performance failed Service Level Objective (5) Memory Usage: Memory usage is at 19.3%, 17.3 above target of 2
	
	(The configured id of the SLO is in parens after Service Level Objective)

	If an exception is thrown on an SLO it would look something like this:
		Component Web Applications failed Service Level Objective (2) Fake Website Responding Quickly: web request took -1 ms, an exception was thrown: The remote name could not be resolved: 'slow-loading-not-real-will-fail.com'





