using DigitalHomeCinemaManager;
using System.Web.Http;
namespace WebApplication1
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected static void Application_Start()
        {

            SwaggerConfig.Register();

            GlobalConfiguration.Configure(config =>
            {
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            });
        }
    }
}