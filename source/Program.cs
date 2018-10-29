using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;

namespace LiveChartDataExportToCSV
{
    class Program
    {
        static string mLogin = string.Empty;
        static string mUrl = string.Empty;
        static string mConnection = string.Empty;
        static string mSessionID = string.Empty;
        static string mPin = string.Empty;
        static string mOutputDir = string.Empty;
        static string mDelimiter = string.Empty;
        static string mFormatDecimal = string.Empty;
        static string mDateTimeSeparator = string.Empty;
        static string mTimezone = string.Empty;
        static List<HistoryConfigData> mHistory = new List<HistoryConfigData>();

        static bool mExit;
        static bool mConnected;

        static Dictionary<string, List<ICandleDataWriter>> mDataWriters;

        static void Main(string[] args)
        {
            ITransport transport = null;
            CandleManager manager = null;
            mDataWriters = new Dictionary<string, List<ICandleDataWriter>>();
            try
            {
                ReadDataFromConfig("Configuration.xml");

                if (args.Length == 0)
                {
                    throw new Exception("Please specify password via command line");
                }

                CtrlCHandler ctrlc = new CtrlCHandler();
                mExit = mConnected = false;
                transport = TransportFactory.CreateForexConnectTransport(mLogin, args[0], mUrl, mConnection, mTimezone);
                //getDayOffsetByTimeZone should depend on the server zone
                manager = new CandleManager(transport, -3/*getDayOffsetByTimeZone(mTimezone)*/, -1);
                transport.OnSessionStatusChanged += OnStatus;
                transport.OnError += OnError;
                transport.login();
                while (!mExit && !mConnected)
                {
                    Thread.Sleep(10);
                }

                if (mConnected)
                {
                    foreach (HistoryConfigData config in mHistory)
                    {
                        CandleHistory history = manager.GetHistory(config.Instrument, config.Timeframe, config.NumBars);
                        mDataWriters[history.ID] = new List<ICandleDataWriter>();
                        //mDataWriters[history.ID].Add(new ConsoleCandleDataWriter(config.Instrument, config.Timeframe, true));
                        mDataWriters[history.ID].Add(new CSVCandleDataWriter(config.Instrument, config.Timeframe, mOutputDir,
                            config.Filename, mDelimiter, mFormatDecimal.ToLower().Equals("y"), mDateTimeSeparator, true, mTimezone, transport.getTimeConverter()));
                        history.OnLoaded += OnHistoryLoaded;
                        history.OnFailed += OnHistoryFailed;
                        history.OnUpdated += OnHistoryUpdated;
                    }

                    while (!ctrlc.ExitRequest)
                    {
                        Thread.Sleep(10);
                    }

                    transport.logout();
                    while (!mExit) ;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                manager = null;
                if (transport != null)
                {
                    transport.Dispose();
                }

                foreach (List<ICandleDataWriter> writers in mDataWriters.Values)
                {
                    foreach (ICandleDataWriter writer in writers)
                    {
                        writer.Dispose();
                    }
                }
            }
        }

        static int getDayOffsetByTimeZone(string timeZone)
        {
            if (timeZone.Equals("EST", StringComparison.OrdinalIgnoreCase))
                return -7;
            if (timeZone.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                return -2;
            return -7;
        }      

        static void ReadDataFromConfig(string configFilePath)
        {
            try
            {
                using (FileStream fs = File.OpenRead(configFilePath))
                {
                    XmlReader reader = XmlReader.Create(fs);
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.LocalName.ToLower())
                            {
                                case "login":
                                    mLogin = reader.ReadElementContentAsString();
                                    break;
                                case "url":
                                    mUrl = reader.ReadElementContentAsString();
                                    break;
                                case "connection":
                                    mConnection = reader.ReadElementContentAsString();
                                    break;
                                case "sessionid":
                                    mSessionID = reader.ReadElementContentAsString();
                                    break;
                                case "pin":
                                    mPin = reader.ReadElementContentAsString();
                                    break;
                                case "outputdir":
                                    mOutputDir = reader.ReadElementContentAsString();
                                    break;
                                case "delimiter":
                                    mDelimiter = reader.ReadElementContentAsString();
                                    break;
                                case "formatdecimalplaces":
                                    mFormatDecimal = reader.ReadElementContentAsString();
                                    break;
                                case "datetimeseparator":
                                    mDateTimeSeparator = reader.ReadElementContentAsString();
                                    break;
                                case "timezone":
                                    mTimezone = reader.ReadElementContentAsString();
                                    break;
                                case "history":
                                    mHistory.Add(GetHistoryConfiguration(reader.ReadSubtree()));
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Invalid config file");
            }
        }

        static HistoryConfigData GetHistoryConfiguration(XmlReader reader)
        {
            string instrument = string.Empty;
            string timeframe = string.Empty;
            string filename = string.Empty;
            int numBars = 0;
            
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.LocalName)
                    {
                        case "Instrument":
                            instrument = reader.ReadElementContentAsString();
                            break;
                        case "Timeframe":
                            timeframe = reader.ReadElementContentAsString();
                            break;
                        case "Filename":
                            filename = reader.ReadElementContentAsString();
                            break;
                        case "NumBars":
                            numBars = reader.ReadElementContentAsInt();
                            break;
                    }
                }
            }
            return new HistoryConfigData(instrument, timeframe, filename, numBars);
        }
        
        static void OnStatus(object sender, SessionStatusEventArgs args)
        {
            Console.WriteLine("Status: {0}", args.Name);
            if (args.Name == "disconnected")
                mExit = true;
            mConnected = args.Connected;
        }
        
        static void OnError(object sender, LoginErrorEventArgs args)
        {
            Console.WriteLine("Connection error: {0}", args.Error);
            mExit = true;
        }
        
        static void OnHistoryLoaded(object sender, CandleHistoryLoadedEventArgs args)
        {
            CandleHistory history = (CandleHistory)sender;
            List<ICandleDataWriter> writers = null;
            if (mDataWriters.TryGetValue(history.ID, out writers))
            {
                for (int i = 0; i < history.Count; i++)
                {
                    foreach (ICandleDataWriter writer in writers)
                    {
                        writer.WriteCandleData(history[i]);
                    }
                }
            }
        }
        
        static void OnHistoryFailed(object sender, CandleHistoryFailedEventArgs args)
        {
            CandleHistory history = (CandleHistory)sender;
            Console.WriteLine("{0} {1} {2}", history.Instrument, history.Timeframe, args.Error);
        }
        
        static void OnHistoryUpdated(object sender, CandleHistoryCandleUpdatedEventArgs args)
        {
            CandleHistory history = (CandleHistory)sender;
            List<ICandleDataWriter> writers = null;

            if (mDataWriters.TryGetValue(history.ID, out writers))
            {
                foreach (ICandleDataWriter writer in writers)
                {
                    writer.WriteCandleData(args.Candle);
                }
            }
        }
    }

    class HistoryConfigData
    {
        public string Instrument
        {
            get
            {
                return mInstrument;
            }
            private set
            {
                mInstrument = value;
            }
        }
        private string mInstrument;

        public string Timeframe
        {
            get
            {
                return mTimeframe;
            }
            private set
            {
                mTimeframe = value;
            }
        }
        private string mTimeframe;

        public string Filename
        {
            get
            {
                return mFilename;
            }
            private set
            {
                mFilename = value;
            }
        }
        private string mFilename;

        public int NumBars
        {
            get
            {
                return mNumBars;
            }
            private set
            {
                mNumBars = value;
            }
        }
        private int mNumBars;

        public HistoryConfigData(string instrument, string timeframe, string filename, int numBars)
        {
            Instrument = instrument;
            Timeframe = timeframe;
            Filename = filename;
            NumBars = numBars;
        }
    }
}
