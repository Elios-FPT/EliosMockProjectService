using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.MockProject.Command;

namespace MockProjectService.Core.Handler.MockProject.Command
{
    public class AddProcessCommandHandler : ICommandHandler<AddProcessCommand, BaseResponseDto<string>>
    {
        private readonly IGenericRepository<Domain.Entities.Process> _processRepository;
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;

        public AddProcessCommandHandler(
            IGenericRepository<Domain.Entities.Process> processRepository,
            IGenericRepository<Domain.Entities.MockProject> projectRepository)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<BaseResponseDto<string>> Handle(AddProcessCommand request, CancellationToken cancellationToken)
        {
            if (request.ProjectId == Guid.Empty)
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "Project ID cannot be empty.",
                    ResponseData = null
                };
            }

            if (string.IsNullOrWhiteSpace(request.StepGuiding))
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "StepGuiding cannot be null or empty.",
                    ResponseData = null
                };
            }

            try
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);
                if (project == null)
                {
                    return new BaseResponseDto<string>
                    {
                        Status = 404,
                        Message = "Mock project not found.",
                        ResponseData = null
                    };
                }

                using var transaction = await _processRepository.BeginTransactionAsync();
                try
                {
                    var process = new Domain.Entities.Process
                    {
                        Id = Guid.NewGuid(),
                        MockProjectId = request.ProjectId,
                        StepGuiding = request.StepGuiding,
                        BaseClassCode = request.BaseClassCode
                    };

                    await _processRepository.AddAsync(process);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<string>
                    {
                        Status = 200,
                        Message = "Process added successfully.",
                        ResponseData = process.Id.ToString()
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<string>
                {
                    Status = 500,
                    Message = $"Failed to add process: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}