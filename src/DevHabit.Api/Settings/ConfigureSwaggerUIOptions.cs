using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace DevHabit.Api.Settings;

public sealed class ConfigureSwaggerUIOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider) :
    IConfigureNamedOptions<SwaggerUIOptions>
{
    public void Configure(SwaggerUIOptions options)
    {
        foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
        }
    }
    public void Configure(string? name, SwaggerUIOptions options)
    {
        Configure(options);
    }
}
