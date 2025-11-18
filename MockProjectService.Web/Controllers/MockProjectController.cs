using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MockProjectService.Contract.Shared;
using MockProjectService.Contract.TransferObjects;
using MockProjectService.Contract.UseCases.MockProject;
using MockProjectService.Web.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;
using static MockProjectService.Contract.UseCases.MockProject.Command;
using static MockProjectService.Contract.UseCases.MockProject.Query;
using static MockProjectService.Contract.UseCases.MockProject.Request;

namespace MockProjectService.Web.Controllers
{
    /// <summary>
    /// Mock project management endpoints.
    /// </summary>
    [ApiVersion(1)]
    [Produces("application/json")]
    [ControllerName("MockProjects")]
    [Route("api/mockproject/mock-projects")]
    public class MockProjectController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IConfiguration _configuration;

        public MockProjectController(ISender sender, IConfiguration configuration)
        {
            _sender = sender;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves a paginated list of mock projects with optional filters.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves a paginated list of mock projects, filtered by language, difficulty, page, and page size.
        /// </pre>
        /// </remarks>
        /// <param name="request">A <see cref="GetMockProjectsRequest"/> object containing filter parameters.</param>
        /// <returns>
        /// → <seealso cref="GetMockProjectsQuery" /><br/>
        /// → <seealso cref="GetMockProjectsQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{BasePagingDto{MockProjectDto}}"/> containing the list of mock projects.<br/>
        /// </returns>
        /// <response code="200">Mock projects retrieved successfully.</response>
        /// <response code="400">The request is invalid (e.g., invalid filter parameters).</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponseDto<BasePagingDto<MockProjectDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<BasePagingDto<MockProjectDto>>> GetMockProjects([FromQuery] GetMockProjectsRequest request)
        {
            var query = new GetMockProjectsQuery(
                Language: request.Language,
                Difficulty: request.Difficulty,
                Page: request.Page ?? 1,
                PageSize: request.PageSize ?? 10
            );

            return await _sender.Send(query);
        }

        /// <summary>
        /// Retrieves details of a specific mock project.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves details of a mock project by its ID.
        /// If the project is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the mock project.</param>
        /// <returns>
        /// → <seealso cref="GetMockProjectQuery" /><br/>
        /// → <seealso cref="GetMockProjectQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{MockProjectDto}"/> containing the mock project details.<br/>
        /// </returns>
        /// <response code="200">Mock project retrieved successfully.</response>
        /// <response code="404">The specified mock project was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<MockProjectDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<MockProjectDto>> GetMockProject([FromRoute] Guid id)
        {
            var query = new GetMockProjectQuery(ProjectId: id);
            return await _sender.Send(query);
        }

        /// <summary>
        /// Creates a new mock project.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint creates a new mock project with the provided details.
        /// If the request is invalid, a 400 Bad Request response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="request">A <see cref="CreateMockProjectRequest"/> object containing the project details.</param>
        /// <returns>
        /// → <seealso cref="CreateMockProjectCommand" /><br/>
        /// → <seealso cref="CreateMockProjectCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{string}"/> containing the ID of the created project.<br/>
        /// </returns>
        /// <response code="200">Mock project created successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid fields).</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost]
        [ServiceAuthorize("Resource Manager")]
        [ProducesResponseType(typeof(BaseResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<string>> CreateMockProject([FromBody, Required] CreateMockProjectRequest request)
        {
            var command = new CreateMockProjectCommand(
                Title: request.Title,
                Language: request.Language,
                Description: request.Description,
                Difficulty: request.Difficulty,
                FileName: request.FileName,
                KeyPrefix: request.KeyPrefix,
                BaseProjectUrl: request.BaseProjectUrl
            );

            return await _sender.Send(command);
        }

        /// <summary>
        /// Updates an existing mock project.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint updates the details of an existing mock project.
        /// If the project is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the mock project to update.</param>
        /// <param name="request">A <see cref="UpdateMockProjectRequest"/> object containing the updated project details.</param>
        /// <returns>
        /// → <seealso cref="UpdateMockProjectCommand" /><br/>
        /// → <seealso cref="UpdateMockProjectCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the update was successful.<br/>
        /// </returns>
        /// <response code="200">Mock project updated successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid fields).</response>
        /// <response code="404">The specified mock project was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> UpdateMockProject([FromRoute] Guid id, [FromBody, Required] UpdateMockProjectRequest request)
        {
            var command = new UpdateMockProjectCommand(
                ProjectId: id,
                Title: request.Title,
                Language: request.Language,
                Description: request.Description,
                Difficulty: request.Difficulty,
                FileName: request.FileName,
                KeyPrefix: request.KeyPrefix,
                BaseProjectUrl: request.BaseProjectUrl
            );

            return await _sender.Send(command);
        }

        /// <summary>
        /// Deletes a mock project.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint deletes a mock project by its ID.
        /// If the project is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the mock project to delete.</param>
        /// <returns>
        /// → <seealso cref="DeleteMockProjectCommand" /><br/>
        /// → <seealso cref="DeleteMockProjectCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the deletion was successful.<br/>
        /// </returns>
        /// <response code="200">Mock project deleted successfully.</response>
        /// <response code="404">The specified mock project was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> DeleteMockProject([FromRoute] Guid id)
        {
            var command = new DeleteMockProjectCommand(ProjectId: id);
            return await _sender.Send(command);
        }

        /// <summary>
        /// Retrieves the list of processes for a mock project.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves the list of processes associated with a mock project.
        /// If the project is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the mock project.</param>
        /// <returns>
        /// → <seealso cref="GetMockProjectProcessesQuery" /><br/>
        /// → <seealso cref="GetMockProjectProcessesQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{List{ProcessDto}}"/> containing the list of processes.<br/>
        /// </returns>
        /// <response code="200">Processes retrieved successfully.</response>
        /// <response code="404">The specified mock project was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("{id}/processes")]
        [ProducesResponseType(typeof(BaseResponseDto<List<ProcessDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<List<ProcessDto>>> GetMockProjectProcesses([FromRoute] Guid id)
        {
            var query = new GetMockProjectProcessesQuery(ProjectId: id);
            return await _sender.Send(query);
        }

        /// <summary>
        /// Adds a new process to a mock project.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint adds a new process to a mock project.
        /// If the project is not found or the request is invalid, appropriate error responses will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the mock project.</param>
        /// <param name="request">A <see cref="AddProcessRequest"/> object containing the process details.</param>
        /// <returns>
        /// → <seealso cref="AddProcessCommand" /><br/>
        /// → <seealso cref="AddProcessCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{string}"/> containing the ID of the created process.<br/>
        /// </returns>
        /// <response code="200">Process added successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid fields).</response>
        /// <response code="404">The specified mock project was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost("{id}/processes")]
        [ProducesResponseType(typeof(BaseResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<string>> AddMockProjectProcess([FromRoute] Guid id, [FromBody, Required] AddProcessRequest request)
        {
            var command = new AddProcessCommand(
                ProjectId: id,
                StepNumber: request.StepNumber,
                StepGuiding: request.StepGuiding,
                BaseClassCode: request.BaseClassCode
            );

            return await _sender.Send(command);
        }

        /// <summary>
        /// Retrieves statistics for all mock projects.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves aggregated statistics for all mock projects.
        /// </pre>
        /// </remarks>
        /// <returns>
        /// → <seealso cref="GetMockProjectsStatisticsQuery" /><br/>
        /// → <seealso cref="GetMockProjectsStatisticsQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{MockProjectStatisticsDto}"/> containing the statistics.<br/>
        /// </returns>
        /// <response code="200">Statistics retrieved successfully.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(BaseResponseDto<MockProjectStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<MockProjectStatisticsDto>> GetMockProjectsStatistics()
        {
            var query = new GetMockProjectsStatisticsQuery();
            return await _sender.Send(query);
        }

        /// <summary>
        /// Retrieves top submissions for a mock project.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint retrieves the top submissions for a mock project, limited by the specified number.
        /// If the project is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the mock project.</param>
        /// <param name="top">The number of top submissions to retrieve (default is 10).</param>
        /// <returns>
        /// → <seealso cref="GetTopSubmissionsQuery" /><br/>
        /// → <seealso cref="GetTopSubmissionsQueryHandler" /><br/>
        /// → A <see cref="BaseResponseDto{List{SubmissionDto}}"/> containing the top submissions.<br/>
        /// </returns>
        /// <response code="200">Top submissions retrieved successfully.</response>
        /// <response code="404">The specified mock project was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpGet("{id}/top-submissions")]
        [ProducesResponseType(typeof(BaseResponseDto<List<SubmissionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<List<SubmissionDto>>> GetTopSubmissions([FromRoute] Guid id, [FromQuery] int top = 10)
        {
            var query = new GetTopSubmissionsQuery(ProjectId: id, Top: top);
            return await _sender.Send(query);
        }

        /// <summary>
        /// Triggers auto-evaluation for a mock project submission.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint triggers an auto-evaluation for a submission in a mock project.
        /// If the project or submission is not found, a 404 Not Found response will be returned.
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the mock project.</param>
        /// <param name="request">A <see cref="AutoEvaluateRequest"/> object containing the submission ID.</param>
        /// <returns>
        /// → <seealso cref="AutoEvaluateCommand" /><br/>
        /// → <seealso cref="AutoEvaluateCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{bool}"/> indicating whether the evaluation was triggered successfully.<br/>
        /// </returns>
        /// <response code="200">Auto-evaluation triggered successfully.</response>
        /// <response code="400">The request is invalid (e.g., missing or invalid submission ID).</response>
        /// <response code="404">The specified mock project or submission was not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost("{id}/auto-evaluate")]
        [ProducesResponseType(typeof(BaseResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<bool>> AutoEvaluateMockProject([FromRoute] Guid id, [FromBody] AutoEvaluateRequest request)
        {
            var command = new AutoEvaluateCommand(ProjectId: id, SubmissionId: request.SubmissionId);
            return await _sender.Send(command);
        }

        /// <summary>
        /// Uploads a .zip file for a mock project and returns the public URL.
        /// </summary>
        /// <remarks>
        /// <pre>
        /// Description:
        /// This endpoint allows uploading a .zip file associated with a mock project.
        /// The file will be stored in the configured storage (e.g., AWS S3, Azure Blob, local disk),
        /// and a publicly accessible URL will be returned.
        /// 
        /// Requirements:
        /// - File must be a valid .zip
        /// - Max size: 50MB (configurable)
        /// - Only one .zip per project (overwrites previous)
        /// </pre>
        /// </remarks>
        /// <param name="id">The ID of the mock project.</param>
        /// <param name="file">The .zip file to upload.</param>
        /// <returns>
        /// → <seealso cref="UploadProjectZipCommand" /><br/>
        /// → <seealso cref="UploadProjectZipCommandHandler" /><br/>
        /// → A <see cref="BaseResponseDto{string}"/> containing the public URL of the uploaded file.<br/>
        /// </returns>
        /// <response code="200">File uploaded successfully. Returns public URL.</response>
        /// <response code="400">Invalid file (not .zip, too large, etc.).</response>
        /// <response code="404">Mock project not found.</response>
        /// <response code="500">Internal server error (storage failure, etc.).</response>
        [HttpPost("upload-zip")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(BaseResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<BaseResponseDto<string>> UploadProjectZip(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    ResponseData = null,
                    Message = "No file uploaded."
                };

            if (Path.GetExtension(file.FileName).ToLower() != ".zip")
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    ResponseData = null,
                    Message = "Only .zip files are allowed."
                };

            if (file.Length > 50 * 1024 * 1024)
                return new BaseResponseDto<string>
                {
                    Status = 400,
                    ResponseData = null,
                    Message = "File size exceeds 50MB limit."
                };

            var client = new HttpClient();

            string keyPrefix = $"mockproject";

            string originalName = Path.GetFileNameWithoutExtension(file.FileName);
            string fileName = originalName + "." + Guid.NewGuid().ToString();

            string extension = Path.GetExtension(file.FileName);

            string baseUtility = _configuration["Utility:baseUtilityUrl"];

            var url = $"{baseUtility}/api/v1/Storage?KeyPrefix={keyPrefix}&FileName={fileName}";

            using var form = new MultipartFormDataContent();

            using (var stream = file.OpenReadStream())
            {
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                form.Add(fileContent, "file", fileName);

                var response = await client.PostAsync(url, form);

                if (!response.IsSuccessStatusCode)
                {
                    return new BaseResponseDto<string>
                    {
                        Status = 400,
                        ResponseData = null,
                        Message = $"Upload failed: {response.StatusCode}"
                    };
                }

                var data = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(data);
                string urlOnly = doc.RootElement.GetProperty("responseData").GetString();

                return new BaseResponseDto<string>
                {
                    Status = 200,
                    ResponseData = urlOnly + extension,
                    Message = "Upload successful"
                };
            }
        }

    }
}