using MockProjectService.Contract.Message;
using MockProjectService.Contract.Shared;
using MockProjectService.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MockProjectService.Contract.UseCases.Submission.Command;

namespace MockProjectService.Core.Handler.Submission.Command
{
    public class CreateSubmissionsClassCommandHandler : ICommandHandler<CreateSubmissionsClassCommand, BaseResponseDto<string>>
    {
        private readonly IGenericRepository<Domain.Entities.SubmissionsClass> _classRepository;
        private readonly IGenericRepository<Domain.Entities.Process> _processRepository;
        private readonly IGenericRepository<Domain.Entities.Submission> _submissionRepository;

        public CreateSubmissionsClassCommandHandler(
            IGenericRepository<Domain.Entities.SubmissionsClass> classRepository,
            IGenericRepository<Domain.Entities.Process> processRepository,
            IGenericRepository<Domain.Entities.Submission> submissionRepository)
        {
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        }

        public async Task<BaseResponseDto<string>> Handle(CreateSubmissionsClassCommand request, CancellationToken cancellationToken)
        {
            if (request.ProcessId == Guid.Empty || request.SubmissionId == Guid.Empty)
            {
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    Message = "Process ID and Submission ID are required.",
                    ResponseData = null
                };
            }

            try
            {
                var process = await _processRepository.GetByIdAsync(request.ProcessId);
                var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId);
                if (process == null || submission == null)
                {
                    return new BaseResponseDto<string>
                    {
                        Status = 400,
                        Message = "Process or Submission not found.",
                        ResponseData = null
                    };
                }

                using var transaction = await _classRepository.BeginTransactionAsync();
                try
                {
                    var entity = new Domain.Entities.SubmissionsClass
                    {
                        Id = Guid.NewGuid(),
                        ProcessId = request.ProcessId,
                        SubmissionId = request.SubmissionId,
                        Code = request.Code,
                        Status = request.Status
                    };

                    await _classRepository.AddAsync(entity);
                    await transaction.CommitAsync();

                    return new BaseResponseDto<string>
                    {
                        Status = 200,
                        Message = "Submission class created successfully.",
                        ResponseData = entity.Id.ToString()
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
                    Message = $"Failed to create class: {ex.Message}",
                    ResponseData = null
                };
            }
        }
    }
}