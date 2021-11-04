using FluentAssertions;
using NUnit.Framework;
using System;

namespace EventRouter.Core.UnitTests
{
    public class QueueableWaitHandleSignalTest
    {
        [Test]
        public void Signal_Should_TriggerWaitHandle()
        {
            // Arrange
            var sut = new QueueableWaitHandleSignal();

            // Assert
            sut.Event.WaitOne(TimeSpan.Zero).Should().BeFalse();

            // Act
            sut.Signal();

            // Assert
            sut.Event.WaitOne(TimeSpan.Zero).Should().BeTrue();
        }
    }
}
