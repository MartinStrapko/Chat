using ChatApp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Tests;

public class ChatControllerTests
{
    [Fact]
    public void InitiateChat_ShouldReturnOkResult()
    {
        var controller = new ChatController();

        var result = controller.InitiateChat();

        Assert.IsType<OkResult>(result);
    }
}