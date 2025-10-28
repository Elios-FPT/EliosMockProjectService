using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.MockProject.Command;

namespace MockProjectService.Core.Handler.MockProject.Command
{
    public class UpdateMockProjectCommandHandler : ICommandHandler<UpdateMockProjectCommand, BaseResponseDto<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;

        public UpdateMockProjectCommandHandler(IGenericRepository<Domain.Entities.MockProject> projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<BaseResponseDto<bool>> Handle(UpdateMockProjectCommand request, CancellationToken cancellationToken)
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

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return new BaseResponseDto<bool>
                {
                    Status = 400,
                    Message = "Title cannot be null or empty.",
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
                    project.Title = request.Title;
                    project.Language = request.Language;
                    project.Description = request.Description;
                    project.Difficulty = request.Difficulty;
                    project.FileName = request.FileName;
                    project.KeyPrefix = request.KeyPrefix;
                    project.BaseProjectUrl = request.BaseProjectUrl;
                    project.UpdateAt = DateTime.UtcNow;

                    await _projectRepository.UpdateAsync(project);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<bool>
                    {
                        Status = 200,
                        Message = "Mock project updated successfully.",
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
                    Message = $"Failed to update mock project: {ex.Message}",
                    ResponseData = false
                };
            }
        }
    }
}