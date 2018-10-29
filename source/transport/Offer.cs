using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    public interface IOffer
    {
        string Instrument
        {
            get;
        }

        DateTime Time
        {
            get;
        }
        
        double Bid
        {
            get;
        }
        
        double Ask
        {
            get;
        }
        
        int Precision
        {
            get;
        }
        
        double PointSize
        {
            get;
        }
        
        double Volume
        {
            get;
        }
    }


    /// <summary>
    /// The class to keep offer data
    /// </summary>
    class Offer : IOffer
    {
        private string mInstrument;
        DateTime mTime;
        double mBid, mAsk;
        double mVolume;
        double mPointSize;
        int mPrecision;
        
        public string Instrument
        {
            get
            {
                return mInstrument;
            }
            set
            {
                mInstrument = value;
            }
        }
        
        public DateTime Time
        {
            get
            {
                return mTime;
            }
            set
            {
                mTime = value;
            }
        }
        
        public double Bid
        {
            get
            {
                return mBid;
            }
            set
            {
                mBid = value;
            }
        }
        
        public double Ask
        {
            get
            {
                return mAsk;
            }
            set
            {
                mAsk = value;
            }
        }
        
        public double Volume
        {
            get
            {
                return mVolume;
            }
            set
            {
                mVolume = value;
            }
        }
        
        public int Precision
        {
            get
            {
                return mPrecision;
            }
            set
            {
                mPrecision = value;
            }
        }
        
        public double PointSize
        {
            get
            {
                return mPointSize;
            }
            set
            {
                mPointSize = value;
            }
        }
        
        internal Offer(string instrument, DateTime time, double bid, double ask, double volume, int precision, double pointSize)
        {
            mInstrument = instrument;
            mTime = time;
            mBid = bid;
            mAsk = ask;
            mVolume = volume;
            mPrecision = precision;
            mPointSize = pointSize;
        }
    }
}
