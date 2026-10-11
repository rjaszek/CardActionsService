using CardActionsService.Enums;
using CardActionsService.Models;
using CardActionsService.Tests.Enums;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace CardActionsService.Tests
{
    public static class Helpers
    {
        public const string ConfigFile = "data.json";

        public static IConfigurationRoot GetConfigurationRoot(string source, InputType inputType, Dictionary<string, string?>? shadowCollection = null)
        {
            ConfigurationBuilder configurationBuilder = new();

            switch (inputType)
            {
                case InputType.String:
                    using (MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(source)))
                    {
                        return configurationBuilder.AddJsonStream(memoryStream).AddInMemoryCollection(shadowCollection).Build();
                    }
                case InputType.File:
                    return configurationBuilder.AddJsonFile(source).AddInMemoryCollection(shadowCollection).Build();
                default:
                    throw new ArgumentException("Unknown inputType");
            }
        }

        public static AllowedAction CreateAllowedAction(string name, List<CardType> cardTypes, Dictionary<CardStatus, PinStatus> cardAndPinStatuses) => new()
        {
            Name = name,
            Conditions = new()
            {
                CardTypes = cardTypes,
                CardAndPinStatuses = cardAndPinStatuses
            }
        };
    }
}
