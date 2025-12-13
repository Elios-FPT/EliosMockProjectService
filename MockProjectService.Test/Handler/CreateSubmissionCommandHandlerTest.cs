using FluentAssertions;
using Moq;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Handler.Submission.Command;
using MockProjectService.Core.Interfaces;
using MockProjectService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static MockProjectService.Contract.UseCases.Submission.Command;

namespace MockProjectService.Test.Handler
{
    public class CreateSubmissionCommandHandlerTest
    {
        private readonly Mock<IGenericRepository<Submission>> _submissionRepositoryMock;
        private readonly Mock<IGenericRepository<MockProject>> _projectRepositoryMock;
        private readonly CreateSubmissionCommandHandler _handler;

        public CreateSubmissionCommandHandlerTest()
        {
            _submissionRepositoryMock = new Mock<IGenericRepository<Submission>>();
            _projectRepositoryMock = new Mock<IGenericRepository<MockProject>>();
            _handler = new CreateSubmissionCommandHandler(
                _submissionRepositoryMock.Object,
                _projectRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ProjectExists_ReturnsSuccessWithSubmissionId()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new CreateSubmissionCommand(userId, projectId);

            var existingProject = new MockProject { Id = projectId, Title = "Test Project" };

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            Submission? addedSubmission = null;

            _projectRepositoryMock
                .Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(existingProject);

            _submissionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(unitOfWorkMock.Object);

            _submissionRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Submission>()))
                .Callback<Submission>(s => addedSubmission = s)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Submission created successfully.");
            result.ResponseData.Should().NotBeNullOrEmpty();

            Guid submissionId = Guid.Parse(result.ResponseData!);
            submissionId.Should().Be(addedSubmission!.Id);

            addedSubmission!.UserId.Should().Be(userId);
            addedSubmission!.MockProjectId.Should().Be(projectId);
            addedSubmission!.Status.Should().Be("Pending");
            addedSubmission!.FinalAssessment.Should().Be("");

            // Verify
            _projectRepositoryMock.Verify(r => r.GetByIdAsync(projectId), Times.Once);
            _submissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Once);
            unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ProjectIdIsEmpty_ReturnsBadRequest()
        {
            var command = new CreateSubmissionCommand(Guid.NewGuid(), Guid.Empty);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Status.Should().Be(400);
            result.Message.Should().Be("Project ID cannot be empty.");
            result.ResponseData.Should().BeNull();

            _projectRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _submissionRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsBadRequest()
        {
            var projectId = Guid.NewGuid();
            var command = new CreateSubmissionCommand(Guid.NewGuid(), projectId);

            _projectRepositoryMock
                .Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync((MockProject?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Status.Should().Be(400);
            result.Message.Should().Be("Mock project not found.");
            result.ResponseData.Should().BeNull();

            _submissionRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
            _submissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AddAsyncThrowsException_RollbacksAndReturnsInternalError()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new CreateSubmissionCommand(userId, projectId);

            var existingProject = new MockProject { Id = projectId };

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            _projectRepositoryMock
                .Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(existingProject);

            _submissionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(unitOfWorkMock.Object);

            _submissionRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Submission>()))
                .ThrowsAsync(new Exception("Database timeout"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to create submission:");
            result.Message.Should().Contain("Database timeout");
            result.ResponseData.Should().BeNull();

            unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
            unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}