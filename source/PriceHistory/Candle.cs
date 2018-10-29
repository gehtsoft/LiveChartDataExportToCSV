using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    /// <summary>
    /// Keeps data about one candle
    /// </summary>
    public class Candle
    {
        private DateTime mDate;
        private DateTime mDateOutput;
        private double mVolume;
        private double mBidOpen, mBidHigh, mBidLow, mBidClose;
        private double mAskOpen, mAskHigh, mAskLow, mAskClose;
        private bool mIsCompleted;
        private TimeframeUnit mTimeframeUnit;
        private int mPrecision;

        public bool IsCompleted
        {
            get
            {
                return mIsCompleted;
            }
        }

        public int Precision
        {
            get
            {
                return mPrecision;
            }
        }

        /// <summary>
        /// Date and time when the candle begins
        /// In the internal(server time zone)
        /// </summary>
        public DateTime Date
        {
            get
            {
                return mDate;
            }
        }

        /// <summary>
        /// Date and time when the candle begins in the output zone.
        /// </summary>
        public DateTime DateOutput
        {
            get
            {
                return mDateOutput;
            }
        }

        public TimeframeUnit Timeframe
        {
            get
            {
                return mTimeframeUnit;
            }
        }

        /// <summary>
        /// Open price of Bid candle
        /// </summary>
        public double BidOpen
        {
            get
            {
                return mBidOpen;
            }
        }

        /// <summary>
        /// High price of bid candle
        /// </summary>
        public double BidHigh
        {
            get
            {
                return mBidHigh;
            }
        }

        /// <summary>
        /// Low price of bid candle
        /// </summary>
        public double BidLow
        {
            get
            {
                return mBidLow;
            }
        }


        /// <summary>
        /// Close price of bid candle
        /// </summary>
        public double BidClose
        {
            get
            {
                return mBidClose;
            }
        }
        
        /// <summary>
        /// Open price of ask candle
        /// </summary>
        public double AskOpen
        {
            get
            {
                return mAskOpen;
            }
        }

        /// <summary>
        /// High price of ask candle
        /// </summary>
        public double AskHigh
        {
            get
            {
                return mAskHigh;
            }
        }


        /// <summary>
        /// Low price of ask candle
        /// </summary>
        public double AskLow
        {
            get
            {
                return mAskLow;
            }
        }

        /// <summary>
        /// Close price of ask candle
        /// </summary>
        public double AskClose
        {
            get
            {
                return mAskClose;
            }
        }

        /// <summary>
        /// Tick volume
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
        public Candle(DateTime date, //DateTime dateOutput, 
            double bidOpen, double bidHigh, double bidLow, double bidClose, 
            double askOpen, double askHigh, double askLow, double askClose, 
            double volume, bool isCompleted, Timeframe timeframe, int precision)
        {
            mDate = date;
           // mDateOutput = dateOutput;

            mBidOpen = bidOpen;           
            mBidHigh = bidHigh;
            mBidLow = bidLow;
            mBidClose = bidClose;
            
            mAskOpen = askOpen;
            mAskHigh = askHigh;
            mAskLow = askLow;
            mAskClose = askClose;

            mVolume = volume;

            mTimeframeUnit = timeframe.Unit;
            mIsCompleted = isCompleted;

            mPrecision = precision;
        }
        
        internal void UpdateCandle(double bid, double ask, double volume)
        {
            mBidClose = bid;
            if (bid > mBidHigh)
                mBidHigh = bid;
            if (bid < mBidLow)
                mBidLow = bid;
            mAskClose = ask;
            if (ask > mAskHigh)
                mAskHigh = ask;
            if (ask < mAskLow)
                mAskLow = ask;
            mVolume += volume;
        }

        internal void CloseCandle()
        {
            mIsCompleted = true;
        }
    }
}
