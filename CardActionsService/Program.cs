
using CardActionsService.Models;
using CardActionsService.Services;
using CardActionsService.Validators;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
            builder.Services.AddSwaggerGen();

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
