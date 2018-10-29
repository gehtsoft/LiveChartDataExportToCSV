using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    public class CandleHistoryCandleUpdatedEventArgs : EventArgs
    {
        Candle mCandle;
        
        public Candle Candle
        {
            get
            {
                return mCandle;
            }
        }
        
        internal CandleHistoryCandleUpdatedEventArgs(Candle candle)
        {
            mCandle = candle;
        }
    }
}
