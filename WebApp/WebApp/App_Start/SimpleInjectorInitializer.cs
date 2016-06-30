using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Integration.Web.Mvc;
using SimpleInjector.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WebApp.App_Start;
using WebApp.Core.Data;
using WebApp.Core.Services;
using WebApp.Models;
using WebActivatorEx;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(SimpleInjectorInitializer), "Initialize")]

namespace WebApp.App_Start
{
    public static class SimpleInjectorInitializer
    {
        public static void Initialize()
        {
            var container = GetConfiguredContainer();
            Prepare(container);
        }

        public static Container GetConfiguredContainer()
        {
            var container = new Container();
            container.Options.PropertySelectionBehavior = new DependencyPropertySelectionBehavior();
            container.Options.ConstructorResolutionBehavior =
                new GreediestConstructorBehavior(container.Options.ConstructorResolutionBehavior);

            RegisterMvcServices(container);

            return container;
        }

        private static void Prepare(Container container)
        {
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container)); // MVC
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(GetAndRegisterApiContainer()); // Web API

        }

        private class GreediestConstructorBehavior : IConstructorResolutionBehavior
        {
            private readonly IConstructorResolutionBehavior _defaultBehavior;

            public GreediestConstructorBehavior(IConstructorResolutionBehavior defaultBehavior)
            {
                _defaultBehavior = defaultBehavior;
            }

            public ConstructorInfo GetConstructor(Type serviceType, Type implementationType)
            {
                try
                {
                    var greediest =
                        implementationType.GetConstructors()
                            .OrderByDescending(c => c.GetParameters().Length)
                            .FirstOrDefault();
                    return greediest ?? _defaultBehavior.GetConstructor(serviceType, implementationType);
                }
                catch (ActivationException)
                {
                    return null;
                }
            }
        }

        private class DependencyPropertySelectionBehavior : IPropertySelectionBehavior
        {
            public bool SelectProperty(Type type, PropertyInfo prop)
            {
                return prop.GetCustomAttributes(typeof(Attribute)).Any();
            }
        }

        private static Container GetAndRegisterApiContainer()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();
            RegisterUniversal(container);
            container.RegisterSingleton<IReadOnlyCollection<TimeZoneInfo>>(() => TimeZoneInfo.GetSystemTimeZones());
            //container.Register<IRepository<Settings>, Repository<Settings>>(Lifestyle.Scoped);
            //container.Register<IRepository<Config>, Repository<Config>>(Lifestyle.Scoped);
            //container.Register<IRepository<Translations>, Repository<Translations>>(Lifestyle.Scoped);
            //container.Register<IRestProvider<ISettings>, RestSettingsProvider<Settings, Config>>(Lifestyle.Scoped);
            //container.Register<IRestProvider<ITranslations>, RestTranslationsProvider<Translations>>(Lifestyle.Scoped);
            //container.Register<ISettings>(() => container.GetInstance<IRestProvider<ISettings>>().Get(HttpContext.Current.Request.Path), Lifestyle.Scoped);
            //container.Register<ITranslations>(() => container.GetInstance<IRestProvider<ITranslations>>().Get(HttpContext.Current.Request.Path), Lifestyle.Scoped);
            //container.Register<ISpredfastService, SpredfastService>();
            //container.Register<IStoredProcedureService, StoredProcedureService>();
            //container.Register<IEntitiesStoredProcedures, MainDB>();
            //container.Register<ICoreFlowService, CoreFlowService>();
            //container.Register<IProjectService, ProjectService>();
            //container.Register<IDateTimeProvider, DateTimeProvider>(Lifestyle.Scoped);
            //container.Register<IPromotionStateProvider, PromotionStateProvider>(Lifestyle.Scoped);
            //container.Register<IMovieService, MovieService>();
            //container.Register(() => new GoogleDrive(), Lifestyle.Scoped);
            //container.Register(() => new ConfigSynchronizationTask(container.GetInstance<GoogleDrive>()
            //    , container.GetInstance<ISettings>()
            //    , ConfigurationManager.ConnectionStrings["MainDB_Raw"].ConnectionString
            //    , ConfigurationManager.AppSettings["Url:Global:Settings"]), Lifestyle.Scoped);
            //container.Register(() => new SettingsSynchronizationTask(container.GetInstance<GoogleDrive>()
            //    , ConfigurationManager.ConnectionStrings["MainDB_Raw"].ConnectionString
            //    , ConfigurationManager.AppSettings["Url:Localized:Settings"]), Lifestyle.Scoped);
            //container.Register(() => new TranslationsSynchronizationTask(container.GetInstance<GoogleDrive>()
            //    , ConfigurationManager.ConnectionStrings["MainDB_Raw"].ConnectionString
            //    , ConfigurationManager.AppSettings["Url:Translations"]), Lifestyle.Scoped);

            return container;
        }

        private static void RegisterMvcServices(Container container)
        {
            #region Core
            RegisterUniversal(container);

            //container.RegisterPerWebRequest(() => GetGlobalPaymentServiceConfiguration(container));
            //container.RegisterPerWebRequest(() => GetPromoVeritasServiceConfiguration(container));

            container.RegisterPerWebRequest<HttpMessageInvoker>(() => new HttpClient());
            #endregion

            //#region Repository
            //container.RegisterPerWebRequest<IRepository<Settings>, Repository<Settings>>();
            //container.RegisterPerWebRequest<IRepository<Config>, Repository<Config>>();
            //container.RegisterPerWebRequest<IRepository<Translations>, Repository<Translations>>();
            //container.RegisterPerWebRequest<IRestProvider<ISettings>, RestSettingsProvider<Settings, Config>>();
            //container.RegisterPerWebRequest<IRestProvider<ITranslations>, RestTranslationsProvider<Translations>>();
            //container.RegisterPerWebRequest<ISettings>(() => container.GetInstance<IRestProvider<ISettings>>().Get(HttpContext.Current.Request.Path));
            //container.RegisterPerWebRequest<ITranslations>(() => container.GetInstance<IRestProvider<ITranslations>>().Get(HttpContext.Current.Request.Path));
            //#endregion

            #region Authentication

            //Identity
            container.Register<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();
            container.Register<IAuthenticationManager>(
                () => HttpContext.Current.GetOwinContext().Authentication);
            #endregion

            #region StoredProcedure service
            //container.RegisterPerWebRequest<IStoredProcedureService, StoredProcedureService>();
            //container.RegisterPerWebRequest<IEntitiesStoredProcedures, MainDB>();
            #endregion

            //container.RegisterPerWebRequest<ISpredfastService, SpredfastService>();

            //#region flow services
            //container.RegisterMvcIntegratedFilterProvider();
            //container.RegisterPerWebRequest<ICoreFlowService, CoreFlowService>();
            //#endregion
        }

        private static void RegisterUniversal(Container container)
        {
            var cacheTime = int.Parse(ConfigurationManager.AppSettings["CacheTimeSeconds"]);
            container.RegisterPerWebRequest<DbContext, MainDB>();
            //container.RegisterSingleton<ILogger, ElmahLogger>();
            //container.RegisterSingleton<ILocalizationResolver>(() => new RelativePathLocalizationResolver("^[a-zA-Z]{2}((-|_)[a-zA-Z]{2})?$"));
            //container.RegisterSingleton(() => new ProviderConfiguration(cacheTime, ConfigurationManager.AppSettings["Environment"]));
            //container.RegisterSingleton<ICache>(() => new InHttpContextCache(cacheTime));
        }

        //private static ISymmetricAlgorithm RegisterAes(Container container)
        //{
        //    var settings = container.GetInstance<ISettings>();
        //    return new AES(settings["AES:Key"], settings["AES:Vector"]);
        //}

        //private static IDictionary<string, string> RegisterPLTConfiguration(Container container)
        //{
        //    var settings = container.GetInstance<ISettings>();
        //    return settings.Where(x => x.Key.ToLower().StartsWith("oneflow") || x.Key.ToLower().StartsWith("marco")).ToDictionary(x => x.Key, x => x.Value);
        //}
    }
}