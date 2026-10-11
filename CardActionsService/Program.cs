
using CardActionsService.Models;
using CardActionsService.Services;
using CardActionsService.Validators;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace CardActionsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                AssemblyName assemblyName = assembly.GetName();

                var xmlFilename = $"{assemblyName.Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                Version? version = assemblyName.Version;
                string versionNumber = version?.ToString(3) ?? "0.0.0";   // Without 4th part of version string

                OpenApiContact openApiContact = new()
                {
                    Name = "Radosław Jaszek",
                    Email = "rjaszek@gmail.com",
                    Url = new Uri("https://radoslaw.jaszek.eu")
                };

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CardActionsService",
                    Contact = openApiContact,
                    Description = "Recruitment task for a .NET Developer position",
                    Version = versionNumber
                });
            });

            builder.Services.AddSingleton<ICardService, CardService>();
            builder.Services.AddSingleton<IRulesService, RulesService>();

            string errMessage = "Configuration error, PathToConditionsFile key is missing or empty";
            string? pathToConditionsFile = builder.Configuration["PathToConditionsFile"];
            if (string.IsNullOrWhiteSpace(pathToConditionsFile)) throw new InvalidOperationException(errMessage);

            builder.Configuration.AddJsonFile(pathToConditionsFile, optional: false, reloadOnChange: true);

            builder.Services
                .AddOptions<List<AllowedAction>>()
                .Bind(builder.Configuration.GetSection(AllowedAction.SectionName))
                .ValidateOnStart();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<List<AllowedAction>>, AllowedActionsValidator>());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
