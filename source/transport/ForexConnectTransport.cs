using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fxcore2;

namespace LiveChartDataExportToCSV
{

    class ForexConnectTimeConverter: IDateTimeConverter
    {
        O2GSession mSession;
        O2GTimeConverterTimeZone mTimezone;         

        internal ForexConnectTimeConverter(O2GSession session, string outputTimeZone)
        {
            mSession = session;
            mTimezone = convToForexConnectZone(outputTimeZone);            
        }

        private O2GTimeConverter getTimeConverter()
        {
            return mSession.getTimeConverter();
        }

        public DateTime convertToOutputZone(DateTime dtInternal)
        {
            O2GTimeConverter tc = getTimeConverter();
            if (tc == null)
                return dtInternal;
            return tc.convert(dtInternal, O2GTimeConverterTimeZone.UTC, mTimezone);
        }

        private static O2GTimeConverterTimeZone convToForexConnectZone(string timezoneName)
        {
            switch (timezoneName.ToLower())
            {
                case "est":
                    return O2GTimeConverterTimeZone.EST;
                    
                case "local":
                    return O2GTimeConverterTimeZone.Local;
                    
            }

            return O2GTimeConverterTimeZone.UTC;
        }
    };

    /// <summary>
    /// Forex connect implementation of ITransport interface
    /// </summary>
    class ForexConnectTransport : ITransport, IO2GSessionStatus, IO2GResponseListener
    {
        string mUser;
        string mPassword;
        string mUrl;
        string mTerminal;
       // O2GTimeConverterTimeZone mTimezone;
        bool mReady;

        O2GSession mSession;
        List<IOffer> mOffersList = new List<IOffer>();
        Dictionary<string, Offer> mOffers = new Dictionary<string, Offer>();
        O2GTimeConverter mTimeConverter;

        ForexConnectTimeConverter mFCTimeConverter;

        Dictionary<string, TransportHistoryRequest> mHistoryRequests = new Dictionary<string, TransportHistoryRequest>();

        public event EventHandler<RequestCompletedEventArgs> OnRequestCompleted;

        internal ForexConnectTransport(string user, string password, string url, string terminal, string timezone)
        {
            mUser = user;
            mPassword = password;
            mUrl = url;
            mTerminal = terminal;
            mReady = false;
                      
            mSession = O2GTransport.createSession();
            mSession.subscribeSessionStatus(this);
            mSession.subscribeResponse(this);

            mFCTimeConverter = new ForexConnectTimeConverter(mSession, timezone);
        }

        public IDateTimeConverter getTimeConverter()
        {
            return mFCTimeConverter;
        }

        /// <summary>
        /// Get list of the supported instruments
        /// </summary>
        public IEnumerable<IOffer> GetInstruments()
        {
            return mOffersList;
        }        

        public int GetInstrumentPrecision(string instrument)
        {
            foreach (IOffer offer in mOffersList)
            {
                if (instrument.Equals(offer.Instrument))
                    return offer.Precision;
            }
            return 0;
        }

        /// <summary>
        /// Get list of the supported time frames
        /// </summary>
        public IEnumerable<string> GetTimeframes()
        {
            if (!mReady)
                return null;

            O2GRequestFactory factory = mSession.getRequestFactory();
            O2GTimeframeCollection timeframes = factory.Timeframes;
            List<string> list = new List<string>();
            for (int i = 0; i < timeframes.Count; i++)
                list.Add(timeframes[i].ID);
            return list;
        }

        /// <summary>
        /// Perform log in
        /// </summary>
        public void login()
        {
            Console.WriteLine("Connecting to {0} **** {1} {2}", mUser, mUrl, mTerminal);
            if (mSession != null)
                mSession.login(mUser, mPassword, mUrl, mTerminal);
        }

        /// <summary>
        /// Returns true if transport is ready to work
        /// </summary>
        public bool IsReady()
        {
            return mReady;
        }

        /// <summary>
        /// Perform log out
        /// </summary>
        public void logout()
        {
            if (mSession != null)
                mSession.logout();
        }

        /// <summary>
        /// Sends a request for the history
        /// </summary>
        public string RequestHistory(TransportHistoryRequest request)
        {
            O2GRequestFactory factory = mSession.getRequestFactory();
            O2GTimeframeCollection timeframes = factory.Timeframes;
            O2GTimeframe timeframe = timeframes[request.Timeframe];
            int count = request.Count > 300 ? 300 : request.Count;
            O2GRequest rq = factory.createMarketDataSnapshotRequestInstrument(request.Instrument, timeframe, count);
            if (request.From != factory.ZERODATE || request.To != factory.ZERODATE)
            {
                DateTime from, to;
                from = request.From;
                to = request.To;
                /*
                if (request.From != factory.ZERODATE)
                    from = mTimeConverter.convert(request.From, O2GTimeConverterTimeZone.EST, O2GTimeConverterTimeZone.UTC);
                else
                    from = factory.ZERODATE;

                if (request.To != factory.ZERODATE)
                    to = mTimeConverter.convert(request.To, O2GTimeConverterTimeZone.EST, O2GTimeConverterTimeZone.UTC);
                else
                    to = factory.ZERODATE;
                */

                factory.fillMarketDataSnapshotRequestTime(rq, from, to, false);
            }
            mHistoryRequests[rq.RequestID] = request;
            mSession.sendRequest(rq);
            return rq.RequestID;
        }


        /// <summary>
        /// Event handled when a session status has been changed
        /// </summary>
        public event EventHandler<SessionStatusEventArgs> OnSessionStatusChanged;

        /// <summary>
        /// Event handled when a tick arrived
        /// </summary>
        public event EventHandler<TickEventArgs> OnTick;

        /// <summary>
        /// Event fired when an login error happened
        /// </summary>
        public event EventHandler<LoginErrorEventArgs> OnError;


        #region Session listener
        void IO2GSessionStatus.onLoginFailed(string error)
        {
            if (OnError != null)
                OnError(this, new LoginErrorEventArgs(error));
        }

        void IO2GSessionStatus.onSessionStatusChanged(O2GSessionStatusCode status)
        {
            bool connected;
            string name;

            switch (status)
            {
                case O2GSessionStatusCode.Connected:
                    connected = true;
                    mTimeConverter = mSession.getTimeConverter();
                    O2GLoginRules rules = mSession.getLoginRules();
                    if (rules.isTableLoadedByDefault(O2GTableType.Offers))
                    {
                        O2GResponse offers = rules.getTableRefreshResponse(O2GTableType.Offers);
                        ReadOffers(offers);
                        mReady = true;
                    }
                    else
                    {
                        O2GRequestFactory reqFactory = mSession.getRequestFactory();
                        O2GRequest request = reqFactory.createRefreshTableRequest(O2GTableType.Offers);
                        mSession.sendRequest(request);
                        mReady = false;
                    }
                    name = "connected";
                    break;
                case O2GSessionStatusCode.Connecting:
                    connected = false;
                    name = "connecting";
                    break;
                case O2GSessionStatusCode.Disconnected:
                    connected = false;
                    name = "disconnected";
                    break;
                case O2GSessionStatusCode.Disconnecting:
                    connected = false;
                    name = "disconnecting";
                    break;
                case O2GSessionStatusCode.PriceSessionReconnecting:
                    connected = false;
                    name = "price channel reconnecting";
                    break;
                case O2GSessionStatusCode.SessionLost:
                    connected = false;
                    name = "session has been lost";
                    break;
                default:
                    connected = false;
                    name = "unknown";
                    break;
            }

            if (!connected)
            {
                mReady = false;
                mTimeConverter = null;
            }

            if (OnSessionStatusChanged != null)
            {
                SessionStatusEventArgs args = new SessionStatusEventArgs(connected, name);
                OnSessionStatusChanged(this, args);
            }
        }
        #endregion

        #region Response listener
        void IO2GResponseListener.onRequestCompleted(string requestId, O2GResponse response)
        {
            if (response.Type == O2GResponseType.GetOffers)
            {
                ReadOffers(response);
                mReady = true;
            }
            else if (response.Type == O2GResponseType.MarketDataSnapshot)
            {
                TransportHistoryRequest req;
                if (mHistoryRequests.TryGetValue(requestId, out req))
                {
                    ReadSnapshot(requestId, response);
                    if (OnRequestCompleted != null)
                    {
                        OnRequestCompleted(this, new RequestCompletedEventArgs(req.ID));
                    }
                }
            }
        }

        void IO2GResponseListener.onRequestFailed(string requestId, string error)
        {
            TransportHistoryRequest request = null;
            mHistoryRequests.TryGetValue(requestId, out request);
            if (request != null)
                request.Reader.onHistoryFailed(requestId, error);
        }

        void IO2GResponseListener.onTablesUpdates(O2GResponse response)
        {
            if (response.Type == O2GResponseType.GetOffers || response.Type == O2GResponseType.TablesUpdates)
            {
                O2GResponseReaderFactory factory = mSession.getResponseReaderFactory();
                if (factory != null)
                {
                    O2GOffersTableResponseReader reader = factory.createOffersTableReader(response);
                    for (int i = 0; i < reader.Count; i++)
                    {
                        O2GOfferRow row = reader.getRow(i);
                        Offer offer = null;
                        mOffers.TryGetValue(row.OfferID, out offer);
                        if (offer == null)
                        {
                            if (row.isInstrumentValid && row.isTimeValid && row.isBidValid && row.isAskValid &&
                                row.isVolumeValid && row.isDigitsValid && row.isPointSizeValid)
                            {
                                //mTimeConverter.convert(row.Time, O2GTimeConverterTimeZone.UTC, mTimezone)
                                //DateTime dt = mTimeConverter.convert(row.Time, O2GTimeConverterTimeZone.Server, O2GTimeConverterTimeZone.UTC);
                                DateTime dt = row.Time;
                                offer = new Offer(row.Instrument, dt, row.Bid, row.Ask, row.Volume, row.Digits, row.PointSize);
                                mOffers[row.OfferID] = offer;
                                mOffersList.Add(offer);
                                SendTick(offer);
                            }
                        }
                        else
                        {
                            if (row.isTimeValid)
                            {
                                //mTimeConverter.convert(row.Time, O2GTimeConverterTimeZone.UTC, mTimezone)
                                //DateTime dt = mTimeConverter.convert(row.Time, O2GTimeConverterTimeZone.Server, O2GTimeConverterTimeZone.UTC);
                                DateTime dt = row.Time;
                                offer.Time = dt;
                            }
                            if (row.isBidValid)
                                offer.Bid = row.Bid;
                            if (row.isAskValid)
                                offer.Ask = row.Ask;
                            if (row.isVolumeValid)
                                offer.Volume = row.Volume;
                            if (row.isPointSizeValid)
                                offer.PointSize = row.PointSize;
                            if (row.isDigitsValid)
                                offer.Precision = row.Digits;
                            SendTick(offer);
                        }
                    }
                }
            }
        }

        void ReadOffers(O2GResponse offers)
        {
            O2GResponseReaderFactory factory = mSession.getResponseReaderFactory();
            if (factory != null)
            {
                O2GOffersTableResponseReader reader = factory.createOffersTableReader(offers);

                mOffers.Clear();
                mOffersList.Clear();

                for (int i = 0; i < reader.Count; i++)
                {
                    O2GOfferRow row = reader.getRow(i);
                    //mTimeConverter.convert(row.Time, O2GTimeConverterTimeZone.UTC, mTimezone)
                    //DateTime dt = mTimeConverter.convert(row.Time, O2GTimeConverterTimeZone.Server, O2GTimeConverterTimeZone.UTC);
                    DateTime dt = row.Time;
                    Offer offer = new Offer(row.Instrument, dt, row.Bid, row.Ask, row.Volume, row.Digits, row.PointSize);
                    mOffers[row.OfferID] = offer;
                    mOffersList.Add(offer);
                    SendTick(offer);
                }
            }
        }

        void ReadSnapshot(string requestId, O2GResponse response)
        {
            TransportHistoryRequest request = null;
            mHistoryRequests.TryGetValue(requestId, out request);
            if (request != null)
            {
                O2GResponseReaderFactory factory = mSession.getResponseReaderFactory();
                if (factory != null)
                {
                    O2GMarketDataSnapshotResponseReader reader = factory.createMarketDataSnapshotReader(response);
                    ITransportHistoryReader consumer = request.Reader;
                    for (int i = 0; i < reader.Count; i++)
                    {
                        //mTimeConverter.convert(reader.getDate(i), O2GTimeConverterTimeZone.UTC, mTimezone)
                        DateTime dt = reader.getDate(i);
                        consumer.onHistoryRow(requestId, dt,
                                              reader.getBidOpen(i), reader.getBidHigh(i), reader.getBidLow(i), reader.getBidClose(i),
                                              reader.getAskOpen(i), reader.getAskHigh(i), reader.getAskLow(i), reader.getAskClose(i),
                                              reader.getVolume(i), i);
                    }
                    //mTimeConverter.convert(reader.getLastBarTime(), O2GTimeConverterTimeZone.UTC, mTimezone)
                    DateTime dt1 = reader.getLastBarTime();
                    consumer.setLastBar(dt1, reader.getLastBarVolume());
                    mHistoryRequests.Remove(requestId);
                }
            }
        }

        void SendTick(IOffer offer)
        {
            if (OnTick != null)
            {
                TickEventArgs args = new TickEventArgs(offer.Instrument, offer.Time, offer.Bid, offer.Ask, offer.Volume);
                OnTick(this, args);
            }
        }


        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ForexConnectTransport()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposal)
        {
            if (mSession != null)
            {
                mSession.logout();
                mSession.unsubscribeResponse(this);
                mSession.unsubscribeSessionStatus(this);
                mSession.Dispose();
                mSession = null;
            }
        }
    }
}
