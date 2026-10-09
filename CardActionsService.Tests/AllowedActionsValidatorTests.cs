using CardActionsService.Models;
using CardActionsService.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace CardActionsService.Tests
{
    public class AllowedActionsValidatorTests
    {
        private enum InputType
        {
            String,
            File
        }

        [Fact]
        public void Validate_FileConfig_Success()
        {
            var result = GetValidateOptionsResult("data.json", InputType.File);
            Assert.True(result.Succeeded);
        }

        [Theory]
        [InlineData("""
            {
              "AllowedActions": [
                  {
                    "Name": "ACTION1",
                    "Conditions": {
                      "CardKind": [
                        "Prepaid",
                        "Debit",
                        "Credit"
                      ],
                      "CardStatus": {
                        "Active": "Any"
                      }
                    }
                  },
                  {
                    "Name": "ACTION2",
                    "Conditions": {
                      "CardKind": [
                        "Prepaid",
                        "Debit",
                        "Credit"
                      ],
                      "CardStatus": {
                        "Inactive": "Any"
                      }
                    }
                  }
              ]
            }
            """)]
        [InlineData("""
            {
              "AllowedActions": [
                {
                  "Name": "ACTION3",
                  "Conditions": {
                    "CardKind": [
                      "Prepaid",
                      "Debit",
                      "Credit"
                    ],
                    "CardStatus": {
                      "Ordered": "Any",
                      "Inactive": "Any",
                      "Active": "Any",
                      "Restricted": "Any",
                      "Blocked": "Any",
                      "Expired": "Any",
                      "Closed": "Any"
                    }
                  }
                }
              ]
            }
            """)]
        public void Validate_MinimalConfig_Success(string json)
        {
            var result = GetValidateOptionsResult(json);
            Assert.True(result.Succeeded);
        }

        [Theory]
        [InlineData("""
            {
              "AllowedActions": []
            }
            """)]
        [InlineData("""
            {
              "AllowedActionsaaa": [
                {
                  "Name": "ACTION3",
                  "Conditions": {
                    "CardKind": [
                      "Prepaid",
                      "Debit",
                      "Credit"
                    ],
                    "CardStatus": {
                      "Ordered": "Any",
                      "Inactive": "Any",
                      "Active": "Any",
                      "Restricted": "Any",
                      "Blocked": "Any",
                      "Expired": "Any",
                      "Closed": "Any"
                    }
                  }
                }
              ]
            }
            """)]
        public void Validate_EmptyOrMissingAllowedActionsSection_Failure(string json)
        {
            var result = GetValidateOptionsResult(json);
            Assert.Contains(result.Failures, f => f.Contains("There is no allowed action or configuration section is missing"));
        }

        [Theory]
        [InlineData("""
            {
              "AllowedActions": [
                {
                  "Conditions": {
                    "CardKind": [
                      "Prepaid",
                      "Debit",
                      "Credit"
                    ],
                    "CardStatus": {
                      "Ordered": "Any",
                      "Inactive": "Any",
                      "Active": "Any",
                      "Restricted": "Any",
                      "Blocked": "Any",
                      "Expired": "Any",
                      "Closed": "Any"
                    }
                  }
                }
              ]
            }
            """)]
        [InlineData("""
            {
              "AllowedActions": [
                {
                  "Name": "",
                  "Conditions": {
                    "CardKind": [
                      "Prepaid",
                      "Debit",
                      "Credit"
                    ],
                    "CardStatus": {
                      "Ordered": "Any",
                      "Inactive": "Any",
                      "Active": "Any",
                      "Restricted": "Any",
                      "Blocked": "Any",
                      "Expired": "Any",
                      "Closed": "Any"
                    }
                  }
                }
              ]
            }
            """)]
        [InlineData("""
            {
              "AllowedActions": [
                {
                  "Name1": "ACTION4",
                  "Conditions": {
                    "CardKind": [
                      "Prepaid",
                      "Debit",
                      "Credit"
                    ],
                    "CardStatus": {
                      "Ordered": "Any",
                      "Inactive": "Any",
                      "Active": "Any",
                      "Restricted": "Any",
                      "Blocked": "Any",
                      "Expired": "Any",
                      "Closed": "Any"
                    }
                  }
                }
              ]
            }
            """)]
        [InlineData("""
            {
              "AllowedActions": [
                {
                  "Name": "          ",
                  "Conditions": {
                    "CardKind": [
                      "Prepaid",
                      "Debit",
                      "Credit"
                    ],
                    "CardStatus": {
                      "Ordered": "Any",
                      "Inactive": "Any",
                      "Active": "Any",
                      "Restricted": "Any",
                      "Blocked": "Any",
                      "Expired": "Any",
                      "Closed": "Any"
                    }
                  }
                }
              ]
            }
            """)]
        public void Validate_EmptyOrMissingActionName_Failure(string json)
        {
            var result = GetValidateOptionsResult(json);
            Assert.Contains(result.Failures, f => f.Contains("Action Name is required"));
        }

        [Theory]
        [InlineData("""
            {
              "AllowedActions": [
                {
                  "Name": "ACTION1"
                }
              ]
            }
            """)]
        [InlineData("""
            {
              "AllowedActions": [
                {
                  "Name": "ACTION2",
                  "Conditions": {
                  }
                }
              ]
            }
            """)]
        [InlineData("""
            {
              "AllowedActions": [
                {
                  "Name": "ACTION3",
                  "ConditionsMissing": {
                    "CardKind": [
                      "Prepaid",
                      "Debit",
                      "Credit"
                    ],
                    "CardStatus": {
                      "Ordered": "Any",
                      "Inactive": "Any",
                      "Active": "Any",
                      "Restricted": "Any",
                      "Blocked": "Any",
                      "Expired": "Any",
                      "Closed": "Any"
                    }
                  }
                }
              ]
            }
            """)]
        public void Validate_EmptyOrMissingConditionsSection_Failure(string json)
        {
            var result = GetValidateOptionsResult(json);
            Assert.Contains(result.Failures, f => f.Contains($"Action {Conditions.SectionName} are required"));
        }

        private static ValidateOptionsResult GetValidateOptionsResult(string source, InputType inputType = InputType.String)
        {
            IConfigurationRoot root = GetConfigurationRoot(source, inputType);
            List<AllowedAction> allowedActions = root.GetSection(AllowedAction.SectionName).Get<List<AllowedAction>>() ?? [];

            AllowedActionsValidator allowedActionsValidator = new(root);

            return allowedActionsValidator.Validate(null, allowedActions);
        }

        private static IConfigurationRoot GetConfigurationRoot(string source, InputType inputType)
        {
            ConfigurationBuilder configurationBuilder = new();

            switch (inputType)
            {
                case InputType.String:
                    using (MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(source)))
                    {
                        return configurationBuilder.AddJsonStream(memoryStream).Build();
                    }
                case InputType.File:
                    return configurationBuilder.AddJsonFile(source).Build();
                default:
                    throw new ArgumentException("Unknown inputType");
            }
        }
    }
}