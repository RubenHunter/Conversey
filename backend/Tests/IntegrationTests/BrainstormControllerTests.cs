using Conversey.BL.Ai;
using Conversey.BL.Ai.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UI_MVC.Controllers.Api;

namespace Tests.IntegrationTests;

public class BrainstormControllerTests
{
    private readonly Mock<IAiManager> _aiManagerMock = new();
    private readonly BrainstormController _controller;

    public BrainstormControllerTests()
    {
        _controller = new BrainstormController(
            _aiManagerMock.Object,
            Mock.Of<ILogger<BrainstormController>>());
    }

    [Fact]
    public async Task ExtractKeyPhrases_ValidTranscript_ReturnsOkWithPhrases()
    {
        _aiManagerMock
            .Setup(m => m.ExtractKeyPhrases("test transcript", "nl", 5, null, null))
            .ReturnsAsync(new ExtractKeyPhrasesResponse(new[] { "phrase 1", "phrase 2" }, Array.Empty<RejectedPhrase>()));

        var result = await _controller.ExtractKeyPhrases(
            new ExtractKeyPhrasesRequest("test transcript", "nl", 5));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExtractKeyPhrasesResponse>(ok.Value);
        Assert.Equal(2, response.Phrases.Count);
    }

    [Fact]
    public async Task ExtractKeyPhrases_EmptyTranscript_ReturnsBadRequest()
    {
        var result = await _controller.ExtractKeyPhrases(
            new ExtractKeyPhrasesRequest("", "nl", 5));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ExtractKeyPhrases_WhitespaceTranscript_ReturnsBadRequest()
    {
        var result = await _controller.ExtractKeyPhrases(
            new ExtractKeyPhrasesRequest("   ", "nl", 5));

        Assert.IsType<BadRequestObjectResult>(result); 
    }

    [Fact]
    public async Task ExtractKeyPhrases_PassesExistingAndRejectedToManager()
    {
        IReadOnlyList<string> existing = new[] { "already shown" };
        IReadOnlyList<string> rejected = new[] { "user rejected this" };

        _aiManagerMock
            .Setup(m => m.ExtractKeyPhrases("text", "nl", 5, existing, rejected))
            .ReturnsAsync(new ExtractKeyPhrasesResponse(new[] { "new phrase" }, Array.Empty<RejectedPhrase>()));

        var result = await _controller.ExtractKeyPhrases(
            new ExtractKeyPhrasesRequest("text", "nl", 5, existing, rejected));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExtractKeyPhrasesResponse>(ok.Value);
        Assert.Single(response.Phrases);
        _aiManagerMock.Verify(
            m => m.ExtractKeyPhrases("text", "nl", 5, existing, rejected),
            Times.Once);
    }
}
