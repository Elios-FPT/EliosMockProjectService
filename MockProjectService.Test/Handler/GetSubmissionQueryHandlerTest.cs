using FluentAssertions;
using Moq;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Handler.Submission.Query;
using MockProjectService.Core.Interfaces;
using MockProjectService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Test.Handler
{
    public class GetSubmissionQueryHandlerTest
    {
        private readonly Mock<IGenericRepository<Submission>> _submissionRepositoryMock;
        private readonly GetSubmissionQueryHandler _handler;

        public GetSubmissionQueryHandlerTest()
        {
            _submissionRepositoryMock = new Mock<IGenericRepository<Submission>>();
            _handler = new GetSubmissionQueryHandler(_submissionRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidSubmissionId_ReturnsSuccessWithSubmissionDto()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var existingSubmission = new Submission
            {
                Id = submissionId,
                UserId = userId,
                MockProjectId = projectId,
                Status = "Completed",
                FinalAssessment = "Great job!",
                FinalGrade = 95.5
            };

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync(existingSubmission);

            var query = new GetSubmissionQuery(submissionId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Submission retrieved successfully.");
            result.ResponseData.Should().NotBeNull();

            var dto = result.ResponseData!;
            dto.Id.Should().Be(submissionId);
            dto.UserId.Should().Be(userId);
            dto.MockProjectId.Should().Be(projectId);
            dto.Status.Should().Be("Completed");
            dto.FinalAssessment.Should().Be("Great job!");
            dto.FinalGrade.Should().Be(95.5);

            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(submissionId), Times.Once);
        }

        [Fact]
        public async Task Handle_SubmissionIdIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var query = new GetSubmissionQuery(Guid.Empty);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(400);
            result.Message.Should().Be("Submission ID cannot be empty.");
            result.ResponseData.Should().BeNull();

            // Repository không được gọi
            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_SubmissionNotFound_ReturnsNotFound()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var query = new GetSubmissionQuery(submissionId);

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync((Submission?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(404);
            result.Message.Should().Be("Submission not found.");
            result.ResponseData.Should().BeNull();

            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(submissionId), Times.Once);
        }

        [Fact]
        public async Task Handle_GetByIdAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var query = new GetSubmissionQuery(submissionId);

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ThrowsAsync(new Exception("Database connection lost"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to retrieve submission:");
            result.Message.Should().Contain("Database connection lost");
            result.ResponseData.Should().BeNull();

            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(submissionId), Times.Once);
        }
    }
}