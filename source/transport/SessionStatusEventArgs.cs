using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    /// <summary>
    /// Event argument for the session status event
    /// </summary>
    public class SessionStatusEventArgs : EventArgs
    {
        bool mConnected;
        string mName;
        
        /// <summary>
        /// The flag indicating whether the transport is connected and can be used to execute requests
        /// </summary>
        public bool Connected
        {
            get
            {
                return mConnected;
            }
        }
        
        /// <summary>
        /// The name of the status.
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
        }
        
        internal SessionStatusEventArgs(bool connected, string name)
        {
            mConnected = connected;
            mName = name;
        }
    }
}
