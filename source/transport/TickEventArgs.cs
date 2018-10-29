using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    /// <summary>
    /// Event arguments for the tick events
    /// </summary>
    public class TickEventArgs : EventArgs
    {
        string mInstrument;
        DateTime mTime;
        double mBid;
        double mAsk;
        double mVolume;
        
        
        /// <summary>
        /// Tick instrument
        /// </summary>
        public string Instrument 
        {
            get
            {
                return mInstrument;
            }
        }
        
        /// <summary>
        /// Tick time
        /// </summary>
        public DateTime Time
        {
            get
            {
                return mTime;
            }
        }
        
        /// <summary>
        /// Tick bid
        /// </summary>
        public double Bid
        {
            get
            {
                return mBid;
            }
        }
        
        /// <summary>
        /// Tick ask
        /// </summary>
        public double Ask
        {
            get
            {
                return mAsk;
            }
        }
        
        /// <summary>
        /// 1-minute accumulated tick volume
        /// </summary>
        public double Volume
        {
            get
            {
                return mVolume;
            }
        }
    
        /// <summary>
        /// Constructor
        /// </summary>
        internal TickEventArgs(string instrument, DateTime time, double bid, double ask, double volume)
        {
            mInstrument = instrument;
            mTime = time;
            mBid = bid;
            mAsk = ask;
            mVolume = volume;
        }
       
    }    

}
