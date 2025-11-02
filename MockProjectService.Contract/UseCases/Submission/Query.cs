using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.UseCases.Submission
{
    public static class Query
    {
        public record GetSubmissionsQuery(Guid? UserId, Guid? ProjectId, string? Status) : IQuery<BaseResponseDto<List<SubmissionDto>>>;

        public record GetSubmissionQuery(Guid SubmissionId) : IQuery<BaseResponseDto<SubmissionDto>>;

        public record GetSubmissionsClassesQuery : IQuery<BaseResponseDto<List<SubmissionsClassDto>>>;

        public record GetSubmissionsClasseaBySubmissionQuery(Guid? SubmissionId) : IQuery<BaseResponseDto<List<SubmissionsClassDto>>>;

        public record GetSubmissionsClassQuery(Guid Id) : IQuery<BaseResponseDto<SubmissionsClassDto>>;

        public record GetSubmissionScoreQuery(Guid SubmissionId) : IQuery<BaseResponseDto<double?>>;

        public record GetSubmissionsStatisticsQuery(Guid? UserId, string? Status) : IQuery<BaseResponseDto<SubmissionStatisticsDto>>;
    }
}
