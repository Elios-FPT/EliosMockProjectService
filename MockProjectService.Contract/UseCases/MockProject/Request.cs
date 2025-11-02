using System;

namespace MockProjectService.Contract.UseCases.MockProject
{
    public static class Request
    {
        public record GetMockProjectsRequest(string? Language, string? Difficulty, int? Page, int? PageSize);

        public record CreateMockProjectRequest(string Title, string Language, string? Description, string Difficulty, string? FileName, string KeyPrefix, string? BaseProjectUrl);

        public record UpdateMockProjectRequest(string Title, string Language, string? Description, string Difficulty, string? FileName, string KeyPrefix, string? BaseProjectUrl);

        public record AddProcessRequest(int StepNumber, string StepGuiding, string? BaseClassCode);

        public record GetMockProjectsStatisticsRequest(Guid? UserId, Guid? ProjectId);

        public record AutoEvaluateRequest(Guid ProjectId, Guid SubmissionId);
    }
}