using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.UseCases.Process
{
    public static class Command
    {
        public record AddProcessCommand(Guid ProjectId, string StepGuiding, string? BaseClassCode) : ICommand<BaseResponseDto<string>>;

        public record UpdateProcessCommand(Guid ProcessId, string StepGuiding, string? BaseClassCode) : ICommand<BaseResponseDto<bool>>;

        public record DeleteProcessCommand(Guid ProcessId) : ICommand<BaseResponseDto<bool>>;
    }
}
