﻿using ChatApp.Interfaces;
using ChatApp.Models;
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
        public void Dequeue_ShouldRemoveAndReturnChatSessionFromFront()
        {
            var chatSession1 = new ChatSession { SessionId = Guid.NewGuid() };
            var chatSession2 = new ChatSession { SessionId = Guid.NewGuid() };

            _queueService.TryEnqueue(chatSession1);
            _queueService.TryEnqueue(chatSession2);

            var dequeuedSession = _queueService.Dequeue();

            Assert.Equal(chatSession1.SessionId, dequeuedSession?.SessionId);
            Assert.False(_queueService.ContainsSession(chatSession1.SessionId));
        }

    }
}