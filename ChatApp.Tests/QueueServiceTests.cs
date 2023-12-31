﻿using ChatApp.Commands;
using ChatApp.Controllers;
using ChatApp.Interfaces;
using ChatApp.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace ChatApp.Tests
{
    public class QueueServiceTests
    {
        private readonly QueueService _queueService;
        private readonly Mock<QueueService> _mockQueueService;

        public QueueServiceTests()
        {
            var settings = new ChatSettings
            {
                TimerIntervalSeconds = 10,
                PollsPerSecond = 1,
                MarkInactiveAfterNumberOfMissedPolls = 3
            };

            var mockSettings = new Mock<IOptions<ChatSettings>>();
            mockSettings.Setup(ap => ap.Value).Returns(settings);
            var mockAgentService = new Mock<IAgentService>();
            _mockQueueService = new Mock<QueueService>(mockSettings.Object, mockAgentService.Object);
            _queueService = _mockQueueService.Object;

        }

        [Fact]
        public void TryEnqueue_ShouldAddChatSessionToQueue()
        {
            var chatSession = new ChatSession { SessionId = Guid.NewGuid() };
            _mockQueueService.Setup(q => q.MaxCapacity).Returns(1);

            bool isEnqueued = _queueService.TryEnqueue(chatSession);

            Assert.True(isEnqueued);
            Assert.True(_queueService.ContainsSession(chatSession.SessionId));
        }

        [Fact]
        public async Task InitiateChat_WhenQueueIsFull_ShouldReturnBadRequest()
        {
            var mockMediator = new Mock<IMediator>();

            mockMediator.Setup(m => m.Send(It.IsAny<InitiateChatCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

            var controller = new ChatController(mockMediator.Object);

            var result = await controller.InitiateChat() as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal("Queue is full.", result.Value);
        }


        [Fact]
        public void TryEnqueue_ShouldReturnTrue_WhenQueueHasSpace()
        {
            _mockQueueService.Setup(q => q.MaxCapacity).Returns(2);

            var chatSession = new ChatSession();
            var result = _queueService.TryEnqueue(chatSession);

            Assert.True(result);
        }

        [Fact]
        public void TryEnqueue_ShouldReturnFalse_WhenQueueIsFull()
        {
            _mockQueueService.Setup(q => q.OverflowCapacity).Returns(0);
            _queueService.TryEnqueue(new ChatSession());

            var anotherChatSession = new ChatSession();
            var result = _queueService.TryEnqueue(anotherChatSession);

            Assert.False(result);
        }

        [Fact]
        public void TryEnqueue_ShouldAddToOverflow_WhenMainQueueIsFullAndItsOfficeHours()
        {
            _mockQueueService.Setup(q => q.MaxCapacity).Returns(1);
            _mockQueueService.Setup(q => q.OverflowCapacity).Returns(1);
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
            _mockQueueService.Setup(q => q.OverflowCapacity).Returns(1);
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
            _mockQueueService.Setup(q => q.OverflowCapacity).Returns(1);
            _queueService.TryEnqueue(new ChatSession());
            _queueService.TryEnqueue(new ChatSession());

            var thirdChatSession = new ChatSession();
            var result = _queueService.TryEnqueue(thirdChatSession);

            Assert.False(result);
        }

        [Fact]
        public void Dequeue_ShouldRemoveAndReturnChatSessionFromFront()
        {
            _mockQueueService.Setup(q => q.MaxCapacity).Returns(1);
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
            var result = _queueService.Dequeue();

            Assert.Null(result);
        }

        [Fact]
        public void Dequeue_MainQueueHasItems_ReturnsItemFromMainQueue()
        {
            _mockQueueService.Setup(q => q.MaxCapacity).Returns(1);
            var chatSession = new ChatSession();
            _queueService.TryEnqueue(chatSession);

            var result = _queueService.Dequeue();

            Assert.Equal(chatSession, result);
        }

        [Fact]
        public void Dequeue_MainAndOverflowBothHaveItems_ReturnsItemFromMainQueueFirst()
        {
            _mockQueueService.Setup(q => q.OverflowCapacity).Returns(1);
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
            _mockQueueService.Setup(q => q.MaxCapacity).Returns(10);
            _mockQueueService.Setup(q => q.OverflowCapacity).Returns(1);
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
