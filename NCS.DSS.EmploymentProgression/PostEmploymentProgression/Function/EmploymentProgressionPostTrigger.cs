using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Validators;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression
{
    public class EmploymentProgressionPostTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions";
        const string FunctionName = "Post";
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionPostTriggerService _employmentProgressionPostTriggerService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IValidate _validate;
        private readonly IGeoCodingService _geoCodingService;
        private readonly IGuidHelper _guidHelper;

        public EmploymentProgressionPostTrigger(
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionPostTriggerService employmentProgressionPostTriggerService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILoggerHelper loggerHelper,
            IGeoCodingService geoCodingService,
            IGuidHelper guidHelper
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
            _guidHelper = guidHelper;
        }

        [FunctionName(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Employment progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this Employment progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Employment progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.EmploymentProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "Post", Description = "Ability to create a new employment progression for a customer. <br>" +
                                              "<br> <b>Validation Rules:</b> <br>" +
                                              "<br><b>EconomicShockCode:</b> Mandatory if EconomicShockStatus = 2 - Government defined economic shock. <br>" +
                                              "<br><b>EmploymentHours:</b> If CurrentEmployment status = 1, 4, 5, 8, 9 then the item must be a valid EmploymentHours reference data item<br>" +
                                              "<br><b>DateOfEmployment:</b> If CurrentEmployment status = 1, 4, 5, 8, 9 then the item is mandatory, ISO8601:2004 <= datetime.now <br>"
                                                )]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteValue)]HttpRequest req, ILogger logger, string customerId)
        {
            _loggerHelper.LogMethodEnter(logger);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var correlationGuid = _guidHelper.ValidateAndGetGuid(correlationId);

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

            if (!_guidHelper.IsValidGuid(customerId))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to parse 'customerId' to a Guid: {customerId}");
                return _httpResponseMessageHelper.BadRequest(customerId);
            }

            var customerGuid = _guidHelper.ValidateAndGetGuid(customerId);

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
            try
            {
                employmentProgressionRequest = await _httpRequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(req);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(logger, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(JObject.FromObject(new { Error = ex.Message }).ToString());
            }

            if (employmentProgressionRequest == null)
            {
                return _httpResponseMessageHelper.UnprocessableEntity();
            }

            _employmentProgressionPostTriggerService.SetIds(employmentProgressionRequest, customerGuid, touchpointId);
            _employmentProgressionPostTriggerService.SetDefaults(employmentProgressionRequest, touchpointId);

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
                    _employmentProgressionPostTriggerService.SetLongitudeAndLatitude(employmentProgressionRequest, position);

                }
                catch (Exception e)
                {
                    _loggerHelper.LogException(logger, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", employmentProgressionRequest.EmployerPostcode), e);
                }
            }

            var employmentProgressionResult = await _employmentProgressionPostTriggerService.CreateEmploymentProgressionAsync(employmentProgressionRequest);
            if (employmentProgressionResult == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to create Employment progression for customerId {customerGuid}, correlationId {correlationGuid}.");
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Sending newly created Employment Progression to service bus for customerId {customerGuid}, correlationId {correlationGuid}.");
            await _employmentProgressionPostTriggerService.SendToServiceBusQueueAsync(employmentProgressionRequest, ApimURL, correlationGuid, logger);

            _loggerHelper.LogMethodExit(logger);

            return employmentProgressionRequest == null ?
            _httpResponseMessageHelper.NoContent(customerGuid) :
            _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(employmentProgressionRequest, "id", "EmploymentProgressionId"));
        }
    }
}