using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    public class CandleHistoryFailedEventArgs : EventArgs
    {
        string mError;
        
        public string Error
        {
            get
            {
                return mError;
            }
        }
        
        internal CandleHistoryFailedEventArgs(string error)
        {
            mError = error;
        }
    }
}
