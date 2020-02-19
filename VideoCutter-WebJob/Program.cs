using System;
using Microsoft.Azure.WebJobs;

namespace VideoCutter_WebJob
{

    public class Program
    {
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            // Default queue check has an exponentially increasing backoff interval.
            // Optionally can set a cap on how long the polling interval will increase to.

            config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(5);

            var host = new JobHost(config);

            // The following code ensures that the WebJob will be running continuously.

            host.RunAndBlock();
        }
    }
}
