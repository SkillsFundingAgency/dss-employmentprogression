using DFC.Common.Standard.GuidHelper;
using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Validators;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NCS.DSS.EmploymentProgression
{
    public class EmploymentProgressionPostTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions";
        const string FunctionName = "Post";
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionPostTriggerService _employmentProgressionPostTriggerService;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<EmploymentProgressionPostTrigger> _loggerHelper;
        private readonly IValidate _validate;
        private readonly IGeoCodingService _geoCodingService;
        private readonly IGuidHelper _guidHelper;
        private readonly IConvertToDynamic<Models.EmploymentProgression> _convertToDynamic;

        public EmploymentProgressionPostTrigger(
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionPostTriggerService employmentProgressionPostTriggerService,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILogger<EmploymentProgressionPostTrigger> loggerHelper,
            IGeoCodingService geoCodingService,
            IGuidHelper guidHelper,
            IConvertToDynamic<Models.EmploymentProgression> convertToDynamic
            )
        {
            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionPostTriggerService = employmentProgressionPostTriggerService;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _geoCodingService = geoCodingService;
            _guidHelper = guidHelper;
            _convertToDynamic = convertToDynamic;
        }

        [Function(FunctionName)]
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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RouteValue)] HttpRequest req, string customerId)
        {
            _loggerHelper.LogInformation("Started EmploymentProgressionPostTrigger");

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var correlationGuid = _guidHelper.ValidateAndGetGuid(correlationId);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);

            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Unable to locate 'TouchpointId' in request header.");
                return new BadRequestResult();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Unable to locate 'apimurl' in request header");
                return new BadRequestResult();
            }

            if (!_guidHelper.IsValidGuid(customerId))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Unable to parse 'customerId' to a Guid: {customerId}");
                return new BadRequestObjectResult(customerId);
            }

            var customerGuid = _guidHelper.ValidateAndGetGuid(customerId);

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Customer does not exist");
                return new NoContentResult();
            }

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Customer is readonly with customerId {customerGuid}, correlationId {correlationGuid}.");
                return new ObjectResult(customerGuid) { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            var doesEmploymentProgressionExist = _employmentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(customerGuid);
            if (doesEmploymentProgressionExist)
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Employment progression details already exists for customerId {customerGuid}, correlationId {correlationGuid}.");
                return new ConflictResult();
            }

            _loggerHelper.LogInformation($"[{correlationGuid}] Attempt to get resource from body of the request correlationId {correlationGuid}.");

            Models.EmploymentProgression employmentProgressionRequest;
            try
            {
                employmentProgressionRequest = await _httpRequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(req);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogError($"[{correlationGuid}] Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
            }

            if (employmentProgressionRequest == null)
            {
                return new UnprocessableEntityResult();
            }

            _employmentProgressionPostTriggerService.SetIds(employmentProgressionRequest, customerGuid, touchpointId);
            _employmentProgressionPostTriggerService.SetDefaults(employmentProgressionRequest, touchpointId);

            var errors = _validate.ValidateResource(employmentProgressionRequest);

            if (errors.Any())
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] validation errors with resource correlationId {correlationGuid}");
                return new UnprocessableEntityObjectResult(errors);
            }

            if (!string.IsNullOrEmpty(employmentProgressionRequest.EmployerPostcode))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Attempting to get long and lat for postcode");
                Position position;

                try
                {
                    var employerPostcode = employmentProgressionRequest.EmployerPostcode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(employerPostcode);
                    _employmentProgressionPostTriggerService.SetLongitudeAndLatitude(employmentProgressionRequest, position);

                }
                catch (Exception e)
                {
                    _loggerHelper.LogError(string.Format("Unable to get long and lat for postcode: {0}", employmentProgressionRequest.EmployerPostcode), e);
                }
            }

            var employmentProgressionResult = await _employmentProgressionPostTriggerService.CreateEmploymentProgressionAsync(employmentProgressionRequest);
            if (employmentProgressionResult == null)
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Unable to create Employment progression for customerId {customerGuid}, correlationId {correlationGuid}.");
                return new NoContentResult();
            }

            _loggerHelper.LogInformation($"[{correlationGuid}] Sending newly created Employment Progression to service bus for customerId {customerGuid}, correlationId {correlationGuid}.");
            await _employmentProgressionPostTriggerService.SendToServiceBusQueueAsync(employmentProgressionRequest, ApimURL, correlationGuid, _loggerHelper);

            _loggerHelper.LogInformation("Exited EmploymentProgressionPostTrigger");

            return employmentProgressionRequest == null ?
            new NoContentResult() :
            new JsonResult(_convertToDynamic.RenameProperty(employmentProgressionRequest, "id", "EmploymentProgressionId")) { StatusCode = (int)HttpStatusCode.Created };
        }
    }
}