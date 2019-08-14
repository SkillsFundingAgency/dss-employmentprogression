using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Mvc;
using System;
using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using DFC.Common.Standard.Logging;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.EmploymentProgression
{
    public class EmploymentProgressionGetByIdTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogessions/{EmploymentProgressionId}";
        const string FunctionName = "GetById";

        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionGetByIdTriggerService _employmentProgressionGetByIdTriggerService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILoggerHelper _loggerHelper;

        public EmploymentProgressionGetByIdTrigger(
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionGetByIdTriggerService EmploymentProgressionGetByIdTriggerService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            ILoggerHelper loggerHelper
            )
        {
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionGetByIdTriggerService = EmploymentProgressionGetByIdTriggerService;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _loggerHelper = loggerHelper;
        }

        [FunctionName(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Employment progression found.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this Employment progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Employment progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.EmploymentProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual employment progression for the given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RouteValue)]
            HttpRequest req, ILogger logger, string customerId, string EmploymentProgressionId)
        {
            _loggerHelper.LogMethodEnter(logger);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var guidHelper = new GuidHelper();
            var correlationGuid = guidHelper.ValidateGuid(correlationId);

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

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to parse 'customerId' to a Guid: {customerId}");
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Bad request");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!Guid.TryParse(EmploymentProgressionId, out var employmentProgressionGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to parse 'employmentProgressioniD' to a Guid: {EmploymentProgressionId}");
                return _httpResponseMessageHelper.BadRequest(employmentProgressionGuid);
            }

            var employmentProgression = await _employmentProgressionGetByIdTriggerService.GetEmploymentProgressionForCustomerAsync(customerGuid, employmentProgressionGuid);

            _loggerHelper.LogMethodExit(logger);

            return employmentProgression == null ?
            _httpResponseMessageHelper.NoContent(customerGuid) :
            _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(employmentProgression, "id", "EmploymentProgressionId"));
        }
    }
}
