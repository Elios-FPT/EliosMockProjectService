using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockProjectService.Contract.UseCases.Process
{
    public static class Query
    {
        public record GetProcessQuery(Guid ProcessId) : IQuery<BaseResponseDto<ProcessDto>>;
    }
}
