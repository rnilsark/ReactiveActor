// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace MessagePump
{
    public static class EventGridPump
    {
        [FunctionName("EventGridPump")]
        public static void Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            //[Blob("{data.url}", FileAccess.Read)] Stream input,
            [ServiceBus("eventstopic", Connection = "ServiceBus", EntityType = EntityType.Topic)]
            out Message message,
            ILogger log)
        {
            if (eventGridEvent.EventType != Events.BlobCreated)
                throw new Exception(
                    $"Expected to be triggered by an {Events.BlobCreated} but received {eventGridEvent.EventType}.");

            //var streamReader = new StreamReader(input);
            //var jsonObject = JsonConvert.DeserializeObject(streamReader.ReadToEnd());

            message = new Message
            {
                MessageId = eventGridEvent.Id
            };

            log.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}