using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NCS.DSS.EmploymentProgression
{
    public class EmploymentProgressionGetTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions";
        const string FunctionName = "GET";
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionGetTriggerService _EmploymentProgressionsGetTriggerService;
        private readonly IConvertToDynamic<Models.EmploymentProgression> _convertToDynamic;
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly ILogger<EmploymentProgressionGetTrigger> _logger;

        public EmploymentProgressionGetTrigger(
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionGetTriggerService EmploymentProgressionsGetTriggerService,
            IConvertToDynamic<Models.EmploymentProgression> convertToDynamic,
            ICosmosDBProvider cosmosDbProvider,
            ILogger<EmploymentProgressionGetTrigger> logger
            )
        {
            _httpRequestHelper = httpRequestHelper;
            _EmploymentProgressionsGetTriggerService = EmploymentProgressionsGetTriggerService;
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
        [Display(Name = "Get", Description = "Ability to return all employment progression for the given customer.")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteValue)]
            HttpRequest req, string customerId)
        {
            var functionName = nameof(EmploymentProgressionGetTrigger);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
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

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}", touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("{CorrelationId} Unable to parse 'customerId' to a Guid: {customerId}", correlationId, customerId);
                return new BadRequestObjectResult(customerId);
            }
            _logger.LogInformation("Attempting to see if customer exists. Customer GUID: {CustomerGuid}", customerGuid);
            var isExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);
            if (!isExist)
            {
                _logger.LogWarning("{CorrelationId} Customer {customerGuid} does not exist", correlationId, customerGuid);
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Customer with {CustomerId} found in Cosmos DB.",correlationId, customerGuid);
            }

            _logger.LogInformation("Attempting to Get Employment Progression. Customer GUID: {CustomerGuid}", customerGuid);

            var employmentProgression = await _EmploymentProgressionsGetTriggerService.GetEmploymentProgressionsForCustomerAsync(customerGuid);
            if (employmentProgression == null)
            {
                _logger.LogWarning("{CorrelationId} Employment Progressions for a Customer with ID {CustomerID} does not exist", correlationId, customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return new JsonResult(_convertToDynamic.RenameProperty(employmentProgression, "id", "EmploymentProgressionId")) { StatusCode = (int)HttpStatusCode.OK };
            }
            
        }
    }
}
