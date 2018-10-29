using System;
using System.Collections.Generic;
using System.Text;

namespace LiveChartDataExportToCSV
{
    public interface IDateFormatter
    {
        string DateTimeSeparator { get; }
        string GetFormattedDate(DateTime dDisplayDate, TimeframeUnit timeFrameUnit);
    }

    public class DateFormatter : IDateFormatter
    {
        public string DateTimeSeparator
        {
            get { return mDateTimeSeparator; }
        }
        private string mDateTimeSeparator = string.Empty;

        public DateFormatter(string dateTimeSeparator)
        {
            mDateTimeSeparator = dateTimeSeparator;
        }

        public string GetFormattedDate(DateTime dDisplayDate, TimeframeUnit timeFrameUnit)
        {            
            string sDisplayDate = string.Empty;
            switch (timeFrameUnit)
            {
                case TimeframeUnit.Minute:
                    sDisplayDate = dDisplayDate.ToString(string.Format("dd'/'MM'/'yyyy'{0}'HH:mm", mDateTimeSeparator));
                    break;
                case TimeframeUnit.Hour:
                    sDisplayDate = dDisplayDate.ToString(string.Format("dd'/'MM'/'yyyy'{0}'HH:mm", mDateTimeSeparator));
                    break;
                case TimeframeUnit.Day:
                    dDisplayDate = dDisplayDate.AddDays(1);
                    //sDisplayDate = dDisplayDate.ToString("dd'/'MM'/'yyyy");
                    sDisplayDate = dDisplayDate.ToString(string.Format("dd'/'MM'/'yyyy'{0}'HH:mm", mDateTimeSeparator));
                    break;
                case TimeframeUnit.Week:
                    dDisplayDate = dDisplayDate.AddDays(1);
                    //sDisplayDate = dDisplayDate.ToString("dd'/'MM'/'yyyy");
                    sDisplayDate = dDisplayDate.ToString(string.Format("dd'/'MM'/'yyyy'{0}'HH:mm", mDateTimeSeparator));
                    break;
                case TimeframeUnit.Month:
                    dDisplayDate = dDisplayDate.AddDays(1);
                    //sDisplayDate = dDisplayDate.ToString("MM'/'yyyy");
                    sDisplayDate = dDisplayDate.ToString(string.Format("dd'/'MM'/'yyyy'{0}'HH:mm", mDateTimeSeparator));
                    break;
                case TimeframeUnit.Year:
                    dDisplayDate = dDisplayDate.AddDays(1);
                    //sDisplayDate = dDisplayDate.ToString("yyyy");
                    sDisplayDate = dDisplayDate.ToString(string.Format("dd'/'MM'/'yyyy'{0}'HH:mm", mDateTimeSeparator));
                    break;
            }
            return sDisplayDate;
        }
    }
}
