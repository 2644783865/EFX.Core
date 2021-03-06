﻿using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog.Extensions.Logging;
using NLog.Web;
using Tibos.Admin.Filters;
using Tibos.Confing.autofac;
using Tibos.Confing.automapper;
using Tibos.ConfingModel.model;
using Tibos.IService;
using Tibos.IService.Tibos;


namespace Tibos.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            var arry = new string[] {
            "                                             _ooOoo_",
            "                                            o8888888o",
            "                                            88\" . \"88",
            "                                            (| -_- |)",
            "                                            O\\  =  /O",
            "                                         ____/`---'\\____",
            "                                       .'  \\\\|     |//  `.",
            "                                      /  \\\\|||  :  |||//  \\",
            "                                     /  _||||| -:- |||||-  \\",
            "                                     |   | \\\\\\  -  /// |   |",
            "                                     | \\_|  ''\\---/''  |   |",
            "                                     \\  .-\\__  `-`  ___/-. /",
            "                                   ___`. .'  /--.--\\  `. . __",
            "                                .\"\" '<  `.___\\_<|>_/___.'  >'\"\".",
            "                               | | :  `- \\`.;`\\ _ /`;.`/ - ` : | |",
            "                               \\  \\ `-.   \\_ __\\ /__ _/   .-` /  /",
            "                          ======`-.____`-.___\\_____/___.-`____.-'======",
            "                                             `=---='",
            "                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^",
            "                                   佛祖保佑       永无BUG"
            };
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var item in arry)
            {
                Console.WriteLine(item);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Colorful.Console.WriteAscii("TibosAdmin");

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) //默认
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //Console.WriteLine("=========================================Autofac替换控制器所有者=============================================");
            //替换控制器所有者
            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());

            Console.WriteLine(">>>>>>>==================================注册AutoMapper===============================================<<<<<<<");
            //添加AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));
            Console.WriteLine(">>>>>>>==================================注册MVC过滤器,设置Json时间格式===============================<<<<<<<");

            //添加认证Cookie信息
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = new PathString("/home/login");
                    });
            Console.WriteLine(">>>>>>>==================================注册Authentication===========================================<<<<<<<");
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ResourceFilterAttribute));
                options.Filters.Add(typeof(ActionFilterAttribute));
                options.Filters.Add(typeof(ExceptionFilterAttribute));
                options.Filters.Add(typeof(ResultFilterAttribute));

            }).AddNewtonsoftJson(options =>
            {
                //忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //不使用驼峰样式的key
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //设置时间格式
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });

            services.AddTransient<HttpContextAccessor>();
            Console.WriteLine(">>>>>>>==================================注册MemoryCache==============================================<<<<<<<");
            //缓存
            services.AddMemoryCache();
            Console.WriteLine(">>>>>>>==================================注册跨域支持=================================================<<<<<<<");
            //配置跨域处理
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    builder.AllowAnyOrigin() //允许任何来源的主机访问
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();//指定处理cookie
                });
            });
            Console.WriteLine(">>>>>>>==================================注册全局配置文件=============================================<<<<<<<");
            //添加options
            services.AddOptions();
            services.Configure<ManageConfig>(Configuration.GetSection("ManageConfig"));
            Console.WriteLine(">>>>>>>==================================注册MVC======================================================<<<<<<<");
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            Console.WriteLine(">>>>>>>==================================注册Session==================================================<<<<<<<");
            //配置session的有效时间,单位秒
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(30); 
            });

            Console.WriteLine(">>>>>>>==================================注册权限验证=================================================<<<<<<<");
            //权限验证
            services.AddAuthorization();
            Console.WriteLine(">>>>>>>==================================Autofac注入底层模块==========================================<<<<<<<");
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });




            //var containerBuilder = new ContainerBuilder();

            //模块化注入
            //containerBuilder.RegisterModule<DefaultModule>();
            //属性注入控制器



            //注入所有Controller
            //containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).PropertiesAutowired().InstancePerDependency();


            //containerBuilder.Populate(services);
            //var container = containerBuilder.Build();

            Console.WriteLine("=========================================注册结束============================================================");
            //return new AutofacServiceProvider(container);


        }


        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).PropertiesAutowired().InstancePerDependency();
            builder.RegisterModule<DefaultModule>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,ILoggerFactory loggerFactory)
        {
            //this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            app.UseSession();
            //使用NLog作为日志记录工具
            loggerFactory.AddNLog();
            env.ConfigureNLog(AppContext.BaseDirectory + "config/nlog.config");//读取Nlog配置文件
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            //验证中间件
            app.UseAuthentication();

            app.UseRouting();

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "CMS",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "SYS",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            });
            Init(app);
        }

        private static void Init(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var _DictService = serviceScope.ServiceProvider.GetService<IDictService>();
                var _NavigationService = serviceScope.ServiceProvider.GetService<INavigationService>();
                var _Cache = serviceScope.ServiceProvider.GetService<IMemoryCache>();
                var list_dict = _DictService.GetList();

                var list_nav = _DictService.GetList();
                foreach (var item in list_dict)
                {
                    _Cache.Set($"dict_{item.Id}", item);
                }
                foreach (var item in list_nav)
                {
                    _Cache.Set($"nav_{item.Id}", item);
                }
            }
        }
    }
}
