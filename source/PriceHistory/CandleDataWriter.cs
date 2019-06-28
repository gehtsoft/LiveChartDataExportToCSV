using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using fxcore2;

namespace LiveChartDataExportToCSV
{
    public interface ICandleDataWriter : IDisposable
    {
        void WriteCandleData(Candle candle);
    }

    public class ConsoleCandleDataWriter : ICandleDataWriter
    {
        private bool mWriteIncompleteCandles;
        private string mInstrument;
        private string mTimeframe;

        public ConsoleCandleDataWriter(string instrument, string timeframe, bool writeIncompleteCandles)
        {
            mInstrument = instrument;
            mTimeframe = timeframe;
            mWriteIncompleteCandles = writeIncompleteCandles;
        }

        public void WriteCandleData(Candle candle)
        {
            if (candle.IsCompleted || mWriteIncompleteCandles)
            {
                Console.WriteLine("{0} bo:{1} bh:{2} bl:{3} bc:{4} ao:{5} ah:{6} al:{7} ac:{8} v:{9}", candle.Date, candle.BidOpen, candle.BidHigh, candle.BidLow, candle.BidClose, candle.AskOpen, candle.AskHigh, candle.AskLow, candle.AskClose, candle.Volume);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

    public class CSVCandleDataWriter : ICandleDataWriter
    {
        private bool mWriteIncompleteCandles;
        private string mInstrument;
        private string mTimeframe;
        private bool mFormatDecimal;
        private string mDelimiter;

        private DateFormatter mFormatter;
        private CsvFileWriter mWriter;
        private bool mAddHeader;
        private bool mIsFirstLine;

        //O2GTimeConverter mTimeConverter;
        //O2GTimeConverterTimeZone mTimezone;
        IDateTimeConverter mDateTimeConverter;

        public CSVCandleDataWriter(string instrument, string timeframe, string outputFolder, string filename, string delimiter,
            bool formatDecimal, string dateTimeSeparator, bool writeIncompleteCandles, string timezone, IDateTimeConverter dateTimeConverter, bool addHeader)
        {
            mInstrument = instrument;
            mTimeframe = timeframe;
            mWriteIncompleteCandles = writeIncompleteCandles;
            mFormatDecimal = formatDecimal;
            mAddHeader = addHeader;
            mIsFirstLine = true;

            //mTimezone = convToForexConnectZone(timezone);
            mDateTimeConverter = dateTimeConverter;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // TODO: folder ends with "\"
            string path = string.Format(@"{0}\{1}.csv", outputFolder, filename);
            if (!File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Close();
                }
            }
            mDelimiter = delimiter;

            mWriter = new CsvFileWriter(path, mDelimiter);
            mFormatter = new DateFormatter(dateTimeSeparator);
        }        

        public void WriteCandleData(Candle candle)
        {
            if (mAddHeader == true && mIsFirstLine == true)
            {
                WriteHeaderData();
            }

            CsvRow row = new CsvRow();
            DateTime dateDisplay = candle.Date;
           // dateDisplay = mTimeConverter.convert(candle.Date, O2GTimeConverterTimeZone.Server, mTimezone);
            //dateDisplay = mDateTimeConverter.convertToOutputZone(dateDisplay);
            
            string format = "{0:f" + candle.Precision + "}";
            row.AddRange(mFormatter.GetFormattedDate(dateDisplay, candle.Timeframe).Split(new string[] { mDelimiter }, StringSplitOptions.RemoveEmptyEntries));
            if (mFormatDecimal)
            {
                row.Add(string.Format(format, candle.BidOpen));
                row.Add(string.Format(format, candle.BidHigh));
                row.Add(string.Format(format, candle.BidLow));
                row.Add(string.Format(format, candle.BidClose));
                row.Add(string.Format(format, candle.AskOpen));
                row.Add(string.Format(format, candle.AskHigh));
                row.Add(string.Format(format, candle.AskLow));
                row.Add(string.Format(format, candle.AskClose));
            }
            else
            {
                row.Add(candle.BidOpen.ToString());
                row.Add(candle.BidHigh.ToString());
                row.Add(candle.BidLow.ToString());
                row.Add(candle.BidClose.ToString());
                row.Add(candle.AskOpen.ToString());
                row.Add(candle.AskHigh.ToString());
                row.Add(candle.AskLow.ToString());
                row.Add(candle.AskClose.ToString());
            }
            row.Add(candle.Volume.ToString());
            mWriter.WriteRow(row);
            mIsFirstLine = false;

            if (!candle.IsCompleted)
            {
                mWriter.MoveToPreviousLine();
            }
        }

        private void WriteHeaderData()
        {
            CsvRow row = new CsvRow();

            row.Add("Date");
            row.Add("BidOpen");
            row.Add("BidHigh");
            row.Add("BidLow");
            row.Add("BidClose");
            row.Add("AskOpen");
            row.Add("AskHigh");
            row.Add("AskLow");
            row.Add("AskClose");
            row.Add("Volume");

            mWriter.WriteRow(row);
        }

        #region IDisposable Members

        public void Dispose()
        {
            mWriter.Flush();
            mWriter.Close();
        }

        #endregion

        private class CsvFileWriter : StreamWriter
        {
            private string mDelimiter;
            private long mPreviousLinePosition;

            public CsvFileWriter(string filename, string delimiter) : base(filename)
            {
                mDelimiter = delimiter;
                mPreviousLinePosition = 0;
            }

            /// <summary>
            /// Writes a single row to a CSV file.
            /// </summary>
            /// <param name="row">The row to be written</param>
            public void WriteRow(CsvRow row)
            {
                mPreviousLinePosition = BaseStream.Position;

                StringBuilder builder = new StringBuilder();
                bool firstColumn = true;
                foreach (string value in row)
                {
                    // Add separator if this isn't the first value
                    if (!firstColumn)
                    {
                        builder.Append(mDelimiter);
                    }
                    // Implement special handling for values that contain comma or quote
                    // Enclose in quotes and double up any double quotes
                    if (value.IndexOfAny(new char[] { '"', mDelimiter[0] }) != -1)
                    {
                        builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                    }
                    else
                    {
                        builder.Append(value);
                    }
                    firstColumn = false;
                }
                string lineText = builder.ToString();
                WriteLine(lineText);
                Flush();
            }

            public void MoveToPreviousLine()
            {
                BaseStream.Seek(mPreviousLinePosition, SeekOrigin.Begin);
            }
        }

        private class CsvRow : List<string>
        {
        }


    }
}
