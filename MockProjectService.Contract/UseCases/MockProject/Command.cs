using MediatR;
using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;

namespace MockProjectService.Contract.UseCases.MockProject
{
    public static class Command
    {
        public record CreateMockProjectCommand(string Title, string Language, string? Description, string Difficulty, string? FileName, string KeyPrefix, string? BaseProjectUrl) : ICommand<BaseResponseDto<string>>;

        public record UpdateMockProjectCommand(Guid ProjectId, string Title, string Language, string? Description, string Difficulty, string? FileName, string KeyPrefix, string? BaseProjectUrl) : ICommand<BaseResponseDto<bool>>;

        public record DeleteMockProjectCommand(Guid ProjectId) : ICommand<BaseResponseDto<bool>>;

        public record AddProcessCommand(Guid ProjectId, string StepGuiding, string? BaseClassCode) : ICommand<BaseResponseDto<string>>;

        public record AutoEvaluateCommand(Guid ProjectId, Guid SubmissionId) : ICommand<BaseResponseDto<bool>>;
    }
}