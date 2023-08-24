using ChatApp.Controllers;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Tests
{
    public class QueueServiceTests
    {
        private readonly QueueService _queueService;

        public QueueServiceTests()
        {
            _queueService = new QueueService();
        }

        [Fact]
        public void TryEnqueue_ShouldAddChatSessionToQueue()
        {
            var chatSession = new ChatSession { SessionId = Guid.NewGuid() };

            bool isEnqueued = _queueService.TryEnqueue(chatSession);

            Assert.True(isEnqueued);
            Assert.True(_queueService.ContainsSession(chatSession.SessionId));
        }

        [Fact]
        public void InitiateChat_WhenQueueIsFull_ShouldReturnBadRequest()
        {
            var mockQueueService = new Mock<IQueueService>();
            var mockAgentService = new Mock<IAgentService>();
            mockQueueService.Setup(s => s.TryEnqueue(It.IsAny<ChatSession>())).Returns(false);

            var controller = new ChatController(mockQueueService.Object, mockAgentService.Object);

            var result = controller.InitiateChat() as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal("Queue is full.", result.Value);
        }

        [Fact]
        public void TryEnqueue_ShouldReturnTrue_WhenQueueHasSpace()
        {
            _queueService.MaxCapacity = 2;

            var chatSession = new ChatSession();
            var result = _queueService.TryEnqueue(chatSession);

            Assert.True(result);
        }

        [Fact]
        public void TryEnqueue_ShouldReturnFalse_WhenQueueIsFull()
        {
            _queueService.MaxCapacity = 1;
            _queueService.TryEnqueue(new ChatSession());

            var anotherChatSession = new ChatSession();
            var result = _queueService.TryEnqueue(anotherChatSession);

            Assert.False(result);
        }

        [Fact]
        public void TryEnqueue_ShouldAddToOverflow_WhenMainQueueIsFullAndItsOfficeHours()
        {
            _queueService.MaxCapacity = 1;
            _queueService.OverflowCapacity = 1;
            var currentTime = DateTime.UtcNow;
            _queueService.OfficeStart = currentTime.TimeOfDay - TimeSpan.FromHours(1);  // Office started 1 hour ago
            _queueService.OfficeEnd = currentTime.TimeOfDay + TimeSpan.FromHours(1);    // Office ends in 1 hour


            _queueService.TryEnqueue(new ChatSession());

            var anotherChatSession = new ChatSession();
            var result = _queueService.TryEnqueue(anotherChatSession);

            Assert.True(result);
        }

        [Fact]
        public void TryEnqueue_ShouldNotAddToOverflow_WhenOutsideOfficeHours()
        {
            _queueService.MaxCapacity = 1;
            _queueService.OverflowCapacity = 1;
            var currentTime = DateTime.UtcNow;
            _queueService.OfficeStart = currentTime.TimeOfDay + TimeSpan.FromMinutes(10);  // Office starts in 10 minutes
            _queueService.OfficeEnd = currentTime.TimeOfDay + TimeSpan.FromHours(8);      // Office ends in 8 hours

            _queueService.TryEnqueue(new ChatSession());

            var anotherChatSession = new ChatSession();
            var result = _queueService.TryEnqueue(anotherChatSession);

            Assert.False(result);
        }

        [Fact]
        public void TryEnqueue_ShouldRefuseChat_WhenBothQueuesAreFull()
        {
            _queueService.MaxCapacity = 1;
            _queueService.OverflowCapacity = 1;
            _queueService.TryEnqueue(new ChatSession());
            _queueService.TryEnqueue(new ChatSession());

            var thirdChatSession = new ChatSession();
            var result = _queueService.TryEnqueue(thirdChatSession);

            Assert.False(result);
        }

        [Fact]
        public void Dequeue_ShouldRemoveAndReturnChatSessionFromFront()
        {
            var chatSession1 = new ChatSession();
            var chatSession2 = new ChatSession();

            _queueService.TryEnqueue(chatSession1);
            _queueService.TryEnqueue(chatSession2);

            var dequeuedSession = _queueService.Dequeue();

            Assert.Equal(chatSession1.SessionId, dequeuedSession?.SessionId);
            Assert.False(_queueService.ContainsSession(chatSession1.SessionId));
        }

        [Fact]
        public void Dequeue_BothQueuesEmpty_ReturnsNull()
        {
            var queueService = new QueueService();

            var result = queueService.Dequeue();

            Assert.Null(result);
        }

        [Fact]
        public void Dequeue_MainQueueHasItems_ReturnsItemFromMainQueue()
        {
            var queueService = new QueueService();
            var chatSession = new ChatSession();
            queueService.TryEnqueue(chatSession);

            var result = queueService.Dequeue();

            Assert.Equal(chatSession, result);
        }

        [Fact]
        public void Dequeue_MainAndOverflowBothHaveItems_ReturnsItemFromMainQueueFirst()
        {
            _queueService.MaxCapacity = 5;
            _queueService.OverflowCapacity = 1;
            var currentTime = DateTime.UtcNow;
            _queueService.OfficeStart = currentTime.TimeOfDay - TimeSpan.FromHours(1);
            _queueService.OfficeEnd = currentTime.TimeOfDay + TimeSpan.FromHours(1);

            var firstInQueue = new ChatSession();
            _queueService.TryEnqueue(firstInQueue);

            for (int i = 0; i < _queueService.MaxCapacity; i++)
            {
                _queueService.TryEnqueue(new ChatSession());
            }

            var resultFromMain = _queueService.Dequeue();

            Assert.NotNull(resultFromMain);
            Assert.Equal(firstInQueue, resultFromMain);
        }

        [Fact]
        public void Dequeue_MainQueueEmptyOverflowHasItems_ReturnsItemFromOverflow()
        {
            _queueService.MaxCapacity = 10;
            _queueService.OverflowCapacity = 1;
            var currentTime = DateTime.UtcNow;
            _queueService.OfficeStart = currentTime.TimeOfDay - TimeSpan.FromHours(1);
            _queueService.OfficeEnd = currentTime.TimeOfDay + TimeSpan.FromHours(1);
            for (int i = 0; i < 11; i++)
            {
                _queueService.TryEnqueue(new ChatSession());
            }

            // Dequeue all items from main queue
            for (int i = 0; i < 10; i++)
            {
                _queueService.Dequeue();
            }

            var overflowSession = _queueService.Dequeue();

            Assert.NotNull(overflowSession);
        }
    }
}
