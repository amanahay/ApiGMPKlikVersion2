using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace ApiGMPKlik.Infrastructure
{
    public class RemoveVersionParameterProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var operation = context.OperationDescription.Operation;

            // Hapus parameter api-version dari query string
            var versionParam = operation.Parameters
                .FirstOrDefault(p => p.Name == "api-version" && p.Kind == OpenApiParameterKind.Query);

            if (versionParam != null)
            {
                operation.Parameters.Remove(versionParam);
            }

            return true;
        }
    }
}
