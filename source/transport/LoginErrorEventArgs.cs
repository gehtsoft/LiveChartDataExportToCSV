using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    public class LoginErrorEventArgs : EventArgs 
    {
        string mError;
        
        public string Error
        {
            get
            {
                return mError;
            }
        }
        
        internal LoginErrorEventArgs(string error)
        {
            mError = error;
        }
    }
}
