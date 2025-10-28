using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using System.ComponentModel.DataAnnotations;
using static MockProjectService.Contract.UseCases.Submission.Command;
using static MockProjectService.Contract.UseCases.Submission.Query;
using static MockProjectService.Contract.UseCases.Submission.Request;

namespace MockProjectService.Web.Controllers
{
    /// <summary>
    /// Submission management endpoints.
    /// </summary>
    [ApiVersion(1)]
    [Produces("application/json")]
    [ControllerName("Submissions")]
    [Route("api/v1/[controller]")]
    public class SubmissionController : ControllerBase
    {
        private readonly ISender _sender;

        public SubmissionController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Retrieves a list of submissions with filters.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves a list of submissions filtered by user ID, project ID, or status.
        /// If the request is invalid, a 400 Bad Request response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="request">A <see cref="GetSubmissionsRequest"/> object containing filter parameters.</param>
        /// <returns>
        /// → <seealso cref="GetSubmissionsQuery" /><br/>
        /// → <seealso cref="GetSubmissionsQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{List{SubmissionDto}}"/> containing the list of submissions.<br/>
        /// </returns>
        /// <response code="200">Submissions retrieved successfully.</response>
        /// <response code="400">The request is invalid (e.g., invalid filter parameters).</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponseDto<List<SubmissionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<List<SubmissionDto>>> GetSubmissions([FromQuery] GetSubmissionsRequest request)
        {
            var query = new GetSubmissionsQuery(
                UserId: request.UserId,
                ProjectId: request.ProjectId,
                Status: request.Status
            );

            return await _sender.Send(query);
        }

        /// <summary>
        /// Retrieves details of a specific submission.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves details of a submission by its ID.
        /// If the submission is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission.</param>
        /// <returns>
        /// → <seealso cref="GetSubmissionQuery" /><br/>
        /// → <seealso cref="GetSubmissionQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{SubmissionDto}"/> containing the submission details.<br/>
        /// </returns>
        /// <response code="200">Submission retrieved successfully.</response>
        /// <response code="404">The specified submission was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<SubmissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<SubmissionDto>> GetSubmission([FromRoute] Guid id)
        {
            var query = new GetSubmissionQuery(SubmissionId: id);
            return await _sender.Send(query);
        }

        /// <summary>
        /// Creates a new submission.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint creates a new submission for a mock project.
        /// If the request is invalid, a 400 Bad Request response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="request">A <see cref="CreateSubmissionRequest"/> object containing the submission details.</param>
        /// <returns>
        /// → <seealso cref="CreateSubmissionCommand" /><br/>
        /// → <seealso cref="CreateSubmissionCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{string}"/> containing the ID of the created submission.<br/>
        /// </returns>
        /// <response code="200">Submission created successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid fields).</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<string>> CreateSubmission([FromBody, Required] CreateSubmissionRequest request)
        {
            var command = new CreateSubmissionCommand(ProjectId: request.ProjectId);
            return await _sender.Send(command);
        }

        /// <summary>
        /// Updates a submission status.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint updates the status and score of a submission.
        /// If the submission is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission to update.</param>
        /// <param name="request">A <see cref="UpdateSubmissionRequest"/> object containing the updated submission details.</param>
        /// <returns>
        /// → <seealso cref="UpdateSubmissionCommand" /><br/>
        /// → <seealso cref="UpdateSubmissionCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the update was successful.<br/>
        /// </returns>
        /// <response code="200">Submission updated successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid fields).</response>
        /// <response code="404">The specified submission was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> UpdateSubmission([FromRoute] Guid id, [FromBody, Required] UpdateSubmissionRequest request)
        {
            var command = new UpdateSubmissionCommand(
                SubmissionId: id,
                Status: request.Status,
                Grade: request.Grade,
                FinalAssessment: request.FinalAssessment
            );

            return await _sender.Send(command);
        }

        /// <summary>
        /// Resubmits a submission.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint resubmits an existing submission.
        /// If the submission is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission to resubmit.</param>
        /// <returns>
        /// → <seealso cref="ResubmitSubmissionCommand" /><br/>
        /// → <seealso cref="ResubmitSubmissionCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{string}"/> containing the ID of the resubmitted submission.<br/>
        /// </returns>
        /// <response code="200">Submission resubmitted successfully.</response>
        /// <response code="400">The request is invalid.</response>
        /// <response code="404">The specified submission was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost("{id}/resubmit")]
        [ProducesResponseType(typeof(BaseResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<string>> ResubmitSubmission([FromRoute] Guid id)
        {
            var command = new ResubmitSubmissionCommand(SubmissionId: id);
            return await _sender.Send(command);
        }

        /// <summary>
        /// Evaluates a submission.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint evaluates a submission and provides feedback.
        /// If the submission is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission to evaluate.</param>
        /// <param name="request">A <see cref="EvaluateRequest"/> object containing the feedback.</param>
        /// <returns>
        /// → <seealso cref="EvaluateSubmissionCommand" /><br/>
        /// → <seealso cref="EvaluateSubmissionCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the evaluation was successful.<br/>
        /// </returns>
        /// <response code="200">Submission evaluated successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid feedback).</response>
        /// <response code="404">The specified submission was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost("{id}/evaluate")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> EvaluateSubmission([FromRoute] Guid id, [FromBody] EvaluateRequest request)
        {
            var command = new EvaluateSubmissionCommand(SubmissionId: id, Feedback: request.Feedback);
            return await _sender.Send(command);
        }

        /// <summary>
        /// Retrieves all submission classes.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves a list of all submission classes.
        /// </pre>
        /// </remarks>
        /// <returns>
        /// → <seealso cref="GetSubmissionsClassesQuery" /><br/>
        /// → <seealso cref="GetSubmissionsClassesQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{List{SubmissionsClassDto}}"/> containing the list of submission classes.<br/>
        /// </returns>
        /// <response code="200">Submission classes retrieved successfully.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("classes")]
        [ProducesResponseType(typeof(BaseResponseDto<List<SubmissionsClassDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<List<SubmissionsClassDto>>> GetSubmissionsClasses()
        {
            var query = new GetSubmissionsClassesQuery();
            return await _sender.Send(query);
        }

        /// <summary>
        /// Retrieves a specific submission class.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves details of a submission class by its ID.
        /// If the submission class is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission class.</param>
        /// <returns>
        /// → <seealso cref="GetSubmissionsClassQuery" /><br/>
        /// → <seealso cref="GetSubmissionsClassQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{SubmissionsClassDto}"/> containing the submission class details.<br/>
        /// </returns>
        /// <response code="200">Submission class retrieved successfully.</response>
        /// <response code="404">The specified submission class was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("classes/{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<SubmissionsClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<SubmissionsClassDto>> GetSubmissionsClass([FromRoute] Guid id)
        {
            var query = new GetSubmissionsClassQuery(Id: id);
            return await _sender.Send(query);
        }

        /// <summary>
        /// Creates a new submission class.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint creates a new submission class.
        /// If the request is invalid, a 400 Bad Request response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="request">A <see cref="CreateSubmissionsClassRequest"/> object containing the submission class details.</param>
        /// <returns>
        /// → <seealso cref="CreateSubmissionsClassCommand" /><br/>
        /// → <seealso cref="CreateSubmissionsClassCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{string}"/> containing the ID of the created submission class.<br/>
        /// </returns>
        /// <response code="200">Submission class created successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid fields).</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost("classes")]
        [ProducesResponseType(typeof(BaseResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<string>> CreateSubmissionsClass([FromBody, Required] CreateSubmissionsClassRequest request)
        {
            var command = new CreateSubmissionsClassCommand(
                ProcessId: request.ProcessId,
                SubmissionId: request.SubmissionId,
                Code: request.Code,
                Status: request.Status
            );

            return await _sender.Send(command);
        }

        /// <summary>
        /// Updates a submission class.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint updates the details of an existing submission class.
        /// If the submission class is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission class to update.</param>
        /// <param name="request">A <see cref="UpdateSubmissionsClassRequest"/> object containing the updated submission class details.</param>
        /// <returns>
        /// → <seealso cref="UpdateSubmissionsClassCommand" /><br/>
        /// → <seealso cref="UpdateSubmissionsClassCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the update was successful.<br/>
        /// </returns>
        /// <response code="200">Submission class updated successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid fields).</response>
        /// <response code="404">The specified submission class was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPut("classes/{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> UpdateSubmissionsClass([FromRoute] Guid id, [FromBody, Required] UpdateSubmissionsClassRequest request)
        {
            var command = new UpdateSubmissionsClassCommand(
                Id: id,
                Grade: request.Grade,
                Assessment: request.Assessment,
                Code: request.Code,
                Status: request.Status
            );

            return await _sender.Send(command);
        }

        /// <summary>
        /// Deletes a submission class.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint deletes a submission class by its ID.
        /// If the submission class is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission class to delete.</param>
        /// <returns>
        /// → <seealso cref="DeleteSubmissionsClassCommand" /><br/>
        /// → <seealso cref="DeleteSubmissionsClassCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the deletion was successful.<br/>
        /// </returns>
        /// <response code="200">Submission class deleted successfully.</response>
        /// <response code="404">The specified submission class was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpDelete("classes/{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> DeleteSubmissionsClass([FromRoute] Guid id)
        {
            var command = new DeleteSubmissionsClassCommand(Id: id);
            return await _sender.Send(command);
        }

        /// <summary>
        /// Saves feedback for a submission.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint saves feedback for a submission.
        /// If the submission is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission.</param>
        /// <param name="request">A <see cref="SaveFeedbackRequest"/> object containing the feedback.</param>
        /// <returns>
        /// → <seealso cref="SaveFeedbackCommand" /><br/>
        /// → <seealso cref="SaveFeedbackCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the feedback was saved successfully.<br/>
        /// </returns>
        /// <response code="200">Feedback saved successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid feedback).</response>
        /// <response code="404">The specified submission was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost("{id}/feedback")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> SaveFeedback([FromRoute] Guid id, [FromBody, Required] SaveFeedbackRequest request)
        {
            var command = new SaveFeedbackCommand(SubmissionId: id, FinalAssessment: request.FinalAssessment);
            return await _sender.Send(command);
        }

        /// <summary>
        /// Retrieves the aggregated score for a submission.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves the aggregated score for a submission.
        /// If the submission is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the submission.</param>
        /// <returns>
        /// → <seealso cref="GetSubmissionScoreQuery" /><br/>
        /// → <seealso cref="GetSubmissionScoreQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{decimal}"/> containing the aggregated score.<br/>
        /// </returns>
        /// <response code="200">Score retrieved successfully.</response>
        /// <response code="404">The specified submission was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("{id}/score")]
        [ProducesResponseType(typeof(BaseResponseDto<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<double?>> GetSubmissionScore([FromRoute] Guid id)
        {
            var query = new GetSubmissionScoreQuery(SubmissionId: id);
            return await _sender.Send(query);
        }

        /// <summary>
        /// Retrieves statistics for submissions.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves aggregated statistics for submissions, filtered by user ID or status.
        /// </pre>
        /// </remarks>
        /// <param name="request">A <see cref="GetSubmissionsStatisticsRequest"/> object containing filter parameters.</param>
        /// <returns>
        /// → <seealso cref="GetSubmissionsStatisticsQuery" /><br/>
        /// → <seealso cref="GetSubmissionsStatisticsQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{SubmissionStatisticsDto}"/> containing the statistics.<br/>
        /// </returns>
        /// <response code="200">Statistics retrieved successfully.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(BaseResponseDto<SubmissionStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<SubmissionStatisticsDto>> GetSubmissionsStatistics([FromQuery] GetSubmissionsStatisticsRequest request)
        {
            var query = new GetSubmissionsStatisticsQuery(
                UserId: request.UserId,
                Status: request.Status
            );

            return await _sender.Send(query);
        }
    }
}