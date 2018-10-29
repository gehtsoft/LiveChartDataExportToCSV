using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    /// <summary>
    /// Collection of the candles
    /// </summary>
    public class CandleHistory : IEnumerable<Candle>
    {
        private List<Candle> mCandles = new List<Candle>();
        private string mInstrument;
        private Timeframe mTimeframe;
        private DateTime mLastMinute;
        private double mLastMinuteVolume;
        private TimeFrameCalculator mCalculator;
        private bool mLoaded;
        private string mID;
        private int mPrecision;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public CandleHistory(string instrument, 
            Timeframe timeframe, TimeFrameCalculator calculator, int precision)
        {
            mInstrument = instrument;
            mTimeframe = timeframe;
            mLastMinute = new DateTime(0);
            mLastMinuteVolume = 0;
            mCalculator = calculator;
            mLoaded = false;
            mID = (Guid.NewGuid().ToString());
            mPrecision = precision;
        }

        /// <summary>
        /// Candle History unique identifier
        /// </summary>
        public string ID
        {
            get
            {
                return mID;
            }
        }

        /// <summary>
        /// Instrument name
        /// </summary>
        public string Instrument
        {
            get
            {
                return mInstrument;
            }
        }

        /// <summary>
        /// Collection time frame
        /// </summary>
        public Timeframe Timeframe
        {
            get
            {
                return mTimeframe;
            }
        }
        
        /// <summary>
        /// Number of the candles 
        /// </summary>
        public int Count
        {
            get
            {
                return mCandles.Count;
            }
        }
        
        /// <summary>
        /// Candle by the index (oldest candle has index 0).
        /// </summary>
        public Candle this[int index] 
        {
            get
            {
                return mCandles[index];
            }
        }
        
        IEnumerator<Candle> IEnumerable<Candle>.GetEnumerator()
        {
            return mCandles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mCandles.GetEnumerator();
        }
        
        /// <summary>
        /// Add historical candle
        /// </summary>
        public void AddCandle(DateTime date, double bidopen, double bidhigh, double bidlow, double bidclose,
                                             double askopen, double askhigh, double asklow, double askclose,
                                             double volume, bool isCompleted, int position, Timeframe timeframe)
        {
            mCandles.Insert(position, new Candle(date, bidopen, bidhigh, bidlow, bidclose, askopen, askhigh, asklow, askclose, volume, isCompleted, timeframe, mPrecision));
        }

        public void AddCandle(Candle candle, int position)
        {
            mCandles.Insert(position, candle);
        }

        public void RemoveAt(int index)
        {
            mCandles.RemoveAt(index);
        }

        /// <summary>
        /// Add tick update to the candle
        /// </summary>
        public void AddTick(Tick tick)
        {
            //get date/time of the candle to which the tick belongs
            DateTime tickCandle = mCalculator.GetCandle(mTimeframe, tick.Date);
            Candle candle;
            
            //if there is no candles yet
            //(there shall not be such situation, mostly that means
            //that the previous get history request has been failed!)
            if (mCandles.Count == 0)
            {
                //just create a new candle on the base of the tick
                candle = new Candle(tickCandle, tick.Bid, tick.Bid, tick.Bid, tick.Bid, tick.Ask, tick.Ask, tick.Ask, tick.Ask, tick.Volume, false, mTimeframe, mPrecision);
                mCandles.Add(candle);
                mLastMinute = tick.TickMinute;
                mLastMinuteVolume = tick.Volume;
            }
            else
            {
                //else take the latest candle we have
                candle = mCandles[mCandles.Count - 1];
            }
            
            if (candle.Date < tickCandle)
            {
                //if candle is older than the candle
                //of the tick
                //create a new candle

                candle.CloseCandle();
                if (OnUpdated != null && mLoaded)
                {
                    CandleHistoryCandleUpdatedEventArgs args = new CandleHistoryCandleUpdatedEventArgs(candle);
                    OnUpdated(this, args);
                }
                

                Candle newcandle = new Candle(tickCandle, candle.BidClose, Math.Max(tick.Bid, candle.BidClose), Math.Min(tick.Bid, candle.BidClose), tick.Bid,
                                                candle.AskClose, Math.Max(tick.Ask, candle.AskClose), Math.Min(tick.Ask, candle.AskClose), tick.Ask,
                                                tick.Volume, false, mTimeframe, mPrecision);
                mCandles.Add(newcandle);
                mLastMinute = tick.TickMinute;
                mLastMinuteVolume = tick.Volume;

            }
            else if (candle.Date == tickCandle)
            {
                //or if tick belongs to the candle
                //update the current candle using ticks.
                
                //the most complex thing here is how to update the tick volume.
                
                //Problem 1) Some ticks may be filtered off
                //Problem 2) In the situation when the historical collection 
                //is subscribed on the server and then we add ticks - we must
                //exactly know how many ticks was added into the tick volume
                //
                //to solve both problems:
                //1) each tick contains the accumulating tick volume for the current minute
                //2) the history contains the minute and the volume of the last
                //   tick minute used to collect the data into the history.
                //please also note that the server guarntees delivering 
                //or the high, low and close ticks of each minute. 

                bool ignore = false;
                double volume = 0;
                
                if (tick.TickMinute == mLastMinute)
                {
                    //if this ticks belongs to the same minute as already processed
                    //find change of the volume since the last tick
                    //and add it to the candle
                    volume = tick.Volume - mLastMinuteVolume;
                    mLastMinuteVolume = tick.Volume;
                }
                else if (tick.TickMinute > mLastMinute)
                {
                    //if this ticks belongs to the next minute
                    //(i.e. it is the first tick of that minute
                    //add all accumulated volume and then 
                    //keep the data for further updates in the same minute
                    volume = tick.Volume;
                    mLastMinute = tick.TickMinute;
                    mLastMinuteVolume = tick.Volume;
                }
                else
                {
                    //ignore tick, it's older than the last minute tick we (really-server) already 
                    //processed
                    ignore = true;
                }
                
                if (!ignore)
                {
                    candle.UpdateCandle(tick.Bid, tick.Ask, volume);

                    if (OnUpdated != null && mLoaded)
                    {
                        CandleHistoryCandleUpdatedEventArgs args = new CandleHistoryCandleUpdatedEventArgs(candle);
                        OnUpdated(this, args);
                    }
                }
            }
        }
        
        /// <summary>
        /// Set the date/time of the last minute and its volume
        /// used to calculate this collection on the server
        /// </summary>
        public void SetLastVolume(DateTime minute, double volume)
        {
            mLastMinuteVolume = volume;
            mLastMinute = minute;
        }
        
        public event EventHandler<CandleHistoryLoadedEventArgs> OnLoaded;
        public event EventHandler<CandleHistoryFailedEventArgs> OnFailed;
        public event EventHandler<CandleHistoryCandleUpdatedEventArgs> OnUpdated;
        
        internal void LoadFinished()
        {
            mLoaded = true;
            if (OnLoaded != null)
            {
                CandleHistoryLoadedEventArgs args = new CandleHistoryLoadedEventArgs();
                OnLoaded(this, args);
            }
        }
        
        internal void LoadFailed(string error)
        {
            if (OnFailed != null)
            {
                CandleHistoryFailedEventArgs args = new CandleHistoryFailedEventArgs(error);
                OnFailed(this, args);
            }
        }
    }
}
