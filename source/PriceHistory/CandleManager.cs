using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    public class CandleManager
    {
        ITransport mTransport;

        Dictionary<string, CandleManagerHistory> mHistories = new Dictionary<string, CandleManagerHistory>();
        //LinkedList<CandleManagerHistory> mHistories = new LinkedList<CandleManagerHistory>();


        TimeFrameCalculator mCalculator = null;
        object mMutex = new object();

        //private int mCount = 300;
        //private string mInstrument = "";
        //private string mTimeframe = "";
        //private CandleManagerHistory mHistory = null;
        
        public CandleManager(ITransport transport, int dayOffset, int weekOffset)
        {
            mTransport = transport;
            mTransport.OnTick += OnTick;
            mTransport.OnRequestCompleted += new EventHandler<RequestCompletedEventArgs>(mTransport_OnRequestCompleted);
            mCalculator = new TimeFrameCalculator(dayOffset, weekOffset);
        }

        void mTransport_OnRequestCompleted(object sender, RequestCompletedEventArgs e)
        {
            CandleManagerHistory hist;
            string key = e.RequestID;
            if (mHistories.TryGetValue(key, out hist))
            {
                if (hist.History.Count < hist.Count)
                {
                    DateTime firstDate = hist.History[0].Date;
                    // request n+1 bars
                    GetHistory(hist.History.Instrument, hist.History.Timeframe.Name, DateTime.FromOADate(0), firstDate, hist.Count - hist.History.Count + 1, true);
                }
                else
                {
                    lock (mMutex)
                    {
                        hist.onHistoryFinished();
                        hist.History.LoadFinished();
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the default number of the bars and subscribe the history
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="timeframe"></param>
        /// <returns></returns>
        public CandleHistory GetHistory(string instrument, string timeframe)
        {
            return GetHistory(instrument, timeframe, DateTime.FromOADate(0), DateTime.Now, 300, true);
        }

        public CandleHistory GetHistory(string instrument, string timeframe, int count)
        {
            return GetHistory(instrument, timeframe, DateTime.FromOADate(0), DateTime.FromOADate(0), count, true);
        }

        private CandleHistory GetHistory(string instrument, string timeframe, DateTime from, DateTime to, int count, bool subscribe)
        {
            lock (mMutex)
            {
                if (mTransport.IsReady())
                {
                    TransportHistoryRequest rq = new TransportHistoryRequest(instrument, timeframe, from, to, count);
                    CandleManagerHistory hist;
                    string key = rq.ID;
                    if(!mHistories.TryGetValue(key, out hist))
                    {
                        hist = new CandleManagerHistory(rq, subscribe, mCalculator, mTransport.GetInstrumentPrecision(instrument));
                        mHistories.Add(key, hist);
                    }
                    rq.Reader = hist;
                    mTransport.RequestHistory(rq);
                    return hist.History;
                }
                else
                {
                    throw new InvalidOperationException("Transport is not in the proper state");
                }
            }
        }

        void OnTick(object sender, TickEventArgs args)
        {
            lock (mMutex)
            {
                if (mHistories.Count > 0)
                {
                    Tick tick = new Tick(args.Instrument, args.Time, args.Bid, args.Ask, args.Volume);

                    List<CandleManagerHistory> toDelete = new List<CandleManagerHistory>();
                    foreach (string key in mHistories.Keys)
                    {
                        if (key.StartsWith(args.Instrument))
                        {
                            if (mHistories[key].NeedToRemove)
                            {
                                toDelete.Add(mHistories[key]);
                            }
                            else
                            {
                                mHistories[key].OnTick(tick);
                            }
                        }
                    }
                }
            }
        }
    }
}
