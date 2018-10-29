using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    /// <summary>
    /// The interface to read the history request results
    /// </summary>
    public interface ITransportHistoryReader
    {
        /// <summary>
        /// The history row parsed. The method will be called for each row of the response
        /// from the oldest to newest
        /// </summary>
        void onHistoryRow(string requestID, DateTime bar, 
                          double bidopen, double bidhigh, double bidlow, double bidclose, 
                          double askopen, double askhigh, double asklow, double askclose,
                          double volume, int position);

        void setLastBar(DateTime lastMinute, double lastMinuteVolume);
        
        /// <summary>
        /// History request failed
        /// </summary>
        void onHistoryFailed(string requestID, string error);
    }
}
