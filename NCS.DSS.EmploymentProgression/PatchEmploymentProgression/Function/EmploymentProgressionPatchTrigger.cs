using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using DFC.Swagger.Standard.Annotations;
using System.Net.Http;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.LearningProgression
{
    public class EmploymentProgressionPatchTrigger
    {
        const string RouteValue = "customers/{customerId}/employmentprogessions/{EmploymentProgessionId}";
        const string FunctionName = "patch";

        public EmploymentProgressionPatchTrigger()
        {
        }

        [FunctionName(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Employment progression updated.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer Resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Post request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this employment progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Employment progression validation error(s).", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to modify/update Employment Progression for a customer. <br>" +
                                              "<br> <b>Validation Rules:</b> <br>" +
                                              "<br><b>DateProgressionRecorded:</b> ISO8601:2004.  <= datetime.now <br>" +
                                              "<br><b>CurrentEmploymentStatus:</b> A valid EmploymentStatus from enum. <br>" +
                                              "<br><b>EconomicShockStatus:</b> A valid EconomicShockStatus from enum. <br>" +
                                              "<br><b>EconomicShockCode:</b> Mandatory if EconomicShockStatus = 2 - Government defined economic shock. <br>" +
                                              "<br><b>EmploymentHours:</b> A valid EconomicShockStatus from enum. If CurrentEmployment status = 1, 4, 5, 8, 9 then the item must be a valid EmploymentHours reference data item<br>" +
                                              "<br><b>DateOfEmployment:</b> If CurrentEmployment status = 1, 4, 5, 8, 9 then the item is mandatory, ISO8601:2004 <= datetime.now <br>" +
                                              "<br><b>DateOfLastEmployment:</b> ISO8601:2004 <= datetime.now<br>" +
                                              "<br><b>LengthOfUnemployment:</b> A valid LengthOfUnemployment from enum. <br>" +
                                               "<br><b>LastModifiedDate:</b> ISO8601:2004 <br>"
                                                )]
        [ProducesResponseType(typeof(EmploymentProgression.Models.EmploymentProgression), (int)HttpStatusCode.OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RouteValue)]HttpRequest req, ILogger logger, string customerId, string EmploymentProgessionId)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}