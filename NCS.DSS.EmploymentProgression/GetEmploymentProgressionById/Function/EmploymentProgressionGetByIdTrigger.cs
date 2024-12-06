using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NCS.DSS.EmploymentProgression.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NCS.DSS.EmploymentProgression
{
    public class EmploymentProgressionGetByIdTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions/{EmploymentProgressionId}";
        const string FunctionName = "GETBYID";


        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionGetByIdTriggerService _employmentProgressionGetByIdTriggerService;
        private readonly IConvertToDynamic<Models.EmploymentProgression> _convertToDynamic;
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly ILogger<EmploymentProgressionGetByIdTrigger> _logger;

        public EmploymentProgressionGetByIdTrigger(
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionGetByIdTriggerService EmploymentProgressionGetByIdTriggerService,
            IConvertToDynamic<Models.EmploymentProgression> convertToDynamic,
            ICosmosDBProvider cosmosDbProvider,
            ILogger<EmploymentProgressionGetByIdTrigger> logger
            )
        {

            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionGetByIdTriggerService = EmploymentProgressionGetByIdTriggerService;
            _convertToDynamic = convertToDynamic;
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        [Function(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Employment progression found.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this Employment progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Employment progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.EmploymentProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual employment progression for the given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteValue)]
            HttpRequest req, string customerId, string EmploymentProgressionId)
        {
            var functionName = nameof(EmploymentProgressionGetByIdTrigger);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogError("{CorrelationId} Unable to locate 'TouchpointId' in request header.", correlationId);
                return new BadRequestResult();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogError("{CorrelationId} Unable to locate 'ApimUrl' in request header.", correlationId);
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogError("{CorrelationId} Unable to parse 'customerId' to a Guid: {customerId}", correlationId,customerId);
                return new BadRequestObjectResult(customerId);
            }

            if (!await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid))
            {
                _logger.LogError("{CorrelationId} Customer with {CustomerGuid} does not exist", correlationId,customerGuid);
                return new BadRequestResult();
            }

            if (!Guid.TryParse(EmploymentProgressionId, out var employmentProgressionGuid))
            {
                _logger.LogError("{CorrelationId} Unable to parse 'employmentProgressioniD' to a Guid: {EmploymentProgressionId}", correlationId,EmploymentProgressionId);
                return new BadRequestObjectResult(EmploymentProgressionId);
            }
            var employmentProgression = await _employmentProgressionGetByIdTriggerService.GetEmploymentProgressionForCustomerAsync(customerGuid, employmentProgressionGuid);
            
            if( employmentProgression == null)
            {
                _logger.LogError("{CorrelationId} Employment Progress with {EmploymentProgressionId} does not exist", correlationId, employmentProgressionGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName); 
            return new JsonResult(_convertToDynamic.RenameProperty(employmentProgression, "id", "EmploymentProgressionId")) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
