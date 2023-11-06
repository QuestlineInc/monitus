using System;
using System.Linq;
using System.Xml;

namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// Used to determine the number of suspended queries in a Database with 0 CPU time currently active.
    /// </summary>
    public class DBSuspendedQueriesSLI : ISLI
    {
        private string _message { get; set; }
        private double? _indicator { get; set; }
        public string DatabaseName { get; set; }

        public DBSuspendedQueriesSLI(XmlTextReader reader)
        {
            reader.MoveToAttribute("databaseName");
            DatabaseName = reader.Value;
        }

        public double GetIndicator()
        {
            if (_indicator.HasValue)
                return _indicator.Value;
            
            _indicator = 0.00;
            try
            {
                // below is an example using SQL Server syntax and the PetaPoco library
                // replace with your own version of SQL and your preferred library

                throw new NotImplementedException();

                //var tooManySuspended = false;
                //var myDB = new Database(DatabaseName);
                //myDB.CommandTimeout = 10000;
                //var querySql = @"
                //SELECT req.status as Status,
                //       req.cpu_time as CpuTime
                //FROM sys.dm_exec_requests req
                //CROSS APPLY sys.dm_exec_sql_text(sql_handle) AS SqlText;";

                //// check if there are more than MaxSuspended suspended queries with cpu time == 0
                //var queries = myDB.Fetch<RunningQuery>(querySql);
                //_indicator = queries.Count(x => x.CpuTime == 0 && x.Status == "suspended");

                //if (tooManySuspended)
                //{
                //    _indicator = 1.00;
                //}
            }
            catch (Exception ex)
            {
                _message = ex.Message;
                _indicator = -1.00;
            }

            return _indicator.Value;
        }

        public string GetMessage()
        {
            return _message;
        }

        public string GetIndicatorMessage()
        {
            return $"*{GetIndicator()}* suspended queries with 0 CPU time currently active";
        }

        private class RunningQuery
        {
            public string Text { get; set; }
            public int SessionId { get; set; }
            public string Status { get; set; }
            public string Command { get; set; }
            public int CpuTime { get; set; }
            public int TotalElapsedTime { get; set; }
        }
    }
}
