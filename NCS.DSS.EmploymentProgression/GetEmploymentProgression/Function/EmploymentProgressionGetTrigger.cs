using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NCS.DSS.EmploymentProgression
{
    public class EmploymentProgressionGetTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions";
        const string FunctionName = "Get";
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionGetTriggerService _EmploymentProgressionsGetTriggerService;
        private readonly IConvertToDynamic<Models.EmploymentProgression> _convertToDynamic;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<EmploymentProgressionGetTrigger> _loggerHelper;
        private readonly IGuidHelper _guidHelper;

        public EmploymentProgressionGetTrigger(
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionGetTriggerService EmploymentProgressionsGetTriggerService,
            IConvertToDynamic<Models.EmploymentProgression> convertToDynamic,
            IResourceHelper resourceHelper,
            ILogger<EmploymentProgressionGetTrigger> loggerHelper,
            IGuidHelper guidHelper
            )
        {
            _httpRequestHelper = httpRequestHelper;
            _EmploymentProgressionsGetTriggerService = EmploymentProgressionsGetTriggerService;
            _convertToDynamic = convertToDynamic;
            _resourceHelper = resourceHelper;
            _loggerHelper = loggerHelper;
            _guidHelper = guidHelper;
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
            _loggerHelper.LogInformation("Started EmploymentProgressionGetTrigger");

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

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Unable to parse 'customerId' to a Guid: {customerId}");
                return new BadRequestObjectResult(customerId);
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Customer [{customerGuid}] does not exist");
                return new NoContentResult();
            }

            var employmentProgression = await _EmploymentProgressionsGetTriggerService.GetEmploymentProgressionsForCustomerAsync(customerGuid);

            _loggerHelper.LogInformation("Exited EmploymentProgressionGetTrigger");

            return employmentProgression == null ?
                new NoContentResult() :
                new JsonResult(_convertToDynamic.RenameProperty(employmentProgression, "id", "EmploymentProgressionId")) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
