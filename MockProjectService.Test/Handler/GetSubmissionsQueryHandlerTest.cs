using FluentAssertions;
using Moq;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Core.Extensions;
using MockProjectService.Core.Handler.Submission.Query;
using MockProjectService.Core.Interfaces;
using MockProjectService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static MockProjectService.Contract.UseCases.Submission.Query;

namespace MockProjectService.Test.Handler
{
    public class GetSubmissionsQueryHandlerTest
    {
        private readonly Mock<IGenericRepository<Submission>> _submissionRepositoryMock;
        private readonly Mock<IQueryExtensions<Submission>> _queryExtensionsMock;
        private readonly GetSubmissionsQueryHandler _handler;

        public GetSubmissionsQueryHandlerTest()
        {
            _submissionRepositoryMock = new Mock<IGenericRepository<Submission>>();
            _queryExtensionsMock = new Mock<IQueryExtensions<Submission>>();
            _handler = new GetSubmissionsQueryHandler(
                _submissionRepositoryMock.Object,
                _queryExtensionsMock.Object);
        }

        [Fact]
        public async Task Handle_NoFilters_ReturnsAllSubmissionsMappedToDtos()
        {
            // Arrange
            var query = new GetSubmissionsQuery(UserId: null, ProjectId: null, Status: null);

            var submissionsFromDb = new List<Submission>
            {
                new Submission
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    MockProjectId = Guid.NewGuid(),
                    Status = "Pending",
                    FinalAssessment = "In progress",
                    FinalGrade = 75.5
                },
                new Submission
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    MockProjectId = Guid.NewGuid(),
                    Status = "Completed",
                    FinalAssessment = "Great job!",
                    FinalGrade = 92
                }
            };

            _queryExtensionsMock
                .Setup(e => e.ApplyIncludes(It.IsAny<IQueryable<Submission>>(), "MockProject", "SubmissionsClasses"))
                .Returns(submissionsFromDb.AsQueryable());

            _submissionRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<Submission, bool>>>(),
                    null,
                    It.IsAny<Expression<Func<IQueryable<Submission>, IQueryable<Submission>>>>(),
                    null,
                    null))
                .ReturnsAsync(submissionsFromDb);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Submissions retrieved successfully.");
            result.ResponseData.Should().NotBeNull();
            result.ResponseData.Should().HaveCount(2);

            result.ResponseData[0].Status.Should().Be(submissionsFromDb[0].Status);
            result.ResponseData[0].FinalGrade.Should().Be(submissionsFromDb[0].FinalGrade);
            result.ResponseData[1].FinalAssessment.Should().Be(submissionsFromDb[1].FinalAssessment);

            _queryExtensionsMock.Verify(e => e.ApplyIncludes(It.IsAny<IQueryable<Submission>>(), "MockProject", "SubmissionsClasses"), Times.Once);
            _submissionRepositoryMock.Verify(r => r.GetListAsync(
                It.IsAny<Expression<Func<Submission, bool>>>(),
                null,
                It.IsAny<Expression<Func<IQueryable<Submission>, IQueryable<Submission>>>>(),
                null,
                null), Times.Once);
        }

        [Fact]
        public async Task Handle_FilterByUserId_ReturnsOnlyThatUsersSubmissions()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var query = new GetSubmissionsQuery(UserId: targetUserId, ProjectId: null, Status: null);

            var submissions = new List<Submission>
            {
                new() { Id = Guid.NewGuid(), UserId = targetUserId, Status = "Pending" },
                new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = "Completed" }, // khác user
                new() { Id = Guid.NewGuid(), UserId = targetUserId, Status = "InProgress" }
            };

            var expected = submissions.Where(s => s.UserId == targetUserId).ToList();

            _queryExtensionsMock
                .Setup(e => e.ApplyIncludes(It.IsAny<IQueryable<Submission>>(), "MockProject", "SubmissionsClasses"))
                .Returns(submissions.AsQueryable());

            _submissionRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<Submission, bool>>>(),
                    null,
                    It.IsAny<Expression<Func<IQueryable<Submission>, IQueryable<Submission>>>>(),
                    null,
                    null))
                .ReturnsAsync(expected);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.ResponseData.Should().HaveCount(2);
            result.ResponseData.All(dto => dto.UserId == targetUserId).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_FilterByProjectIdAndStatus_ReturnsCorrectlyFiltered()
        {
            // Arrange
            var targetProjectId = Guid.NewGuid();
            var targetStatus = "Completed";
            var query = new GetSubmissionsQuery(UserId: null, ProjectId: targetProjectId, Status: targetStatus);

            var submissions = new List<Submission>
            {
                new() { MockProjectId = targetProjectId, Status = targetStatus },
                new() { MockProjectId = targetProjectId, Status = "Pending" },
                new() { MockProjectId = Guid.NewGuid(), Status = targetStatus },
                new() { MockProjectId = targetProjectId, Status = targetStatus }
            };

            var expected = new[] { submissions[0], submissions[3] };

            _queryExtensionsMock
                .Setup(e => e.ApplyIncludes(It.IsAny<IQueryable<Submission>>(), "MockProject", "SubmissionsClasses"))
                .Returns(submissions.AsQueryable());

            _submissionRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<Submission, bool>>>(),
                    null,
                    It.IsAny<Expression<Func<IQueryable<Submission>, IQueryable<Submission>>>>(),
                    null,
                    null))
                .ReturnsAsync(expected);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.ResponseData.Should().HaveCount(2);
            result.ResponseData.All(dto =>
                dto.MockProjectId == targetProjectId &&
                dto.Status == targetStatus).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_AllFiltersApplied_ReturnsMatchingSubmissionsOnly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var status = "Pending";
            var query = new GetSubmissionsQuery(UserId: userId, ProjectId: projectId, Status: status);

            var submissions = new List<Submission>
            {
                new() { UserId = userId, MockProjectId = projectId, Status = status },     // match
                new() { UserId = userId, MockProjectId = projectId, Status = "Completed" }, // sai status
                new() { UserId = userId, MockProjectId = Guid.NewGuid(), Status = status }, // sai project
                new() { UserId = Guid.NewGuid(), MockProjectId = projectId, Status = status } // sai user
            };

            var expected = new[] { submissions[0] };

            _queryExtensionsMock
                .Setup(e => e.ApplyIncludes(It.IsAny<IQueryable<Submission>>(), "MockProject", "SubmissionsClasses"))
                .Returns(submissions.AsQueryable());

            _submissionRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<Submission, bool>>>(),
                    null,
                    It.IsAny<Expression<Func<IQueryable<Submission>, IQueryable<Submission>>>>(),
                    null,
                    null))
                .ReturnsAsync(expected);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.ResponseData.Should().HaveCount(1);
            var dto = result.ResponseData![0];
            dto.UserId.Should().Be(userId);
            dto.MockProjectId.Should().Be(projectId);
            dto.Status.Should().Be(status);
        }

        [Fact]
        public async Task Handle_NoMatchingSubmissions_ReturnsEmptyListWithSuccess()
        {
            // Arrange
            var query = new GetSubmissionsQuery(UserId: Guid.NewGuid(), ProjectId: null, Status: null);

            _queryExtensionsMock
                .Setup(e => e.ApplyIncludes(It.IsAny<IQueryable<Submission>>(), "MockProject", "SubmissionsClasses"))
                .Returns(Enumerable.Empty<Submission>().AsQueryable());

            _submissionRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<Submission, bool>>>(),
                    null,
                    It.IsAny<Expression<Func<IQueryable<Submission>, IQueryable<Submission>>>>(),
                    null,
                    null))
                .ReturnsAsync(new List<Submission>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(200);
            result.Message.Should().Be("Submissions retrieved successfully.");
            result.ResponseData.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            var query = new GetSubmissionsQuery(UserId: null, ProjectId: null, Status: null);

            _submissionRepositoryMock
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<Submission, bool>>>(),
                    null,
                    It.IsAny<Expression<Func<IQueryable<Submission>, IQueryable<Submission>>>>(),
                    null,
                    null))
                .ThrowsAsync(new InvalidOperationException("Database timeout"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Status.Should().Be(500);
            result.Message.Should().StartWith("Failed to retrieve submissions:");
            result.Message.Should().Contain("Database timeout");
            result.ResponseData.Should().BeNull();
        }
    }
}