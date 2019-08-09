using Microsoft.Azure.Documents;

namespace NCS.DSS.EmploymentProgression.CosmosDocumentClient
{
    public interface ICosmosDocumentClient
    {
        IDocumentClient GetDocumentClient();
    }
}