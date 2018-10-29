using System;
using System.Collections.Generic;
using System.Text;

namespace LiveChartDataExportToCSV
{
    delegate DateTime GetCandleDelegate(DateTime tick, int dayOffset, int weekOffset);

    /// <summary>
    /// Types of the time units for the time frame
    /// </summary>
    public enum TimeframeUnit
    {
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year,
    };

    /// <summary>
    /// Time frame description
    /// </summary>
    public class Timeframe
    {
        private const int SECONDS_IN_DAY = 86400;
        private const int MINUTES_IN_DAY = 1440;
        private const int SECONDS_IN_HOUR = 3600;
    
        private TimeframeUnit mUnit;
        int mLength;
        long mLengthInSeconds;
        
        /// <summary>
        /// Get time frame units
        /// </summary>
        public TimeframeUnit Unit
        {
            get
            {
                return mUnit;
            }
        }
        
        /// <summary>
        /// Get time frame length expressed in units
        /// </summary>
        public int Length
        {
            get
            {
                return mLength;
            }
        }
        
        /// <summary>
        /// Get time frame name
        /// </summary>
        public string Name
        {
            get
            {
                string name;
                switch (mUnit)
                {
                case    TimeframeUnit.Minute:
                        name = "m";
                        break;
                case    TimeframeUnit.Hour:
                        name = "H";
                        break;
                case    TimeframeUnit.Day:
                        name = "D";
                        break;
                case    TimeframeUnit.Week:
                        name = "W";
                        break;
                case    TimeframeUnit.Month:
                        name = "M";
                        break;
                case    TimeframeUnit.Year:
                        name = "Y";
                        break;
                default:
                        name = "?";
                        break;
                }
                name = name + mLength.ToString();
                return name;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
        
        /// <summary>
        /// Parse time frame name
        /// </summary>
        /// <param name="timeframe">Name of the time frame to parse</param>
        public static Timeframe Parse(string timeframe)
        {
            TimeframeUnit unit;
            int length;
            
            char c = timeframe[0];
            switch (c)
            {
            case    'm':
                    unit = TimeframeUnit.Minute;
                    break;
            case    'H':
                    unit = TimeframeUnit.Hour;
                    break;
            case    'D':
                    unit = TimeframeUnit.Day;
                    break;
            case    'W':
                    unit = TimeframeUnit.Week;
                    break;
            case    'M':
                    unit = TimeframeUnit.Month;
                    break;
            case    'Y':
                    unit = TimeframeUnit.Year;
                    break;
            default:
                    throw new ArgumentException("The name of the timeframe must start with m, H, D, W, M or Y");
            }
            
            length = Int32.Parse(timeframe.Substring(1));
            return new Timeframe(unit, length);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Timeframe(TimeframeUnit unit, int length)
        {
            if (length <= 0)
                throw new ArgumentException("The length cannot be zero or negative");
        
            //validate length
            switch (unit)
            {
            case    TimeframeUnit.Minute:
                    if (length >= MINUTES_IN_DAY || ((MINUTES_IN_DAY % length) != 0))
                        throw new ArgumentException("Minute time frame must be shorter than a day and there must be the whole number of bars in a day");
                        
                    if (length == 1)
                        mGetCandle = new GetCandleDelegate(this.GetCandleOneMinute);
                    else
                        mGetCandle = new GetCandleDelegate(this.GetCandleNMinutes);
                    mLengthInSeconds = (long)length * 60;
                    break;
            case    TimeframeUnit.Hour:
                    if (length >= 24 || ((24 % length) != 0))
                        throw new ArgumentException("Hour time frame must be shorter than a day and there must be the whole number of bars in a day");
                    mGetCandle = new GetCandleDelegate(this.GetCandleNMinutes);                        
                    mLengthInSeconds = (long)length * 3600;                        
                    break;
            case    TimeframeUnit.Day:
                    if (length >= 365)
                        throw new ArgumentException("Day frame frame must be shorter than an year");
                    mLengthInSeconds = (long)length * SECONDS_IN_DAY;
                    if (length == 1)
                        mGetCandle = new GetCandleDelegate(this.GetCandleOneDay);
                    else
                        mGetCandle = new GetCandleDelegate(this.GetCandleNDays);
                    break;
            case    TimeframeUnit.Week:
                    if (length != 1)
                        throw new ArgumentException("1-week period only is supported");
                    mGetCandle = new GetCandleDelegate(this.GetCandleWeek);
                    break;
            case    TimeframeUnit.Month:
                    if (length >= 12 || ((12 % length) != 0))
                        throw new ArgumentException("Month time frame must be shorter than an year and there must be the whole number of bars in an year");
                    mGetCandle = new GetCandleDelegate(this.GetCandleMonth); 
                    break;
            case    TimeframeUnit.Year:
                    if (length != 1)
                        throw new ArgumentException("1-year period only is supported");
                    mGetCandle = new GetCandleDelegate(this.GetCandleYear);                         
                    break;
            }
            mUnit = unit;
            mLength = length;
        }
        
        internal DateTime GetCandle(DateTime tick, int dayoffset, int weekoffset)
        {
            return mGetCandle(tick, dayoffset, weekoffset);
        }
        
        #region Candle math
        GetCandleDelegate mGetCandle = null;

        /// <summary>
        /// Calculate candle border for 1 minute timeframe
        /// </summary>
        internal DateTime GetCandleOneMinute(DateTime tick, int dayOffset, int weekOffset)
        {
            long d = toSeconds(tick.Ticks);
            long r = d - d % 60;
            return fromSeconds(r);
        }        
        
        /// <summary>
        /// Calculate candle border for N-minutes time frame
        /// </summary>
        private DateTime GetCandleNMinutes(DateTime tick, int dayOffset, int weekOffset)
        {
            long start = toSeconds(tick.Ticks);
            long day = getTradingDay(tick.Ticks, dayOffset);
            if (day > start)
                day -= SECONDS_IN_DAY;
            long n = (start - day) / mLengthInSeconds;
            return fromSeconds(day + n * mLengthInSeconds);
        }
        
        /// <summary>
        /// Calculate candle border for 1-day time frame
        /// </summary>
        private DateTime GetCandleOneDay(DateTime tick, int dayOffset, int weekOffset)
        {
            return fromSeconds(getTradingDay(tick.Ticks, dayOffset));
        }
        
        /// <summary>
        /// Calculate candle border for N-days time frame
        /// </summary>
        private DateTime GetCandleNDays(DateTime tick, int dayOffset, int weekOffset)
        {
            long time = toSeconds(tick.Ticks);
            time -= dayOffset * SECONDS_IN_HOUR;
            DateTime t = fromSeconds(time);
            t = new DateTime(t.Year, t.Month, 1);
            time = toSeconds(t.Ticks);
            time += dayOffset * SECONDS_IN_HOUR;
            
            long start = time;
            
            time = toSeconds(tick.Ticks);
            long n = (time - start) / mLengthInSeconds;
            start += n * mLengthInSeconds;
            return fromSeconds(start);
        }
        
        /// <summary>
        /// Calculate candle border for N-months time frame
        /// </summary>
        private DateTime GetCandleMonth(DateTime tick, int dayOffset, int weekOffset)
        {
            long seconds = toSeconds(tick.Ticks);
            seconds -= dayOffset * SECONDS_IN_HOUR;
            DateTime t = fromSeconds(seconds);
            int month = (t.Month - 1) / mLength;
            month = month * mLength + 1;
            t = new DateTime(t.Year, month, 1);
            seconds = toSeconds(t.Ticks);
            return fromSeconds(seconds + dayOffset * SECONDS_IN_HOUR);
        }
        
        /// <summary>
        /// Calculate time borders for 1-year time frame
        /// </summary>
        private DateTime GetCandleYear(DateTime tick, int dayOffset, int weekOffset)
        {
            return fromSeconds(getTradingYear(tick, dayOffset));
        }
        
        /// <summary>
        /// Calculate time borders for 1-week time frame
        /// </summary>
        private DateTime GetCandleWeek(DateTime tick, int dayOffset, int weekOffset)
        {            
            DateTime t;
            
            long s = toSeconds(tick.Ticks);
            s -= (long)dayOffset * SECONDS_IN_HOUR;
            s -= s % SECONDS_IN_DAY;
            s -= (weekOffset + 1) * SECONDS_IN_DAY;
            t = fromSeconds(s);
            long off = (long)(int)t.DayOfWeek;
            s = toSeconds(t.Ticks);
            s -= off * SECONDS_IN_DAY;
            s += (long)dayOffset * SECONDS_IN_HOUR;
            s += (weekOffset + 1)* SECONDS_IN_DAY;
            return fromSeconds(s);
        }
        
        private const long TICKS_IN_SECOND = 10000000;
        private const long TICKS_IN_MILLISECOND = TICKS_IN_SECOND / 1000;
        private const long TICKS_IN_HALFMILLISECOND = TICKS_IN_SECOND / 2000;
        
        /// <summary>
        /// convert ticks to seconds
        /// </summary>
        private long toSeconds(long ticks)
        {
            long ms = ticks / TICKS_IN_MILLISECOND;
            if (ticks % TICKS_IN_MILLISECOND >= TICKS_IN_HALFMILLISECOND) 
                ms++;
            return ms / 1000;
        }

        /// <summary>
        /// Get the serial second of the beginning of the trading day
        /// </summary>
        private long getTradingDay(long ticks, int dayoffset)
        {
            long seconds = toSeconds(ticks);
            seconds -= (long)dayoffset * SECONDS_IN_HOUR;
            seconds -= seconds % SECONDS_IN_DAY;
            return seconds + (long)dayoffset * SECONDS_IN_HOUR;
        }
        
        /// <summary>
        /// Get the serial second of the beginning of the trading year
        /// </summary>
        private long getTradingYear(DateTime tick, int dayOffset)
        {
            long time = toSeconds(tick.Ticks);
            time -= dayOffset * SECONDS_IN_HOUR;
            DateTime t = fromSeconds(time);
            t = new DateTime(t.Year, 1, 1);
            time = toSeconds(t.Ticks);
            time += dayOffset * SECONDS_IN_HOUR;
            return time;
        }
        
        /// <summary>
        /// convert seconds to date
        /// </summary>
        private DateTime fromSeconds(long seconds)
        {
            return new DateTime(seconds * TICKS_IN_SECOND);
        }
        
        #endregion
    }
}
