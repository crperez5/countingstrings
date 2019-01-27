using System.Collections.Generic;
using Swashbuckle.AspNetCore.Examples;

namespace CountingStrings.API.SwaggerProviders
{
    public class SubmitWordsSwaggerProvider : IExamplesProvider
    {
        public object GetExamples()
        {
            return new List<string>
            {
                "chocolate-protein-shake-100072",
                "strawberry-protein-shake-100073",
                "vanilla-protein-shake-100074"
            };
        }
    }
}
