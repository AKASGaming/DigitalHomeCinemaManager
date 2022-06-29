using System.Web.Http;
using WebActivatorEx;
using DigitalHomeCinemaManager;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace DigitalHomeCinemaManager
{
    public static class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c => c.SingleApiVersion("v1", "DigitalHomeCinemaManager"))
                .EnableSwaggerUi();
        }
    }
}
