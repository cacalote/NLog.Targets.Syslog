using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>Allows to build Syslog messages</summary>
    public abstract class MessageBuilder
    {
        private SplitOnNewLinePolicy splitOnNewLinePolicy;

        /// <summary>The Syslog facility to log from (its name e.g. local0 or local7)</summary>
        public Facility Facility { get; set; }

        protected MessageBuilder()
        {
            Facility = Facility.Local1;
        }

        /// <summary>Initializes the MessageBuilder</summary>
        /// <param name="initedEnforcement">The enforcement to apply</param>
        internal virtual void Initialize(Enforcement initedEnforcement)
        {
            splitOnNewLinePolicy = new SplitOnNewLinePolicy(initedEnforcement);
        }

        /// <summary>Builds a set of Syslog messages according to an RFC</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="layout">The NLog.LogEventInfo</param>
        /// <returns>For each Syslog message the bytes containing it</returns>
        internal IEnumerable<IEnumerable<byte>> BuildMessages(LogEventInfo logEvent, Layout layout)
        {
            var pri = Pri(Facility, (Severity)logEvent.Level);
            var logEntries = LogEntries(logEvent, layout).ToList();
            var toBeSent = logEntries.Select(logEntry => BuildMessage(logEvent, pri, logEntry));
            return toBeSent;
        }

        /// <summary>Builds the Syslog message according to an RFC</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>Bytes containing the Syslog message</returns>
        protected abstract IEnumerable<byte> BuildMessage(LogEventInfo logEvent, string pri, string logEntry);

        private static string Pri(Facility facility, Severity severity)
        {
            var priVal = (int)facility * 8 + (int)severity;
            var priValString = priVal.ToString(CultureInfo.InvariantCulture);
            return $"<{priValString}>";
        }

        private IEnumerable<string> LogEntries(LogEventInfo logEvent, Layout layout)
        {
            var originalLogEntry = layout.Render(logEvent);
            return splitOnNewLinePolicy.IsApplicable() ? splitOnNewLinePolicy.Apply(originalLogEntry) : new[] { originalLogEntry };
        }
    }
}