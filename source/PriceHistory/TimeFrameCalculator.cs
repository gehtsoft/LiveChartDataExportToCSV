using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{

    public class TimeFrameCalculator
    {
        private int mDayOffset;
        private int mWeekOffset;

        private const int SECONDS_IN_DAY = 86400;
        private const int MINUTES_IN_DAY = 1440;
        private const int SECONDS_IN_HOUR = 3600;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dayoffset">Offset of the beginning of the trading day against midnight, expressed in hours</param>
        /// <param name="weekoffset">Offset of the beginning of the trading week agains Sunday, expressed in days</param>
        public TimeFrameCalculator(int dayoffset, int weekoffset)
        {
            mDayOffset = dayoffset;
            mWeekOffset = weekoffset;
        }
        
        public DateTime GetCandle(Timeframe timeframe, DateTime time)
        {
            return timeframe.GetCandle(time, mDayOffset, mWeekOffset);
        }
    }
}
