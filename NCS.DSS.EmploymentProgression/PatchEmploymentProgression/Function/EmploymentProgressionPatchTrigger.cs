using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using DFC.Swagger.Standard.Annotations;
using System.Net.Http;
using DFC.HTTP.Standard;
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.Validators;
using DFC.Common.Standard.Logging;
using System;
using Newtonsoft.Json;
using System.Linq;
using DFC.Common.Standard.GuidHelper;
using DFC.GeoCoding.Standard.AzureMaps.Model;
using NCS.DSS.EmployeeProgression.GeoCoding;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.EmploymentProgression.Function
{
    public class EmploymentProgressionPatchTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogressions/{EmploymentProgressionId}";
        const string FunctionName = "Patch";

        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IEmploymentProgressionPatchTriggerService _employmentProgressionPatchTriggerService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IGeoCodingService _geoCodingService;

        public EmploymentProgressionPatchTrigger(
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            IEmploymentProgressionPatchTriggerService employmentProgressionPatchTriggerService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILoggerHelper loggerHelper,
            IGeoCodingService geoCodingService
            )
        {
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _employmentProgressionPatchTriggerService = employmentProgressionPatchTriggerService;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _geoCodingService = geoCodingService;
        }

        [FunctionName(FunctionName)]
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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RouteValue)]HttpRequest req, ILogger logger, string customerId, string EmploymentProgressionId)
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

            if (!Guid.TryParse(EmploymentProgressionId, out var employmentProgressionGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to parse 'employmentProgressionId' to a Guid: {employmentProgressionGuid}");
                return _httpResponseMessageHelper.BadRequest(employmentProgressionGuid);
            }

            EmploymentProgressionPatch employmentProgressionPatchRequest;

            try
            {
                employmentProgressionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(req);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(logger, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(JObject.FromObject(new { Error = ex.Message }).ToString());
            }

            if (employmentProgressionPatchRequest == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"A patch body was not provided. CorrelationId: {correlationGuid}.");
                return _httpResponseMessageHelper.NoContent();
            }

            _employmentProgressionPatchTriggerService.SetIds(employmentProgressionPatchRequest, employmentProgressionGuid, touchpointId);

            if (await _resourceHelper.IsCustomerReadOnly(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Customer is readonly with customerId {customerGuid}.");
                return _httpResponseMessageHelper.Forbidden(customerGuid);
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Bad request");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!_employmentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Employment progression does not exist for customerId {customerGuid}.");
                return _httpResponseMessageHelper.NoContent();
            }

            var currentEmploymentProgressionAsJson = await _employmentProgressionPatchTriggerService.GetEmploymentProgressionForCustomerToPatchAsync(customerGuid, employmentProgressionGuid);

            if (currentEmploymentProgressionAsJson == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Employment progression does not exist for {employmentProgressionGuid}.");
                return _httpResponseMessageHelper.NoContent(employmentProgressionGuid);
            }

            var patchedEmploymentProgressionAsJson = _employmentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(currentEmploymentProgressionAsJson, employmentProgressionPatchRequest);
            if (patchedEmploymentProgressionAsJson == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Employment progression does not exist for {employmentProgressionGuid}.");
                return _httpResponseMessageHelper.NoContent(employmentProgressionGuid);
            }

            Models.EmploymentProgression employmentProgressionValidationObject;
            try
            {
                employmentProgressionValidationObject = JsonConvert.DeserializeObject<Models.EmploymentProgression>(patchedEmploymentProgressionAsJson);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(logger, correlationGuid, "Unable to retrieve body from req", ex);
                _loggerHelper.LogError(logger, correlationGuid, ex);
                throw;
            }

            if (employmentProgressionValidationObject == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Employment Progression Validation Object is null.");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            var errors = _validate.ValidateResource(employmentProgressionValidationObject);
            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"validation errors with resource customerId {customerGuid}.");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            if (!string.IsNullOrEmpty(employmentProgressionPatchRequest.EmployerPostcode))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Attempting to get long and lat for postcode");
                Position position;

                try
                {
                    var employerPostcode = employmentProgressionPatchRequest.EmployerPostcode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(employerPostcode);
                    _employmentProgressionPatchTriggerService.SetLongitudeAndLatitude(employmentProgressionPatchRequest, position);
                }
                catch (Exception e)
                {
                    _loggerHelper.LogException(logger, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", employmentProgressionPatchRequest.EmployerPostcode), e);
                    throw;
                }
            }



            var updatedEmploymentProgression = await _employmentProgressionPatchTriggerService.UpdateCosmosAsync(patchedEmploymentProgressionAsJson, employmentProgressionGuid);
            if (updatedEmploymentProgression != null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"attempting to send to service bus {employmentProgressionGuid}.");

                await _employmentProgressionPatchTriggerService.SendToServiceBusQueueAsync(updatedEmploymentProgression, ApimURL);
            }

            _loggerHelper.LogMethodExit(logger);

            return updatedEmploymentProgression == null ?
            _httpResponseMessageHelper.NoContent(customerGuid) :
            _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedEmploymentProgression, "id", "EmploymentProgressionId"));
        }
    }
}