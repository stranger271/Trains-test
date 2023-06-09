using Ninject;
using Ninject.Modules;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;



namespace Trains
{
    public class WebApiApplication : System.Web.HttpApplication
    {   

        protected void Application_Start()
        {

            //AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);            
            
            NinjectModule registrations = new NinjectRegistrations();
            var kernel = new StandardKernel(registrations);            
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);

     

        }
    }
}
