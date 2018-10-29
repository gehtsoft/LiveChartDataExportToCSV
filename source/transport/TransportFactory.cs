using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    public class TransportFactory
    {
        public static ITransport CreateForexConnectTransport(string user, string password, string url, string connection, string timezone)
        {
            return new ForexConnectTransport(user, password, url, connection, timezone);
        }
    }
}
