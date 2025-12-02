using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpPollClient.Common.Metrics
{
    public class DefaultMetricsTracker : IMetricsTracker
    {
        public DefaultMetricsTracker() { }

        public void Log(string message)
        {
            Console.WriteLine($"[Metrics] {message}");
        }
    }
}
