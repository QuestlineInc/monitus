using System;
using System.Xml;

namespace Service.Monitus.Core.SLIs
{
    /// <summary>
    /// Used to determine if there are any long running queries on a particular database.
    /// </summary>
    public class DBLongRunningQueriesSLI : ISLI
    {
        private double? _indicator { get; set; }
        private string _message { get; set; }
        public string DatabaseName { get; set; }
        private string _queryText { get; set; }

        public DBLongRunningQueriesSLI(XmlTextReader reader)
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
                //var myDB = new Database(DatabaseName);
                //myDB.CommandTimeout = 10000;
                //var querySql = @"
                //SELECT req.total_elapsed_time as TotalElapsedTime,
                //sqltext.TEXT as QueryText
                //FROM sys.dm_exec_requests req
                //CROSS APPLY sys.dm_exec_sql_text(sql_handle) AS SqlText;";

                //// Grabs the max elapsed time from a list of running queries
                //var queries = myDB.Fetch<QueryInfo>(querySql);
                //var firstResult = queries.OrderByDescending(x => x.TotalElapsedTime).First();
                //_indicator = firstResult?.TotalElapsedTime;
                //_queryText = firstResult?.QueryText;
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
            return $"a query has been running for *{GetIndicator()} ms*. The query was: *{_queryText}*";
        }

        private class QueryInfo
        {
            public int TotalElapsedTime { get; set; }
            public string QueryText { get; set; }
        }
    }
}
