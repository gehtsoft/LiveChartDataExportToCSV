using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    
    /// <summary>
    /// Record about a collection inside the candle manager
    /// </summary>
    internal class CandleManagerHistory : ITransportHistoryReader
    {
        private CandleHistory mHistory;
        private LinkedList<Tick> mTicks;
        private bool mWaitForData;
        private bool mSubscribe;
        private bool mFailed;
        private DateTime mLastMinute;
        private double mLastMinuteVolume;
        private Timeframe mTimeframe;
        private int mPrecision;
        
        internal CandleManagerHistory(TransportHistoryRequest request, bool subscribe, TimeFrameCalculator calculator, int precision)
        {
            mPrecision = precision;
            mTimeframe = Timeframe.Parse(request.Timeframe);
            mHistory = new CandleHistory(request.Instrument, mTimeframe, calculator, mPrecision);
            if (subscribe)
                mTicks = new LinkedList<Tick>();
            mWaitForData = true;
            mSubscribe = subscribe;
            mFailed = false;
            mCount = request.Count;
            mLastMinute = DateTime.MinValue;
        }

        internal int Count
        {
            get
            {
                return mCount;
            }
        }
        int mCount;
        
        /// <summary>
        /// Process a tick
        /// </summary>
        internal void OnTick(Tick tick)
        {
            if (tick.Instrument == mHistory.Instrument && mSubscribe)
            {
                //if a tick belongs the the same instrument

                Console.WriteLine("{0} {1} ask:{2} bid:{3} volume:{4}", tick.Instrument, tick.Date, tick.Ask, tick.Bid, tick.Volume);

                if (mWaitForData)
                {
                    //keep to for further processing 
                    //when history is loaded
                    if (mTicks != null)
                    {
                        mTicks.AddLast(tick);
                    }
                }
                else
                {
                    //if collection is alive - process it immediatelly
                    mHistory.AddTick(tick);
                }
            }
        }

        /// <summary>
        /// The history row parsed. The method will be called for each row of the response
        /// from the oldest to newest
        /// </summary>
        void ITransportHistoryReader.onHistoryRow(string requestID, DateTime barDate,
                                                  double bidopen, double bidhigh, double bidlow, double bidclose,
                                                  double askopen, double askhigh, double asklow, double askclose,
                                                  double volume, int position)
        {
            if (mHistory.Count > position && DateTime.Compare(barDate, mHistory[position].Date) >= 0)
            {
                mHistory.RemoveAt(position);
            }
            mHistory.AddCandle(barDate, bidopen, bidhigh, bidlow, bidclose, askopen, askhigh, asklow, askclose, volume, true, position, mTimeframe);
        }

        /// <summary>
        /// All rows are parsed
        /// </summary>
        /// <param name="lastMinute">The minute of the last tick used to build the history on the server</param>
        /// <param name="lastMinuteVolume">The accumulated tick volume of the last minute</param>
        public void onHistoryFinished()
        {
            mHistory.SetLastVolume(mLastMinute, mLastMinuteVolume);

            // last historical candle is incompleted
            if (mHistory.Count > 0)
            {
                Candle lastCandle = mHistory[mHistory.Count - 1];
                Candle newCandle = new Candle(lastCandle.Date, lastCandle.BidOpen, lastCandle.BidHigh, lastCandle.BidLow, lastCandle.BidClose, lastCandle.AskOpen, lastCandle.AskHigh, lastCandle.AskLow, lastCandle.AskClose, lastCandle.Volume, false, mTimeframe, mPrecision);
                mHistory.RemoveAt(mHistory.Count - 1);
                mHistory.AddCandle(newCandle, mHistory.Count);
            }

            if (mTicks != null)
            {
                foreach (Tick tick in mTicks)
                    mHistory.AddTick(tick);
                mTicks.Clear();
                mTicks = null;
            }

            mWaitForData = false;
        }
        
        internal bool NeedToRemove
        {
            get
            {
                return (!mWaitForData && !mSubscribe) || (mFailed);
            }
        }
        
        internal CandleHistory History
        {
            get
            {
                return mHistory;
            }
        }

        #region ITransportHistoryReader Members

        void ITransportHistoryReader.setLastBar(DateTime lastMinute, double lastMinuteVolume)
        {
            if (DateTime.Compare(lastMinute, mLastMinute) >= 0)
            {
                mLastMinute = lastMinute;
                mLastMinuteVolume = lastMinuteVolume;
            }
        }

        void ITransportHistoryReader.onHistoryFailed(string requestID, string error)
        {
            mFailed = true;
            mWaitForData = false;
            mHistory.LoadFailed(error);
        }

        #endregion
    }
}
