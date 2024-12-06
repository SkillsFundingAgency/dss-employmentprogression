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
                return new BadRequestResult();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogWarning("{CorrelationId} Unable to locate 'apimurl' in request header", correlationId);
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("{CorrelationId} Unable to parse 'customerId' to a Guid: {customerId}", correlationId, customerId);
                return new BadRequestObjectResult(customerId);
            }

            if (!await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid))
            {
                _logger.LogWarning("{CorrelationId} Customer does not exist");
                return new NoContentResult();
            }

            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("{CorrelationId} Customer is readonly with customerId {customerGuid}.", correlationId, customerGuid);
                return new ObjectResult(customerGuid) { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            var doesEmploymentProgressionExist = await _employmentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(customerGuid);
            if (doesEmploymentProgressionExist)
            {
                _logger.LogWarning("{CorrelationId} Employment progression details already exists for customerId {customerGuid}", correlationId, customerGuid);
                return new ConflictResult();
            }

            _logger.LogInformation("{CorrelationId} Attempt to get resource from body of the request", correlationId);

            Models.EmploymentProgression employmentProgressionRequest;
            try
            {
                employmentProgressionRequest = await _httpRequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{CorrelationId} Unable to retrieve body from req. Exception {Error}", correlationId, ex.Message);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
            }

            if (employmentProgressionRequest == null)
            {
                _logger.LogWarning("{CorrelationId} Employment progression details does not exist for customerId {customerGuid}", correlationId, customerGuid);
                return new UnprocessableEntityResult();
            }

            _employmentProgressionPostTriggerService.SetIds(employmentProgressionRequest, customerGuid, touchpointId);
            _employmentProgressionPostTriggerService.SetDefaults(employmentProgressionRequest, touchpointId);

            var errors = _validate.ValidateResource(employmentProgressionRequest);

            if (errors.Any())
            {
                _logger.LogWarning("{CorrelationId} validation errors with resource", correlationId);
                return new UnprocessableEntityObjectResult(errors);
            }

            if (!string.IsNullOrEmpty(employmentProgressionRequest.EmployerPostcode))
            {
                _logger.LogInformation("{CorrelationId} Attempting to get long and lat for postcode", correlationId);
                Position position;

                try
                {
                    var employerPostcode = employmentProgressionRequest.EmployerPostcode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(employerPostcode);
                    _employmentProgressionPostTriggerService.SetLongitudeAndLatitude(employmentProgressionRequest, position);

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to get long and lat for postcode: {Postcode}", employmentProgressionRequest.EmployerPostcode);
                }
            }

            var employmentProgressionResult = await _employmentProgressionPostTriggerService.CreateEmploymentProgressionAsync(employmentProgressionRequest);
            if (employmentProgressionResult == null)
            {
                _logger.LogWarning("{CorrelationId} Unable to create Employment progression for customerId {customerGuid}", correlationId, customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Sending newly created Employment Progression to service bus for customerId {customerGuid}, correlationId {correlationGuid}.");
                await _employmentProgressionPostTriggerService.SendToServiceBusQueueAsync(employmentProgressionRequest, ApimURL);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return new JsonResult(_convertToDynamic.RenameProperty(employmentProgressionRequest, "id", "EmploymentProgressionId")) { StatusCode = (int)HttpStatusCode.Created };
            }
        }
    }
}