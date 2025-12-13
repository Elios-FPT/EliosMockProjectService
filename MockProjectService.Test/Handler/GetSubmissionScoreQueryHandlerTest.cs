using FluentAssertions;
using Moq;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Handler.Submission.Query;
using MockProjectService.Core.Interfaces;
using MockProjectService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Test.Handler
{
    public class GetSubmissionScoreQueryHandlerTest
    {
        private readonly Mock<IGenericRepository<Submission>> _submissionRepositoryMock;
        private readonly Mock<IGenericRepository<SubmissionsClass>> _classRepositoryMock;
        private readonly GetSubmissionScoreQueryHandler _handler;

        public GetSubmissionScoreQueryHandlerTest()
        {
            _submissionRepositoryMock = new Mock<IGenericRepository<Submission>>();
            _classRepositoryMock = new Mock<IGenericRepository<SubmissionsClass>>();
            _handler = new GetSubmissionScoreQueryHandler(
                _submissionRepositoryMock.Object,
                _classRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidSubmission_WithGrades_ReturnsAverageScore()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var query = new GetSubmissionScoreQuery(submissionId);

            var submission = new Submission { Id = submissionId };

            var classes = new List<SubmissionsClass>
            {
                new SubmissionsClass { SubmissionId = submissionId, Grade = 80.0 },
                new SubmissionsClass { SubmissionId = submissionId, Grade = 90.5 },
                new SubmissionsClass { SubmissionId = submissionId, Grade = 100.0 },
                new SubmissionsClass { SubmissionId = submissionId, Grade = null }
            };

            var expectedScore = 67.625;

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync(submission);

            _classRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<SubmissionsClass, bool>>>(),
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync(classes);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Score retrieved successfully.");
            result.ResponseData.Should().NotBeNull();
            result.ResponseData.Should().BeApproximately(expectedScore, 0.0001);

            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(submissionId), Times.Once);
            _classRepositoryMock.Verify(r => r.GetListAsync(
                It.IsAny<Expression<Func<SubmissionsClass, bool>>>(),
                null, null, null, null), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidSubmission_NoClasses_ReturnsNullScore()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var query = new GetSubmissionScoreQuery(submissionId);

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync(new Submission { Id = submissionId });

            _classRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<SubmissionsClass, bool>>>(),
                    null, null, null, null))
                .ReturnsAsync(new List<SubmissionsClass>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Score retrieved successfully.");
            result.ResponseData.Should().BeNull();

            _classRepositoryMock.Verify(r => r.GetListAsync(
                It.IsAny<Expression<Func<SubmissionsClass, bool>>>(),
                null, null, null, null), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidSubmission_AllGradesNull_ReturnsNullScore()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var query = new GetSubmissionScoreQuery(submissionId);

            var submission = new Submission { Id = submissionId };

            var classes = new List<SubmissionsClass>
            {
                new SubmissionsClass { SubmissionId = submissionId, Grade = null },
                new SubmissionsClass { SubmissionId = submissionId, Grade = null }
            };

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync(submission);

            _classRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<SubmissionsClass, bool>>>(),
                    null, null, null, null))
                .ReturnsAsync(classes);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Score retrieved successfully.");
            result.ResponseData.Should().BeNull();

            result.ResponseData.Should().Be(0.0);
        }

        [Fact]
        public async Task Handle_SubmissionIdIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var query = new GetSubmissionScoreQuery(Guid.Empty);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(400);
            result.Message.Should().Be("Submission ID cannot be empty.");
            result.ResponseData.Should().BeNull();

            _submissionRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _classRepositoryMock.Verify(r => r.GetListAsync(It.IsAny<Expression<Func<SubmissionsClass, bool>>>(), null, null, null, null), Times.Never);
        }

        [Fact]
        public async Task Handle_SubmissionNotFound_ReturnsNotFound()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var query = new GetSubmissionScoreQuery(submissionId);

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync((Submission?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(404);
            result.Message.Should().Be("Submission not found.");
            result.ResponseData.Should().BeNull();

            _classRepositoryMock.Verify(r => r.GetListAsync(It.IsAny<Expression<Func<SubmissionsClass, bool>>>(), null, null, null, null), Times.Never);
        }

        [Fact]
        public async Task Handle_ExceptionInGetListAsync_ReturnsInternalServerError()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var query = new GetSubmissionScoreQuery(submissionId);

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ReturnsAsync(new Submission { Id = submissionId });

            _classRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<SubmissionsClass, bool>>>(),
                    null, null, null, null))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to get score:");
            result.Message.Should().Contain("Database connection failed");
            result.ResponseData.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExceptionInGetByIdAsync_ReturnsInternalServerError()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var query = new GetSubmissionScoreQuery(submissionId);

            _submissionRepositoryMock
                .Setup(r => r.GetByIdAsync(submissionId))
                .ThrowsAsync(new InvalidOperationException("Query timeout"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to get score:");
            result.Message.Should().Contain("Query timeout");
            result.ResponseData.Should().BeNull();
        }
    }
}