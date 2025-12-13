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
    public class SaveFeedbackCommandHandlerTest
    {
        private readonly Mock<IGenericRepository<Submission>> _submissionRepositoryMock;
        private readonly SaveFeedbackCommandHandler _handler;

        public SaveFeedbackCommandHandlerTest()
        {
            _submissionRepositoryMock = new Mock<IGenericRepository<Submission>>();
            _handler = new SaveFeedbackCommandHandler(_submissionRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_UpdatesFinalAssessmentAndReturnsTrue()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var newFeedback = "Excellent work! Keep it up. Fix minor issues in naming conventions.";

            var command = new SaveFeedbackCommand(submissionId, newFeedback);

            var existingSubmission = new Submission
            {
                Id = submissionId,
                UserId = Guid.NewGuid(),
                MockProjectId = Guid.NewGuid(),
                Status = "Completed",
                FinalAssessment = "Previous feedback", // cũ
                FinalGrade = 85
            };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            Submission? updatedSubmission = null;

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync(existingSubmission);

            _submissionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(unitOfWorkMock.Object);

            _submissionRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Submission>()))
                .Callback<Submission>(s => updatedSubmission = s)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Feedback saved successfully.");
            result.ResponseData.Should().BeTrue();

            // Kiểm tra feedback đã được cập nhật đúng
            updatedSubmission.Should().NotBeNull();
            updatedSubmission!.FinalAssessment.Should().Be(newFeedback);
            // Các field khác không bị thay đổi
            updatedSubmission.UserId.Should().Be(existingSubmission.UserId);
            updatedSubmission.Status.Should().Be(existingSubmission.Status);

            // Verify interactions
            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(submissionId), Times.Once);
            _submissionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Submission>()), Times.Once);
            unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SubmissionIdIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var command = new SaveFeedbackCommand(Guid.Empty, "Some feedback");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(400);
            result.Message.Should().Be("Submission ID cannot be empty.");
            result.ResponseData.Should().BeFalse();

            // Không gọi repository
            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _submissionRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SubmissionNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var command = new SaveFeedbackCommand(nonExistentId, "Feedback text");

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Submission?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(404);
            result.Message.Should().Be("Submission not found.");
            result.ResponseData.Should().BeFalse();

            // Không thực hiện update
            _submissionRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
            _submissionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Submission>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdateAsyncThrowsException_RollbacksAndReturnsInternalError()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var command = new SaveFeedbackCommand(submissionId, "New feedback");

            var submission = new Submission
            {
                Id = submissionId,
                FinalAssessment = "Old feedback"
            };

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync(submission);

            _submissionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(unitOfWorkMock.Object);

            _submissionRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Submission>()))
                .ThrowsAsync(new InvalidOperationException("Optimistic concurrency violation"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to save feedback:");
            result.Message.Should().Contain("Optimistic concurrency violation");
            result.ResponseData.Should().BeFalse();

            unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
            unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_GetByIdAsyncThrowsException_ReturnsInternalError()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var command = new SaveFeedbackCommand(submissionId, "Feedback");

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ThrowsAsync(new Exception("Database unavailable"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to save feedback:");
            result.Message.Should().Contain("Database unavailable");
            result.ResponseData.Should().BeFalse();

            _submissionRepositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyFeedbackString_StillSavesSuccessfully()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var command = new SaveFeedbackCommand(submissionId, ""); // Feedback rỗng hợp lệ

            var submission = new Submission { Id = submissionId, FinalAssessment = "Old" };

            var unitOfWorkMock = new Mock<IUnitOfWork>();

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync(submission);

            _submissionRepositoryMock
                .Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(unitOfWorkMock.Object);

            _submissionRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Submission>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.ResponseData.Should().BeTrue();

            _submissionRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Submission>(s => s.FinalAssessment == "")), Times.Once);
        }
    }
}