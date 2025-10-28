using MediatR;
using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;

namespace MockProjectService.Contract.UseCases.MockProject
{
    public static class Query
    {
        public record GetMockProjectsQuery(string? Language, string? Difficulty, int Page, int PageSize) : IQuery<BaseResponseDto<BasePagingDto<MockProjectDto>>>;

        public record GetMockProjectQuery(Guid ProjectId) : IQuery<BaseResponseDto<MockProjectDto>>;

        public record GetMockProjectProcessesQuery(Guid ProjectId) : IQuery<BaseResponseDto<List<ProcessDto>>>;

        public record GetMockProjectsStatisticsQuery : IQuery<BaseResponseDto<MockProjectStatisticsDto>>;

        public record GetTopSubmissionsQuery(Guid ProjectId, int Top) : IQuery<BaseResponseDto<List<SubmissionDto>>>;
    }
}