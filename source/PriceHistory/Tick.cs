using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    /// <summary>
    /// Information about one tick
    /// </summary>
    public class Tick
    {
        static Timeframe mOneMinute = new Timeframe(TimeframeUnit.Minute, 1);
    
        private string mInstrument;
        private DateTime mDate;
        private DateTime mTickMinute;
        private double mBid, mAsk;
        private double mVolume;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public Tick(string instrument, DateTime date, double bid, double ask, double volume)
        {
            mInstrument = instrument;
            mDate = date;
            mTickMinute = mOneMinute.GetCandleOneMinute(date, 0, 0);
            mBid = bid;
            mAsk = ask;
            mVolume = volume;
        }
        
        /// <summary>
        /// Name of the instrument
        /// </summary>
        public string Instrument
        {
            get
            {
                return mInstrument;
            }
        }
        
        /// <summary>
        /// Date and time of the tick
        /// </summary>
        public DateTime Date
        {
            get
            {
                return mDate;
            }
        }
        
        public DateTime TickMinute
        {
            get
            {
                return mTickMinute;
            }
        }
        
        /// <summary>
        /// Bid price of the tick
        /// </summary>
        public double Bid
        {
            get
            {
                return mBid;
            }
        }
        
        /// <summary>
        /// Ask price of the tick
        /// </summary>
        public double Ask
        {
            get
            {
                return mAsk;
            }
        }
        
        /// <summary>
        /// Accumulated tick volume of the tick's minute.
        /// </summary>
        public double Volume
        {
            get
            {
                return mVolume;
            }
        }
    }
}
