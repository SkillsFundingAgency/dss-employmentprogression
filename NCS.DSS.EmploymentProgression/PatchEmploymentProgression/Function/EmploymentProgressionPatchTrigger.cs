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
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Validators;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NCS.DSS.EmploymentProgression.Function
{
    public class EmploymentProgressionPatchTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions/{EmploymentProgressionId}";
        const string FunctionName = "Patch";

        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionPatchTriggerService _employmentProgressionPatchTriggerService;
        private readonly IConvertToDynamic<Models.EmploymentProgression> _convertToDynamic;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly ILogger<EmploymentProgressionPatchTrigger> _loggerHelper;
        private readonly IGeoCodingService _geoCodingService;
        private readonly IGuidHelper _guidHelper;
        private readonly IEmploymentProgressionPatchService _employmentProgressionPatchService;

        public EmploymentProgressionPatchTrigger(
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionPatchTriggerService employmentProgressionPatchTriggerService,
            IConvertToDynamic<Models.EmploymentProgression> convertToDynamic,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILogger<EmploymentProgressionPatchTrigger> loggerHelper,
            IGeoCodingService geoCodingService,
            IGuidHelper guidHelper,
            IEmploymentProgressionPatchService employmentProgressionPatchService
            )
        {
            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionPatchTriggerService = employmentProgressionPatchTriggerService;
            _convertToDynamic = convertToDynamic;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _geoCodingService = geoCodingService;
            _guidHelper = guidHelper;
            _employmentProgressionPatchService = employmentProgressionPatchService;
        }

        [Function(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Employment progression updated.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer Resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this employment progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Employment progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.EmploymentProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "Patch", Description = "Ability to modify/update employment progression for a customer. <br>" +
                                               "<br> <b>Validation Rules:</b> <br>" +
                                               "<br><b>EconomicShockCode:</b> Mandatory if EconomicShockStatus = 2 - Government defined economic shock. <br>" +
                                               "<br><b>EmploymentHours:</b> If CurrentEmployment status = 1, 4, 5, 8, 9 then the item must be a valid EmploymentHours reference data item<br>" +
                                               "<br><b>DateOfEmployment:</b> If CurrentEmployment status = 1, 4, 5, 8, 9 then the item is mandatory, ISO8601:2004 <= datetime.now <br>"
                                                )]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RouteValue)] HttpRequest req, string customerId, string EmploymentProgressionId)
        {
            _loggerHelper.LogInformation("Started EmploymentProgressionPatchTrigger");

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

            if (!_guidHelper.IsValidGuid(EmploymentProgressionId))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Unable to parse 'employmentProgressionId' to a Guid: {EmploymentProgressionId}");
                return new BadRequestObjectResult(EmploymentProgressionId);
            }

            var employmentProgressionGuid = _guidHelper.ValidateAndGetGuid(EmploymentProgressionId);
            EmploymentProgressionPatch employmentProgressionPatchRequest;

            try
            {
                employmentProgressionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(req);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogError($"[{correlationGuid}] Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (employmentProgressionPatchRequest == null)
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] A patch body was not provided. CorrelationId: {correlationGuid}.");
                return new NoContentResult();
            }

            _employmentProgressionPatchTriggerService.SetIds(employmentProgressionPatchRequest, employmentProgressionGuid, touchpointId);
            _employmentProgressionPatchTriggerService.SetDefaults(employmentProgressionPatchRequest);

            if (await _resourceHelper.IsCustomerReadOnly(customerGuid))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Customer is readonly with customerId {customerGuid}.");
                return new ObjectResult(customerGuid)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Customer with [{customerGuid}] does not exist");
                return new BadRequestResult();
            }

            if (!_employmentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(customerGuid))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Employment progression does not exist for customerId {customerGuid}.");
                return new NoContentResult();
            }

            var currentEmploymentProgressionAsJson = await _employmentProgressionPatchTriggerService.GetEmploymentProgressionForCustomerToPatchAsync(customerGuid, employmentProgressionGuid);

            if (currentEmploymentProgressionAsJson == null)
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Employment progression does not exist for {employmentProgressionGuid}.");
                return new NoContentResult();
            }

            if (!string.IsNullOrEmpty(employmentProgressionPatchRequest.EmployerPostcode))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Attempting to get long and lat for postcode");
                Position position;

                try
                {
                    var employerPostcode = employmentProgressionPatchRequest.EmployerPostcode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(employerPostcode);
                    _employmentProgressionPatchTriggerService.SetLongitudeAndLatitude(employmentProgressionPatchRequest, position);
                }
                catch (Exception ex)
                {
                    _loggerHelper.LogError($"[{correlationGuid}] Unable to get long and lat for postcode: {employmentProgressionPatchRequest.EmployerPostcode}. Exception: {_convertToDynamic.ExcludeProperty(ex, ["TargetSite"])}");
                    throw;
                }
            }

            var patchedEmploymentProgressionAsJson = _employmentProgressionPatchService.PatchEmploymentProgressionAsync(currentEmploymentProgressionAsJson, employmentProgressionPatchRequest);
            if (patchedEmploymentProgressionAsJson == null)
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Employment progression does not exist for {employmentProgressionGuid}.");
                return new NoContentResult();
            }

            Models.EmploymentProgression employmentProgressionValidationObject;
            try
            {
                employmentProgressionValidationObject = JsonConvert.DeserializeObject<Models.EmploymentProgression>(patchedEmploymentProgressionAsJson);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogError($"[{correlationGuid}] Unable to retrieve body from req", ex);
                _loggerHelper.LogError($"[{correlationGuid}] Excepton : {_convertToDynamic.ExcludeProperty(ex, ["TargetSite"])}");
                throw;
            }

            if (employmentProgressionValidationObject == null)
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Employment Progression Validation Object is null.");
                return new UnprocessableEntityObjectResult(req);
            }

            var errors = _validate.ValidateResource(employmentProgressionValidationObject);
            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] validation errors with resource customerId {customerGuid}.");
                return new UnprocessableEntityObjectResult(errors);
            }

            var updatedEmploymentProgression = await _employmentProgressionPatchTriggerService.UpdateCosmosAsync(patchedEmploymentProgressionAsJson, employmentProgressionGuid);
            if (updatedEmploymentProgression != null)
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] attempting to send to service bus {employmentProgressionGuid}.");

                await _employmentProgressionPatchTriggerService.SendToServiceBusQueueAsync(updatedEmploymentProgression, customerGuid, ApimURL, correlationGuid, _loggerHelper);
            }

            _loggerHelper.LogInformation("Exited EmploymentProgressionPatchTrigger");

            return updatedEmploymentProgression == null ?
            new NoContentResult() :
            new JsonResult(_convertToDynamic.RenameProperty(updatedEmploymentProgression, "id", "EmploymentProgressionId"))
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}