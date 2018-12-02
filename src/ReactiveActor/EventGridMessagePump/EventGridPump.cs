// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace EventGridMessagePump
{
    public static class EventGridPump
    {
        [FunctionName("EventGridPump")]
        public static void Run(
            [EventGridTrigger]EventGridEvent eventGridEvent, 
            [ServiceBus("eventstopic", Connection = "ServiceBus", EntityType = EntityType.Topic)]out string queueMessage,
            ILogger log)
        {
            if (eventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
            {
                log.LogInformation("Blob created.");
            }

            queueMessage = eventGridEvent.Id;

            log.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}
