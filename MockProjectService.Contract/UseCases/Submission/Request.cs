using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.UseCases.Submission
{
    public static class Request
    {
        public record GetSubmissionsRequest(Guid? UserId, Guid? ProjectId, string? Status);

        public record CreateSubmissionRequest(Guid ProjectId);

        public record UpdateSubmissionRequest(string Status, double? Grade, string? FinalAssessment);

        public record EvaluateRequest(string Feedback);

        public record CreateSubmissionClassRequest(Guid ProcessId, Guid SubmissionId, string Code, string Status);

        public record UpdateSubmissionClassRequest(double? Grade, string? Assessment, string Code, string Status);

        public record SaveFeedbackRequest(string FinalAssessment);

        public record GetSubmissionsStatisticsRequest(Guid? UserId, string? Status);
    }
}
