using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.MockProject.Command;

namespace MockProjectService.Core.Handler.MockProject.Command
{
    public class DeleteMockProjectCommandHandler : ICommandHandler<DeleteMockProjectCommand, BaseResponseDto<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;

        public DeleteMockProjectCommandHandler(IGenericRepository<Domain.Entities.MockProject> projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<BaseResponseDto<bool>> Handle(DeleteMockProjectCommand request, CancellationToken cancellationToken)
        {
            if (request.ProjectId == Guid.Empty)
            {
                return new BaseResponseDto<bool>
                {
                    Status = 400,
                    Message = "Project ID cannot be empty.",
                    ResponseData = false
                };
            }

            try
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);
                if (project == null)
                {
                    return new BaseResponseDto<bool>
                    {
                        Status = 404,
                        Message = "Mock project not found.",
                        ResponseData = false
                    };
                }

                using var transaction = await _projectRepository.BeginTransactionAsync();
                try
                {
                    await _projectRepository.DeleteAsync(project);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<bool>
                    {
                        Status = 200,
                        Message = "Mock project deleted successfully.",
                        ResponseData = true
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
                return new BaseResponseDto<bool>
                {
                    Status = 500,
                    Message = $"Failed to delete mock project: {ex.Message}",
                    ResponseData = false
                };
            }
        }
    }
}