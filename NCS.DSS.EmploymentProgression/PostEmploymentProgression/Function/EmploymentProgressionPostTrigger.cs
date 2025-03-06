using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
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
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly ILogger<EmploymentProgressionPostTrigger> _logger;
        private readonly IValidate _validate;
        private readonly IGeoCodingService _geoCodingService;
        private readonly IConvertToDynamic<Models.EmploymentProgression> _convertToDynamic;

        public EmploymentProgressionPostTrigger(
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionPostTriggerService employmentProgressionPostTriggerService,
            ICosmosDBProvider cosmosDbProvider,
            IValidate validate,
            ILogger<EmploymentProgressionPostTrigger> logger,
            IGeoCodingService geoCodingService,
            IConvertToDynamic<Models.EmploymentProgression> convertToDynamic
            )
        {
            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionPostTriggerService = employmentProgressionPostTriggerService;
            _cosmosDbProvider = cosmosDbProvider;
            _validate = validate;
            _logger = logger;
            _geoCodingService = geoCodingService;
            _convertToDynamic = convertToDynamic;
        }

        [Function(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Employment progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this Employment progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Conflict, Description = "Resource already exists", ShowSchema = false)]
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
            var functionName = nameof(EmploymentProgressionPostTrigger);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (correlationId == null)
            {
                correlationId = Guid.NewGuid().ToString();
            }
            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);

            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("{CorrelationId} Unable to locate 'TouchpointId' in request header.", correlationId);
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header.");
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogWarning("{CorrelationId} Unable to locate 'apimurl' in request header", correlationId);
                return new BadRequestObjectResult("Unable to locate 'apimurl' in request header");
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("{CorrelationId} Unable to parse 'customerId' to a Guid: {customerId}", correlationId, customerId);
                return new BadRequestObjectResult($"Unable to parse 'customerId' to a Guid: {customerId}");
            }

            _logger.LogInformation("{CorrelationId} Input validation has succeeded.", correlationId);

            _logger.LogInformation("Attempting to see if customer exists. Customer GUID: {CustomerGuid}", customerGuid);

            if (!await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid))
            {
                _logger.LogWarning("{CorrelationId} Customer with ID {customerId} does not exist", correlationId, customerId);
                return new NotFoundObjectResult($"Customer with ID {customerId} does not exist.");
            }

            _logger.LogInformation("Attempting to see if customer have a termination date (read only) or not. Customer GUID: {CustomerGuid}", customerGuid);

            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("{CorrelationId} Customer is readonly with customerId {customerGuid}.", correlationId, customerGuid);
                return new ObjectResult($"Customer with ID {customerId} is readonly.") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            _logger.LogInformation("Attempting to see if customer have an Employment Progression Record or not. Customer GUID: {CustomerGuid}", customerGuid);

            var doesEmploymentProgressionExist = await _employmentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(customerGuid);
            if (doesEmploymentProgressionExist)
            {
                _logger.LogWarning("{CorrelationId} Employment progression details already exists for customerId {customerGuid}", correlationId, customerGuid);
                return new ConflictObjectResult($"Employment progression details already exists for customerId {customerId}");
            }

            _logger.LogInformation("{CorrelationId} Attempting to get resource from body of the request", correlationId);

            Models.EmploymentProgression employmentProgressionRequest;
            try
            {
                employmentProgressionRequest = await _httpRequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{CorrelationId} Unable to retrieve body from req. Exception {Error}", correlationId, ex.Message);
                return new UnprocessableEntityObjectResult($"Unable to retrieve body from request. Exception is \"{ex.Message}\"");
            }

            if (employmentProgressionRequest == null)
            {
                _logger.LogWarning("Resource returned NULL when extracted from request. CorrelationId: {CorrelationId}. CustomerId: {customerGuid}", correlationId, customerGuid);
                return new UnprocessableEntityObjectResult($"Please ensure data has been added to the request body. Resource returned NULL when extracted from request for customer {customerId}.");
            }
            _logger.LogInformation("Attempting to set ids for Employment Progression Request. Customer GUID: {CustomerGuid}", customerGuid);
            _employmentProgressionPostTriggerService.SetIds(employmentProgressionRequest, customerGuid, touchpointId);

            _logger.LogInformation("Attempting to set defaults for Employment Progression Request. Customer GUID: {CustomerGuid}", customerGuid);
            _employmentProgressionPostTriggerService.SetDefaults(employmentProgressionRequest, touchpointId);

            _logger.LogInformation("Attempting to validate Employment Progression Request. Customer GUID: {CustomerGuid}", customerGuid);

            var errors = _validate.ValidateResource(employmentProgressionRequest);

            if (errors.Any())
            {
                _logger.LogWarning("{CorrelationId} validation errors with resource", correlationId);
                return new UnprocessableEntityObjectResult(string.Join(";", errors));
            }

            _logger.LogInformation("Employment Progression Request validation has succeeded. Customer GUID: {CustomerGuid}", customerGuid);

            if (!string.IsNullOrEmpty(employmentProgressionRequest.EmployerPostcode))
            {
                _logger.LogInformation("{CorrelationId} Attempting to get long and lat for postcode", correlationId);
                Position position;

                try 
                { 
                    var employerPostcode = employmentProgressionRequest.EmployerPostcode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(employerPostcode);
                    _employmentProgressionPostTriggerService.SetLongitudeAndLatitude(employmentProgressionRequest, position);
                    _logger.LogInformation("{CorrelationId} Successfully retrieved long and lat for postcode", correlationId);

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to get long and lat for postcode: {Postcode}", employmentProgressionRequest.EmployerPostcode);
                }
            }

            _logger.LogInformation("Attempting to Create Employment Progression Request. Customer GUID: {CustomerGuid}", customerGuid);
            var employmentProgressionResult = await _employmentProgressionPostTriggerService.CreateEmploymentProgressionAsync(employmentProgressionRequest);
            if (employmentProgressionResult == null)            
            {
                _logger.LogWarning("{CorrelationId} Unable to create Employment progression for customerId {customerGuid}", correlationId, customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return new BadRequestObjectResult($"Unable to create Employment progression for customerId {customerGuid}");
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Successfully Created Employment Progression record with {ID} for customerId {customerGuid}", correlationId,employmentProgressionRequest.EmploymentProgressionId, customerGuid);

                _logger.LogInformation("{CorrelationId} Attempting to Send newly created Employment Progression to service bus for customerId {customerGuid}, correlationId {correlationGuid}.");
                await _employmentProgressionPostTriggerService.SendToServiceBusQueueAsync(employmentProgressionRequest, ApimURL);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return new JsonResult(_convertToDynamic.RenameProperty(employmentProgressionRequest, "id", "EmploymentProgressionId")) { StatusCode = (int)HttpStatusCode.Created };
            }
        }
    }
}