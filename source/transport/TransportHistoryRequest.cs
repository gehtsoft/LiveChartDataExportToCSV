using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    /// <summary>
    /// History request
    /// </summary>
    public class TransportHistoryRequest
    {
        ITransportHistoryReader mReader;
        string mInstrument;
        string mTimeFrame;
        DateTime mFrom;
        DateTime mTo;
        int mCount;
        string mID;
        
        public TransportHistoryRequest(string instrument, string timeframe)
        {
            mInstrument = instrument;
            mTimeFrame = timeframe;
            mFrom = DateTime.FromOADate(0);
            mTo = DateTime.FromOADate(0);
            mCount = 300;
        }
        
        public TransportHistoryRequest(string instrument, string timeframe, int count)
        {
            mInstrument = instrument;
            mTimeFrame = timeframe;
            mFrom = DateTime.FromOADate(0);
            mTo = DateTime.FromOADate(0);
            mCount = count;
            //if (mCount > 300)
            //{
            //    mCount = 300;
            //}
            mID = GetKey(instrument, timeframe);
        }

        public TransportHistoryRequest(string instrument, string timeframe, DateTime from, DateTime to, int count)
        {
            mInstrument = instrument;
            mTimeFrame = timeframe;
            mFrom = from;
            mTo = to;
            mCount = count;
            //if (mCount > 300)
            //{
            //    mCount = 300;
            //}
            mID = GetKey(instrument, timeframe);
        }

        string GetKey(string instrument, string timeframe)
        {
            return string.Format("{0}_{1}", instrument, timeframe);
        }
        
        internal ITransportHistoryReader Reader
        {
            get
            {
                return mReader;
            }
            set
            {
                mReader = value;
            }
        }
        
        internal string Instrument
        {
            get
            {
                return mInstrument;
            }
        }
        
        internal string Timeframe
        {
            get
            {
                return mTimeFrame;
            }
        }
        
        internal DateTime From
        {
            get
            {
                return mFrom;
            }
        }
        
        internal DateTime To
        {
            get
            {
                return mTo;
            }
        }
        
        internal int Count
        {
            get
            {
                return mCount;
            }
        }
        
        public string ID
        {
            get
            {
                return mID;
            }
        }
    }
}
