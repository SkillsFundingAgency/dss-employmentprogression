using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Reflection;

namespace NCS.DSS.EmploymentProgression.APIDefinition
{
    public class GenerateEmploymentProgressionSwaggerDoc
    {
        public const string ApiTitle = "EmploymentProgressions";
        public const string ApiDefinitionName = "API-Definition";
        public const string ApiDefRoute = ApiTitle + "/" + ApiDefinitionName;
        public const string ApiDescription = "Basic details of a National Careers Service " + ApiTitle + " Resource";
        public const string Method = "get";

        private readonly ISwaggerDocumentGenerator _swaggerDocumentGenerator;
        public const string ApiVersion = "4.0.0";

        public GenerateEmploymentProgressionSwaggerDoc(ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            _swaggerDocumentGenerator = swaggerDocumentGenerator;
        }

        [Function(ApiDefinitionName)]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, Method, Route = ApiDefRoute)] HttpRequest req)
        {
            var swaggerDoc = _swaggerDocumentGenerator.GenerateSwaggerDocument(req, ApiTitle, ApiDescription,
                ApiDefinitionName, ApiVersion, Assembly.GetExecutingAssembly(), false);

            if (string.IsNullOrEmpty(swaggerDoc))
            {
                return new NoContentResult();
            }

            return new OkObjectResult(swaggerDoc);
        }
    }
}