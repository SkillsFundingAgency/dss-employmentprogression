using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
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
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IValidate _validate;
        private readonly ILogger<EmploymentProgressionPatchTrigger> _logger;
        private readonly IGeoCodingService _geoCodingService;
        private readonly IEmploymentProgressionPatchService _employmentProgressionPatchService;

        public EmploymentProgressionPatchTrigger(
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionPatchTriggerService employmentProgressionPatchTriggerService,
            IConvertToDynamic<Models.EmploymentProgression> convertToDynamic, 
            ICosmosDBProvider cosmosDbProvider,
            IValidate validate,
            ILogger<EmploymentProgressionPatchTrigger> logger,
            IGeoCodingService geoCodingService,
            IEmploymentProgressionPatchService employmentProgressionPatchService
            )
        {
            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionPatchTriggerService = employmentProgressionPatchTriggerService;
            _convertToDynamic = convertToDynamic;
            _cosmosDbProvider = cosmosDbProvider;
            _validate = validate;
            _logger = logger;
            _geoCodingService = geoCodingService;
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
            var functionName = nameof(EmploymentProgressionPatchTrigger);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (correlationId == null) {
                correlationId = Guid.NewGuid().ToString();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("{CorrelationId} Unable to locate 'TouchpointId' in request header.",correlationId);

                return new BadRequestResult();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogWarning("{CorrelationId} Unable to locate 'apimurl' in request header",correlationId);
                return new BadRequestResult();
            }           

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("{CorrelationId} Unable to parse 'customerId' to a Guid: {customerId}",correlationId,customerId);
                return new BadRequestObjectResult(customerId);
            }
            if (!Guid.TryParse(EmploymentProgressionId, out var employmentProgressionGuid))
            {
                _logger.LogWarning("{CorrelationId} Unable to parse 'employmentProgressionId' to a Guid: {EmploymentProgressionId}",correlationId,EmploymentProgressionId);
                return new BadRequestObjectResult(EmploymentProgressionId);
            }

            _logger.LogInformation("{CorrelationId} Input validation has succeeded.", correlationId);

            _logger.LogInformation("Attempting to see if customer exists. Customer GUID: {CustomerGuid}", customerGuid);
            if (!await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid))
            {
                _logger.LogWarning("{CorrelationId} Customer with [{customerGuid}] does not exist", correlationId, customerGuid);
                return new BadRequestResult();
            }

            _logger.LogInformation("Attempting to see if customer have a termination date (read only) or not. Customer GUID: {CustomerGuid}", customerGuid);

            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("{CorrelationId} Customer is readonly with customerId {customerGuid}.",correlationId,customerGuid);
                return new ObjectResult(customerGuid)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            EmploymentProgressionPatch employmentProgressionPatchRequest;
            _logger.LogInformation("{CorrelationId} Attempting to get resource from body of the request", correlationId);
            try
            {
                employmentProgressionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(req);
            }
            catch (Exception ex)
            {
                var eObject = _convertToDynamic.ExcludeProperty(ex, ["TargetSite"]);
                _logger.LogError("{CorrelationId} Unable to retrieve body from req. Exception {Exception}", correlationId, eObject);
                return new UnprocessableEntityObjectResult(eObject);
            }

            if (employmentProgressionPatchRequest == null)
            {
                _logger.LogWarning("{CorrelationId} A patch body was not provided.", correlationId);
                return new NoContentResult();
            }

            _logger.LogInformation("{CorrelationId} Attempting to set ids for Employment Progression Request. Customer GUID: {CustomerGuid}", correlationId, customerGuid);
            _employmentProgressionPatchTriggerService.SetIds(employmentProgressionPatchRequest, employmentProgressionGuid, touchpointId);

            _logger.LogInformation("{CorrelationId} Attempting to set defaults for Employment Progression Request. Customer GUID: {CustomerGuid}", correlationId, customerGuid);
            _employmentProgressionPatchTriggerService.SetDefaults(employmentProgressionPatchRequest);

            _logger.LogInformation("Attempting to validate Employment Progression Request. Customer GUID: {CustomerGuid}", customerGuid);

            var errors = _validate.ValidateResource(employmentProgressionPatchRequest);

            if (errors.Any())
            {
                _logger.LogWarning("{CorrelationId} validation errors with resource", correlationId);
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation("Employment Progression Request validation has succeeded. Customer GUID: {CustomerGuid}", customerGuid);


            _logger.LogInformation("{CorrelationId} Attempting to Check Employment Progression Exists for Customer. Customer GUID: {CustomerGuid}", correlationId, customerGuid);

            if (!await _employmentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(customerGuid))
            {
                _logger.LogWarning("{CorrelationId} Employment progression does not exist for customerId {customerGuid}.", correlationId, customerGuid);
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Employment progression exist for customerId {customerGuid}.", correlationId, customerGuid);
            }

            _logger.LogInformation("{CorrelationId} Attempting to Get Employment Progression with {ID} for Customer. Customer GUID: {CustomerGuid}", correlationId,employmentProgressionGuid, customerGuid);

            var currentEmploymentProgressionAsJson = await _employmentProgressionPatchTriggerService.GetEmploymentProgressionForCustomerToPatchAsync(customerGuid, employmentProgressionGuid);

            if (currentEmploymentProgressionAsJson == null)
            {
                _logger.LogWarning("{CorrelationId} Employment progression does not exist for {employmentProgressionGuid}.", correlationId, employmentProgressionGuid);
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Employment progression found with {ID} for customerId {customerGuid}.", correlationId,employmentProgressionGuid, customerGuid);
            }

            if (!string.IsNullOrEmpty(employmentProgressionPatchRequest.EmployerPostcode))
            {
                _logger.LogInformation("{CorrelationId} Attempting to get long and lat for postcode",correlationId);
                Position position;

                try
                {
                    var employerPostcode = employmentProgressionPatchRequest.EmployerPostcode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(employerPostcode);
                    _employmentProgressionPatchTriggerService.SetLongitudeAndLatitude(employmentProgressionPatchRequest, position);
                    _logger.LogInformation("{CorrelationId} Successfully retrieved long and lat for postcode", correlationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"{CorrelationId} Unable to get long and lat for postcode: {Postcode}. Exception: {Exception}",correlationId, employmentProgressionPatchRequest.EmployerPostcode, ex.Message);
                    throw;
                }
            }

            _logger.LogInformation("{CorrelationId} Attempting to Build Patch Employment Progression request with {ID} for customerId {customerGuid}.", correlationId, employmentProgressionGuid, customerGuid);

            var patchedEmploymentProgressionAsJson = _employmentProgressionPatchService.PatchEmploymentProgressionAsync(currentEmploymentProgressionAsJson, employmentProgressionPatchRequest);
            if (patchedEmploymentProgressionAsJson == null)
            {
                _logger.LogInformation("{CorrelationId} Employment progression does not exist for {employmentProgressionGuid}.");
                return new NoContentResult();
            }

            _logger.LogInformation("{CorrelationId} Attempting to Deserialize Patch Employment Progression request object with {ID} for customerId {customerGuid}.", correlationId, employmentProgressionGuid, customerGuid);

            Models.EmploymentProgression employmentProgressionValidationObject;
            try
            {
                employmentProgressionValidationObject = JsonConvert.DeserializeObject<Models.EmploymentProgression>(patchedEmploymentProgressionAsJson);
            }
            catch (Exception ex)
            {
                var eObject = _convertToDynamic.ExcludeProperty(ex, ["TargetSite"]);
                _logger.LogError("{CorrelationId} Unable to retrieve body from req. Excepton : {Exception}", eObject);
                throw;
            }
            
            if (employmentProgressionValidationObject == null)
            {
                _logger.LogInformation("{CorrelationId} Employment Progression Validation Object is null.");
                return new UnprocessableEntityObjectResult(req);
            }
            _logger.LogInformation("{CorrelationId} Attempting to Validate Patch Employment Progression request object with {ID} for customerId {customerGuid}.", correlationId, employmentProgressionGuid, customerGuid);

            var errorsList = _validate.ValidateResource(employmentProgressionValidationObject);
            if (errorsList != null && errorsList.Any())
            {
                _logger.LogInformation("{CorrelationId} validation errors with resource customerId {customerGuid}.");
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation("{CorrelationId} Attempting to Update Employment Progression with {ID} for customerId {customerGuid}.", correlationId, employmentProgressionGuid, customerGuid);

            var updatedEmploymentProgression = await _employmentProgressionPatchTriggerService.UpdateCosmosAsync(patchedEmploymentProgressionAsJson, employmentProgressionGuid);
            if (updatedEmploymentProgression == null)            
            {
                _logger.LogWarning("{CorrelationId} Failed to Update Employment Progression.",correlationId);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation("{CorrelationId} attempting to send to service bus {employmentProgressionGuid}.");                
                await _employmentProgressionPatchTriggerService.SendToServiceBusQueueAsync(updatedEmploymentProgression, customerGuid, ApimURL);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return new JsonResult(_convertToDynamic.RenameProperty(updatedEmploymentProgression, "id", "EmploymentProgressionId"))
                {
                    StatusCode = (int)HttpStatusCode.OK
                }; 
            }
        }
    }
}