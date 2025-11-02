using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.UseCases.Submission
{
    public static class Command
    {
        public record CreateSubmissionCommand(Guid UserId, Guid ProjectId) : ICommand<BaseResponseDto<string>>;

        public record UpdateSubmissionCommand(Guid SubmissionId, string Status, double? Grade, string? FinalAssessment) : ICommand<BaseResponseDto<bool>>;

        public record ResubmitSubmissionCommand(Guid SubmissionId) : ICommand<BaseResponseDto<string>>;

        public record EvaluateSubmissionCommand(Guid SubmissionId, string Feedback) : ICommand<BaseResponseDto<bool>>;

        public record CreateSubmissionsClassCommand(Guid ProcessId, Guid SubmissionId, string Code, string Status) : ICommand<BaseResponseDto<string>>;

        public record UpdateSubmissionsClassCommand(Guid Id, double? Grade, string? Assessment, string Code, string Status) : ICommand<BaseResponseDto<bool>>;

        public record DeleteSubmissionsClassCommand(Guid Id) : ICommand<BaseResponseDto<bool>>;

        public record SaveFeedbackCommand(Guid SubmissionId, string FinalAssessment) : ICommand<BaseResponseDto<bool>>;
    }
}
