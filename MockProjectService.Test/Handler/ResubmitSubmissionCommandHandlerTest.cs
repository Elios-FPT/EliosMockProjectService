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
    public class ResubmitSubmissionCommandHandlerTest
    {
        private readonly Mock<IGenericRepository<Submission>> _submissionRepositoryMock;
        private readonly ResubmitSubmissionCommandHandler _handler;

        public ResubmitSubmissionCommandHandlerTest()
        {
            _submissionRepositoryMock = new Mock<IGenericRepository<Submission>>();
            _handler = new ResubmitSubmissionCommandHandler(_submissionRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidSubmissionId_CreatesNewSubmissionAndReturnsNewId()
        {
            // Arrange
            var originalSubmissionId = Guid.NewGuid();
            var command = new ResubmitSubmissionCommand(originalSubmissionId);

            var originalSubmission = new Submission
            {
                Id = originalSubmissionId,
                UserId = Guid.NewGuid(),
                MockProjectId = Guid.NewGuid(),
                Status = "Completed",
                FinalAssessment = "Good",
                FinalGrade = 90
            };

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            Submission? addedSubmission = null;

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(originalSubmissionId))
                .ReturnsAsync(originalSubmission);

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
            result.Message.Should().Be("Submission resubmitted successfully.");
            result.ResponseData.Should().NotBeNullOrEmpty();

            var newSubmissionId = Guid.Parse(result.ResponseData!);
            newSubmissionId.Should().NotBe(originalSubmissionId); // ID mới khác cũ
            newSubmissionId.Should().Be(addedSubmission!.Id);

            // Kiểm tra dữ liệu mới được copy đúng
            addedSubmission!.UserId.Should().Be(originalSubmission.UserId);
            addedSubmission!.MockProjectId.Should().Be(originalSubmission.MockProjectId);
            addedSubmission!.Status.Should().Be("Pending");
            addedSubmission!.FinalAssessment.Should().BeNull(); // Không copy
            addedSubmission!.FinalGrade.Should().BeNull();      // Không copy

            // Verify interactions
            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(originalSubmissionId), Times.Once);
            _submissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Once);
            unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SubmissionIdIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var command = new ResubmitSubmissionCommand(Guid.Empty);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(400);
            result.Message.Should().Be("Submission ID cannot be empty.");
            result.ResponseData.Should().BeNull();

            // Không gọi đến repository
            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _submissionRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SubmissionNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var command = new ResubmitSubmissionCommand(nonExistentId);

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Submission?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(404);
            result.Message.Should().Be("Submission not found.");
            result.ResponseData.Should().BeNull();

            // Không tạo submission mới
            _submissionRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
            _submissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AddAsyncThrowsException_RollbacksAndReturnsInternalError()
        {
            // Arrange
            var originalId = Guid.NewGuid();
            var command = new ResubmitSubmissionCommand(originalId);

            var original = new Submission
            {
                Id = originalId,
                UserId = Guid.NewGuid(),
                MockProjectId = Guid.NewGuid()
            };

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(originalId))
                .ReturnsAsync(original);

            _submissionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(unitOfWorkMock.Object);

            _submissionRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Submission>()))
                .ThrowsAsync(new InvalidOperationException("Unique constraint violation"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to resubmit:");
            result.Message.Should().Contain("Unique constraint violation");
            result.ResponseData.Should().BeNull();

            unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
            unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_GetByIdAsyncThrowsException_ReturnsInternalError()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var command = new ResubmitSubmissionCommand(submissionId);

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to resubmit:");
            result.Message.Should().Contain("Database connection failed");
            result.ResponseData.Should().BeNull();

            // Không bắt đầu transaction
            _submissionRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
        }
    }
}