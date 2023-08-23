using ChatApp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Tests;

public class ChatControllerTests
{
    [Fact]
    public void InitiateChat_ShouldReturnOkResult()
    {
        var controller = new ChatController();

        var result = controller.InitiateChat() as OkObjectResult;

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void InitiateChat_ShouldReturnSessionId()
    {
        var controller = new ChatController();

        var result = controller.InitiateChat() as OkObjectResult;

        Assert.NotNull(result);
        Assert.IsType<Guid>(result.Value);
    }
}