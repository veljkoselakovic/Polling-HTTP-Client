using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpPollClient.Common.Metrics
{
    public interface IMetricsTracker
    {
        public void Log(string message);

    }
}
