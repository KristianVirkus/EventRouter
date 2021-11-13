using FluentAssertions;
using NUnit.Framework;
using System;

namespace EventRouter.Core.UnitTests
{
    class QueueableEventTest
    {
        [Test]
        public void ConstructorWithEventNull_ShouldThrow_ArgumentNullException()
        {
            // Arrange
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new QueueableEvent<TestRoutable>(evt: null));
        }

        [Test]
        public void Constructor_Should_SetProperties()
        {
            // Arrange
            var evt = new TestRoutable();

            // Act
            var obj = new QueueableEvent<TestRoutable>(evt: evt);

            // Assert
            obj.Event.Should().BeSameAs(evt);
        }
    }
}
