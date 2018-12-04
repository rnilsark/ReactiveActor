using System;
using FluentAssertions;
using Microsoft.Azure.EventGrid.Models;
using NUnit.Framework;

namespace MessagePump.Tests
{
    [TestFixture]
    public class WhenBlobIsDeleted
    {
        [Test]
        public void ThenNoOp()
        {
            var data = new StorageBlobDeletedEventData();
            var eventGridEvent = new EventGridEvent(id: "", subject: "", data: data, eventType: "Microsoft.Storage.BlobDeleted", eventTime: DateTime.UtcNow, dataVersion: "");

            Action action = () => EventGridPump.Run(eventGridEvent, out var output, new StubLogger());
            action.Should().Throw<Exception>("Expected to be triggered by an Microsoft.Storage.BlobCreated but received Microsoft.Storage.BlobDeleted.");
        }
    }
}
