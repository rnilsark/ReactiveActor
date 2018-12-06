using System;
using FluentAssertions;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.ServiceBus;
using NUnit.Framework;

namespace MessagePump.Tests
{
    [TestFixture]
    public class WhenBlobIsCreated
    {
        [Test]
        public void ThenOutputsMessage()
        {
            var id = Guid.NewGuid().ToString();
            
            var timeStamp = DateTime.Parse("2018-01-01 00:00:00");
            var data = new StorageBlobCreatedEventData();
            var eventGridEvent = new EventGridEvent(id: id, subject: "", data: data, eventType: Events.BlobCreated, eventTime: timeStamp, dataVersion: "");
            
            EventGridPump.Run(eventGridEvent, out var output, new StubLogger());

            var expected = new Message{MessageId = id};

            output.MessageId.Should().Be(expected.MessageId);
        }
    }
}
