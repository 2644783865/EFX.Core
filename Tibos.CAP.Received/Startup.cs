﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tibos.CAP.Received
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //配置跨域处理
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("default", builder =>
            //    {
            //        builder.AllowAnyOrigin() //允许任何来源的主机访问
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials();//指定处理cookie
            //    });
            //});

            services.AddCap(x =>
            {
                x.UseMySql("Data Source=132.232.4.73;Initial Catalog=CAP_DB;port=3307; User ID=root;Password=As123456;SslMode = none;");
                //x.UseKafka("132.232.4.73:9092");
                x.UseRabbitMQ(mq =>
                {
                    mq.HostName = "132.232.4.73";
                    mq.UserName = "guest";
                    mq.Password = "ghosts1t";
                }
                );
            });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseCors("default");
            app.UseMvc();
        }
    }
}
