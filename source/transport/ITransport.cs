using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveChartDataExportToCSV
{
    /// <summary>
    /// Interface to a transport
    /// </summary>
    public interface ITransport : IDisposable
    {
        /// <summary>
        /// Perform log in
        /// </summary>
        void login();
        
        /// <summary>
        /// Perform log out
        /// </summary>
        void logout();
    
        /// <summary>
        /// Get list of the supported instruments
        /// </summary>
        IEnumerable<IOffer> GetInstruments();

        int GetInstrumentPrecision(string instrument);
        
        /// <summary>
        /// Get list of the supported time frames
        /// </summary>
        IEnumerable<string> GetTimeframes();
        
        /// <summary>
        /// Returns true if transport is ready to wrok
        /// </summary>
        bool IsReady();
        
        /// <summary>
        /// Sends a request for the history
        /// </summary>
        string RequestHistory(TransportHistoryRequest request);

        IDateTimeConverter getTimeConverter();

        /// <summary>
        /// Event fired when a session status has been changed
        /// </summary>
        event EventHandler<SessionStatusEventArgs> OnSessionStatusChanged;
        
        /// <summary>
        /// Event fired when a tick arrived
        /// </summary>
        event EventHandler<TickEventArgs> OnTick;
        
        /// <summary>
        /// Event fired when an login error happened
        /// </summary>
        event EventHandler<LoginErrorEventArgs> OnError;

        event EventHandler<RequestCompletedEventArgs> OnRequestCompleted;                
    }

    public interface IDateTimeConverter
    {
        DateTime convertToOutputZone(DateTime dtInternal);
    };
}
