using ChatApp.Commands;
using ChatApp.Controllers;
using ChatApp.Interfaces;
using ChatApp.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatApp.Tests;

public class ChatControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new ChatController(_mockMediator.Object);

        _mockMediator.Setup(mediator => mediator.Send(It.IsAny<InitiateChatCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
    }

    [Fact]
    public async void InitiateChat_ShouldReturnOkResult()
    {
        var result = await _controller.InitiateChat() as OkObjectResult;

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async void InitiateChat_ShouldReturnSessionId()
    {
        var result = await _controller.InitiateChat() as OkObjectResult;

        Assert.NotNull(result);
        Assert.IsType<Guid>(result.Value);
    }
}