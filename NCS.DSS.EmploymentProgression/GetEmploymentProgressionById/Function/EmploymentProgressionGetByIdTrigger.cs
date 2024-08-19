using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NCS.DSS.EmploymentProgression.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NCS.DSS.EmploymentProgression
{
    public class EmploymentProgressionGetByIdTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions/{EmploymentProgressionId}";
        const string FunctionName = "GetById";


        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionGetByIdTriggerService _employmentProgressionGetByIdTriggerService;
        private readonly IConvertToDynamic<Models.EmploymentProgression> _convertToDynamic;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<EmploymentProgressionGetByIdTrigger> _loggerHelper;
        private readonly IGuidHelper _guidHelper;

        public EmploymentProgressionGetByIdTrigger(
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionGetByIdTriggerService EmploymentProgressionGetByIdTriggerService,
            IConvertToDynamic<Models.EmploymentProgression> convertToDynamic,
            IResourceHelper resourceHelper,
            ILogger<EmploymentProgressionGetByIdTrigger> loggerHelper,
            IGuidHelper guidHelper
            )
        {

            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionGetByIdTriggerService = EmploymentProgressionGetByIdTriggerService;
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
        [Display(Name = "Get", Description = "Ability to retrieve an individual employment progression for the given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteValue)]
            HttpRequest req, string customerId, string EmploymentProgressionId)
        {
            _loggerHelper.LogInformation("Started EmploymentProgressionGetByIdTrigger");

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
                _loggerHelper.LogInformation($"[{correlationGuid}] Unable to locate 'ApimUrl' in request header.");
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
                _loggerHelper.LogInformation($"[{correlationGuid}] Customer with [{customerGuid}] does not exist");
                return new BadRequestResult();
            }

            if (!_guidHelper.IsValidGuid(EmploymentProgressionId))
            {
                _loggerHelper.LogInformation($"[{correlationGuid}] Unable to parse 'employmentProgressioniD' to a Guid: {EmploymentProgressionId}");
                return new BadRequestObjectResult(EmploymentProgressionId);
            }
            var employmentProgressionGuid = _guidHelper.ValidateAndGetGuid(EmploymentProgressionId);

            var employmentProgression = await _employmentProgressionGetByIdTriggerService.GetEmploymentProgressionForCustomerAsync(customerGuid, employmentProgressionGuid);

            _loggerHelper.LogInformation("Exiting EmploymentProgressionGetByIdTrigger");

            return employmentProgression == null ?
            new NoContentResult() :
            new JsonResult(_convertToDynamic.RenameProperty(employmentProgression, "id", "EmploymentProgressionId")) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
