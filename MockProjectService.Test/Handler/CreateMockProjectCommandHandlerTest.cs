using FluentAssertions;
using Moq;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Handler.MockProject.Command;
using MockProjectService.Core.Interfaces;
using MockProjectService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static MockProjectService.Contract.UseCases.MockProject.Command;

namespace MockProjectService.Test.Handler
{
    public class CreateMockProjectCommandHandlerTest
    {
        private readonly Mock<IGenericRepository<MockProject>> _projectRepositoryMock;
        private readonly CreateMockProjectCommandHandler _handler;

        public CreateMockProjectCommandHandlerTest()
        {
            _projectRepositoryMock = new Mock<IGenericRepository<MockProject>>();
            _handler = new CreateMockProjectCommandHandler(_projectRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccessResponseWithProjectId()
        {
            // Arrange
            var command = new CreateMockProjectCommand(
                Title: "Valid Title",
                Language: "C#",
                Description: "Some detailed description",
                Difficulty: "Medium",
                FileName: "project.zip",
                KeyPrefix: "VALID",
                BaseProjectUrl: "https://example.com/base-project"
            );

            MockProject? addedProject = null;
            var transactionMock = new Mock<IUnitOfWork>();

            _projectRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            _projectRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<MockProject>()))
                .Callback<MockProject>(p => addedProject = p)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Mock project created successfully.");
            result.ResponseData.Should().NotBeNullOrEmpty();

            Guid parsedId = Guid.Parse(result.ResponseData!);
            parsedId.Should().Be(addedProject!.Id);

            // Verify repository interactions
            _projectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MockProject>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
            transactionMock.Verify(t => t.RollbackAsync(), Times.Never);
        }

        [Theory]
        [InlineData(null, "C#", "Medium", "VALID")]           // Title null
        [InlineData("", "C#", "Medium", "VALID")]             // Title empty
        [InlineData("Title", null, "Medium", "VALID")]        // Language null
        [InlineData("Title", "", "Medium", "VALID")]         // Language empty
        [InlineData("Title", "C#", null, "VALID")]           // Difficulty null
        [InlineData("Title", "C#", "", "VALID")]             // Difficulty empty
        [InlineData("Title", "C#", "Medium", null)]          // KeyPrefix null
        [InlineData("Title", "C#", "Medium", "")]            // KeyPrefix empty
        public async Task Handle_MissingRequiredFields_ReturnsBadRequest(
            string? title, string? language, string? difficulty, string? keyPrefix)
        {
            // Arrange
            var command = new CreateMockProjectCommand(
                Title: title!,
                Language: language!,
                Description: "Optional desc",
                Difficulty: difficulty!,
                FileName: null,
                KeyPrefix: keyPrefix!,
                BaseProjectUrl: null
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(400);
            result.Message.Should().Contain("cannot be null or empty");
            result.ResponseData.Should().BeNull();

            // Repository không được gọi
            _projectRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
            _projectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MockProject>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AddAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var command = new CreateMockProjectCommand(
                Title: "Valid Title",
                Language: "JavaScript",
                Description: null,
                Difficulty: "Hard",
                FileName: "app.js",
                KeyPrefix: "TEST",
                BaseProjectUrl: null
            );

            var transactionMock = new Mock<IUnitOfWork>();

            _projectRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            _projectRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<MockProject>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to create mock project:");
            result.Message.Should().Contain("Database connection failed");
            result.ResponseData.Should().BeNull();

            transactionMock.Verify(t => t.RollbackAsync(), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Never);
        }
    }
}