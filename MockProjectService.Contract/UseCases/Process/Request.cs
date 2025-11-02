using System;

namespace MockProjectService.Contract.UseCases.Process
{
    public static class Request
    {
        public record DirectAddProcessRequest(Guid ProjectId, string StepGuiding, string? BaseClassCode);

        public record UpdateProcessRequest(int StepNumber, string StepGuiding, string? BaseClassCode);
    }
}