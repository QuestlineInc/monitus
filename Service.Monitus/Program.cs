using System.Net;
using System.ServiceProcess;

namespace Service.Monitus
{
	static class Program
	{
		static void Main()
		{
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServiceBase.Run(new MonitusService());
		}
	}
}
