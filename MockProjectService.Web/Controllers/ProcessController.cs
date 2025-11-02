using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using System.ComponentModel.DataAnnotations;
using static MockProjectService.Contract.UseCases.Process.Query;
using static MockProjectService.Contract.UseCases.Process.Command;
using static MockProjectService.Contract.UseCases.Process.Request;

namespace MockProjectService.Web.Controllers
{
    /// <summary>
    /// Process management endpoints.
    /// </summary>
    [ApiVersion(1)]
    [Produces("application/json")]
    [ControllerName("Processes")]
    [Route("api/mockproject/processes")]
    public class ProcessController : ControllerBase
    {
        private readonly ISender _sender;

        public ProcessController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Retrieves details of a specific process.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves details of a process by its ID.
        /// If the process is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the process.</param>
        /// <returns>
        /// → <seealso cref="GetProcessQuery" /><br/>
        /// → <seealso cref="GetProcessQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{ProcessDto}"/> containing the process details.<br/>
        /// </returns>
        /// <response code="200">Process retrieved successfully.</response>
        /// <response code="404">The specified process was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<ProcessDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<ProcessDto>> GetProcess([FromRoute] Guid id)
        {
            var query = new GetProcessQuery(ProcessId: id);
            return await _sender.Send(query);
        }

        /// <summary>
        /// Updates a process.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint updates the details of an existing process.
        /// If the process is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the process to update.</param>
        /// <param name="request">A <see cref="UpdateProcessRequest"/> object containing the updated process details.</param>
        /// <returns>
        /// → <seealso cref="UpdateProcessCommand" /><br/>
        /// → <seealso cref="UpdateProcessCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the update was successful.<br/>
        /// </returns>
        /// <response code="200">Process updated successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid fields).</response>
        /// <response code="404">The specified process was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> UpdateProcess([FromRoute] Guid id, [FromBody, Required] UpdateProcessRequest request)
        {
            var command = new UpdateProcessCommand(
                ProcessId: id,
                StepNumber: request.StepNumber,
                StepGuiding: request.StepGuiding,
                BaseClassCode: request.BaseClassCode
            );

            return await _sender.Send(command);
        }

        /// <summary>
        /// Deletes a process.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint deletes a process by its ID.
        /// If the process is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the process to delete.</param>
        /// <returns>
        /// → <seealso cref="DeleteProcessCommand" /><br/>
        /// → <seealso cref="DeleteProcessCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the deletion was successful.<br/>
        /// </returns>
        /// <response code="200">Process deleted successfully.</response>
        /// <response code="404">The specified process was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> DeleteProcess([FromRoute] Guid id)
        {
            var command = new DeleteProcessCommand(ProcessId: id);
            return await _sender.Send(command);
        }
    }
}