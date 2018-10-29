using System;
using System.Collections.Generic;
using System.Text;

namespace LiveChartDataExportToCSV
{
    public class RequestCompletedEventArgs : EventArgs
    {
        public string RequestID
        {
            get
            {
                return mRequestID;
            }
        }
        string mRequestID;

        internal RequestCompletedEventArgs(string requestID)
        {
            mRequestID = requestID;
        }
    }
}
