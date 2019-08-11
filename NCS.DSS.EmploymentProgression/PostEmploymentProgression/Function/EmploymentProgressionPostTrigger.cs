using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using DFC.Swagger.Standard.Annotations;
using System.Net.Http;
using System;
using DFC.HTTP.Standard;
using DFC.Common.Standard.GuidHelper;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.Validators;
using System.Linq;
using DFC.Common.Standard.Logging;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using DFC.GeoCoding.Standard.AzureMaps.Model;
using NCS.DSS.EmployeeProgression.GeoCoding;

namespace NCS.DSS.EmploymentProgression
{
    public class EmploymentProgressionPostTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions";
        const string FunctionName = "post";
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionPostTriggerService _employmentProgressionPostTriggerService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IValidate _validate;
        private readonly IGeoCodingService _geoCodingService;

        public EmploymentProgressionPostTrigger(
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionPostTriggerService employmentProgressionPostTriggerService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILoggerHelper loggerHelper,
            IGeoCodingService geoCodingService
            )
        {
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionPostTriggerService = employmentProgressionPostTriggerService;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _geoCodingService = geoCodingService;
        }
        
        [FunctionName(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Employment progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Post request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this Employment progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Employment progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.EmploymentProgression), (int)HttpStatusCode.OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteValue)]HttpRequest req, ILogger logger, string customerId)
        {
            _loggerHelper.LogMethodEnter(logger);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var guidHelper = new GuidHelper();
            var correlationGuid = guidHelper.ValidateGuid(correlationId);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Unable to locate 'TouchpointId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to parse 'customerId' to a Guid: {customerId}");
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Bad request");
                return _httpResponseMessageHelper.NoContent();
            }

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Customer is readonly with customerId {customerGuid}, correlationId {correlationGuid}.");
                return _httpResponseMessageHelper.Forbidden(customerGuid);
            }

            var doesEmploymentProgressionExist = _employmentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(customerGuid);
            if (doesEmploymentProgressionExist)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Employment progression details already exists for customerId {customerGuid}, correlationId {correlationGuid}.");
                return _httpResponseMessageHelper.Conflict();
            }

            _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Attempt to get resource from body of the request correlationId {correlationGuid}.");

            Models.EmploymentProgression employmentProgressionRequest;
            employmentProgressionRequest = await _httpRequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(req);
            if (employmentProgressionRequest == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"A post body was not provided. CorrelationId: {correlationGuid}.");
                return _httpResponseMessageHelper.NoContent();
            }

            _employmentProgressionPostTriggerService.SetIds(employmentProgressionRequest, customerGuid, touchpointId);

            var errors = _validate.ValidateResource(employmentProgressionRequest);

            if (errors.Any())
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"validation errors with resource correlationId {correlationGuid}");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            if (!string.IsNullOrEmpty(employmentProgressionRequest.EmployerPostcode))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Attempting to get long and lat for postcode");
                Position position;

                try
                {
                    var employerPostcode = employmentProgressionRequest.EmployerPostcode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(employerPostcode);

                }
                catch (Exception e)
                {
                    _loggerHelper.LogException(logger, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", employmentProgressionRequest.EmployerPostcode), e);
                    throw;
                }

                _employmentProgressionPostTriggerService.SetLongitudeAndLatitude(employmentProgressionRequest, position);
            }

            var employmentProgressionResult = await _employmentProgressionPostTriggerService.CreateEmploymentProgressionAsync(employmentProgressionRequest);
            if (employmentProgressionResult == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to create Employment progression for customerId {customerGuid}, correlationId {correlationGuid}.");
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Sending newly created Employment Progression to service bus for customerId {customerGuid}, correlationId {correlationGuid}.");
            await _employmentProgressionPostTriggerService.SendToServiceBusQueueAsync(employmentProgressionRequest, ApimURL);

            _loggerHelper.LogMethodExit(logger);

            return employmentProgressionRequest == null ?
            _httpResponseMessageHelper.NoContent(customerGuid) :
            _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(employmentProgressionRequest, "id", "EmploymentProgressionId"));
        }
    }
}