using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.MockProject.Command;

namespace MockProjectService.Core.Handler.MockProject.Command
{
    public class CreateMockProjectCommandHandler : ICommandHandler<CreateMockProjectCommand, BaseResponseDto<string>>
    {
        private readonly IGenericRepository<Domain.Entities.MockProject> _projectRepository;

        public CreateMockProjectCommandHandler(IGenericRepository<Domain.Entities.MockProject> projectRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<BaseResponseDto<string>> Handle(CreateMockProjectCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "Title cannot be null or empty.",
                    ResponseData = null
                };
            }

            if (string.IsNullOrWhiteSpace(request.Language))
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "Language cannot be null or empty.",
                    ResponseData = null
                };
            }

            if (string.IsNullOrWhiteSpace(request.Difficulty))
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "Difficulty cannot be null or empty.",
                    ResponseData = null
                };
            }

            if (string.IsNullOrWhiteSpace(request.KeyPrefix))
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "KeyPrefix cannot be null or empty.",
                    ResponseData = null
                };
            }

            try
            {
                using var transaction = await _projectRepository.BeginTransactionAsync();
                try
                {
                    var project = new Domain.Entities.MockProject
                    {
                        Id = Guid.NewGuid(),
                        Title = request.Title,
                        Language = request.Language,
                        Description = request.Description,
                        Difficulty = request.Difficulty,
                        FileName = request.FileName,
                        KeyPrefix = request.KeyPrefix,
                        BaseProjectUrl = request.BaseProjectUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdateAt = null
                    };

                    await _projectRepository.AddAsync(project);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<string>
                    {
                        Status = 200,
                        Message = "Mock project created successfully.",
                        ResponseData = project.Id.ToString()
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
                    Message = $"Failed to create mock project: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}