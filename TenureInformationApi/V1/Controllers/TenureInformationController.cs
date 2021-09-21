using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Hackney.Core.Http;
using Hackney.Core.JWT;
using Hackney.Core.Logging;
using Hackney.Core.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TenureInformationApi.V1.Boundary.Requests;
using TenureInformationApi.V1.Boundary.Requests.Validation;
using TenureInformationApi.V1.Boundary.Response;
using TenureInformationApi.V1.Factories;
using TenureInformationApi.V1.Infrastructure;
using TenureInformationApi.V1.Infrastructure.Exceptions;
using TenureInformationApi.V1.UseCase.Interfaces;
using HeaderConstants = TenureInformationApi.V1.Infrastructure.HeaderConstants;

namespace TenureInformationApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/tenures")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class TenureInformationController : BaseController
    {
        private readonly IGetByIdUseCase _getByIdUseCase;
        private readonly IPostNewTenureUseCase _postNewTenureUseCase;
        private readonly IUpdateTenureForPersonUseCase _updateTenureForPersonUseCase;
        private readonly IEditTenureDetailsUseCase _editTenureDetailsUseCase;
        private readonly ITokenFactory _tokenFactory;
        private readonly IHttpContextWrapper _contextWrapper;
        public TenureInformationController(IGetByIdUseCase getByIdUseCase, IPostNewTenureUseCase postNewTenureUseCase, IUpdateTenureForPersonUseCase updateTenureForPersonUseCase, IEditTenureDetailsUseCase editTenureDetailsUseCase,
            ITokenFactory tokenFactory, IHttpContextWrapper contextWrapper)
        {
            _getByIdUseCase = getByIdUseCase;
            _postNewTenureUseCase = postNewTenureUseCase;
            _updateTenureForPersonUseCase = updateTenureForPersonUseCase;
            _editTenureDetailsUseCase = editTenureDetailsUseCase;
            _tokenFactory = tokenFactory;
            _contextWrapper = contextWrapper;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <response code="200">Successfully retrieved details for the specified ID</response>
        /// <response code="404">No tenure information found for the specified ID</response>
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(TenureResponseObject), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("{id}")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> GetByID([FromRoute] TenureQueryRequest query)
        {
            var result = await _getByIdUseCase.Execute(query).ConfigureAwait(false);
            if (result == null) return NotFound(query.Id);

            var eTag = string.Empty;
            if (result.VersionNumber.HasValue)
                eTag = result.VersionNumber.ToString();

            HttpContext.Response.Headers.Add(HeaderConstants.ETag, EntityTagHeaderValue.Parse($"\"{eTag}\"").Tag);

            return Ok(result.ToResponse());
        }

        [ProducesResponseType(typeof(TenureResponseObject), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> PostNewTenure([FromBody] CreateTenureRequestObject createTenureRequestObject)
        {
            var token = _tokenFactory.Create(_contextWrapper.GetContextRequestHeaders(HttpContext));

            var tenure = await _postNewTenureUseCase.ExecuteAsync(createTenureRequestObject, token).ConfigureAwait(false);
            return Created(new Uri($"api/v1/tenures/{tenure.Id}", UriKind.Relative), tenure);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [Route("{id}/person/{personid}")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> UpdateTenureForPerson([FromRoute] UpdateTenureRequest query, [FromBody] UpdateTenureForPersonRequestObject updateTenureRequestObject)
        {
            var token = _tokenFactory.Create(_contextWrapper.GetContextRequestHeaders(HttpContext));

            var tenure = await _updateTenureForPersonUseCase.ExecuteAsync(query, updateTenureRequestObject, token).ConfigureAwait(false);
            if (tenure == null) return NotFound(query.Id);

            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [Route("{id}")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> EditTenureDetails([FromRoute] TenureQueryRequest query, [FromBody] EditTenureDetailsRequestObject editTenureDetailsRequestObject)
        {
            // get raw body text (Only the parameters that need to be changed will be sent.
            // Deserializing the request object makes it imposible to figure out if the requester
            // wants to set a parameter to null, or to not update that value.
            // The bodyText is the raw request object that will be used to determine this information).
            var bodyText = await HttpContext.Request.GetRawBodyStringAsync().ConfigureAwait(false);
            var ifMatch = GetIfMatchFromHeader();

            var token = _tokenFactory.Create(_contextWrapper.GetContextRequestHeaders(HttpContext));

            try
            {
                var tenure = await _editTenureDetailsUseCase.ExecuteAsync(query, editTenureDetailsRequestObject, bodyText, token, ifMatch).ConfigureAwait(false);

                if (tenure == null) return NotFound();

                return NoContent();
            }
            catch (EditTenureInformationValidationException e)
            {
                // either only tenureStartDate or tenureEndDate have been updated, but fail validation against items in database             

                var response = BuildCustomEditTenureBadRequestResponse(e.ValidationResult);

                return BadRequest(response);
            }
            catch (VersionNumberConflictException vncErr)
            {
                return Conflict(vncErr.Message);
            }
        }

        private static CustomEditTenureDetailsBadRequestResponse BuildCustomEditTenureBadRequestResponse(ValidationResult validationResult)
        {
            var errorResponse = new Dictionary<string, List<string>>();

            foreach (var error in validationResult.Errors)
            {
                // create list at key if it doesnt exist
                if (!errorResponse.ContainsKey(error.PropertyName)) errorResponse.Add(error.PropertyName, new List<string>());

                var errorObject = new
                {
                    ErrorCode = error.ErrorCode,
                    ErrorMessage = error.ErrorMessage,
                    CustomState = error.CustomState
                };

                errorResponse[error.PropertyName].Add(JsonConvert.SerializeObject(errorObject));
            }

            return new CustomEditTenureDetailsBadRequestResponse
            {
                Errors = errorResponse
            };
        }

        private int? GetIfMatchFromHeader()
        {
            var header = HttpContext.Request.Headers.GetHeaderValue(HeaderConstants.IfMatch);

            if (header == null)
                return null;

            var eTag = EntityTagHeaderValue.TryParse(header, out var entityTagHeaderValue);

            if (entityTagHeaderValue == null)
                return null;

            var version = entityTagHeaderValue.Tag.Replace("\"", string.Empty);

            if (int.TryParse(version, out var numericValue))
                return numericValue;

            return null;
        }
    }
}
