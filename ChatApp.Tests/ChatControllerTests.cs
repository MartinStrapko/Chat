using ChatApp.Controllers;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatApp.Tests;

public class ChatControllerTests
{
    private readonly Mock<IQueueService> _mockQueueService;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _mockQueueService = new Mock<IQueueService>();
        var mockAgentService = new Mock<IAgentService>();
        _mockQueueService.Setup(service => service.TryEnqueue(It.IsAny<ChatSession>())).Returns(true);

        _controller = new ChatController(_mockQueueService.Object, mockAgentService.Object);
    }

    [Fact]
    public void InitiateChat_ShouldReturnOkResult()
    {
        var result = _controller.InitiateChat() as OkObjectResult;

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void InitiateChat_ShouldReturnSessionId()
    {
        var result = _controller.InitiateChat() as OkObjectResult;

        Assert.NotNull(result);
        Assert.IsType<Guid>(result.Value);
    }
}