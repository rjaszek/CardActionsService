using CardActionsService.Models;
using CardActionsService.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace CardActionsService.Tests
{
    public class AllowedActionsValidatorTests
    {
        private const string ConfigFilePath = "data.json";

        private enum InputType
        {
            String,
            File
        }


        [Fact]
        public void Validate_FileConfig_Success()
        {
            var result = GetValidateOptionsResult(ConfigFilePath, InputType.File);
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

        [Theory]
        [InlineData("""            
            {
              "AllowedActions": [
                {
                  "Name": "ACTION1",
                  "Conditions": {
                    "CardKind": [],
                    "CardStatus": {
                      "Active": "Any"
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
                  "Name": "ACTION1",
                  "Conditions": {
                    "CardStatus": {
                      "Active": "Any"
                    }
                  }
                }
              ]
            }
            """)]
        public void Validate_EmptyOrMissingCardTypeSection_Failure(string json)
        {
            var result = GetValidateOptionsResult(json);
            Assert.Contains(result.Failures, f => f.Contains($"Action {Conditions.SectionName} -> {Conditions.CardTypeSectionName} cannot be empty"));
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
                  "Name": "ACTION1",
                  "Conditions": {
                    "CardKind": [
                      "Prepaid",
                      "Debit",
                      "Credit"
                    ]
                  }
                }
              ]
            }
            """)]        
        public void Validate_EmptyOrMissingCardStatusSection_Failure(string json)
        {
            var result = GetValidateOptionsResult(json);
            Assert.Contains(result.Failures, f => f.Contains($"Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName} cannot be empty"));
        }

        [Theory]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "Prepaid1")]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "Debit12")]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "")]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "tttt")]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "   ")]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "9")]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "1")]
        public void Validate_UnknownCardType_Failure(string key, string? value)
        {
            ValidateOptionsResult result = ValidateWithOverride(key, value);
            Assert.Single(result.Failures);
            Assert.Contains(result.Failures, f => f.Contains($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardTypeSectionName}"));
        }

        [Theory]
        [InlineData("AllowedActions:0:Conditions:CardStatus:NotOrdered", "Any")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:  ", "Any")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:9", "Any")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:1", "Any")]
        public void Validate_UnknownCardStatus_Failure(string key, string? value)
        {
            ValidateOptionsResult result = ValidateWithOverride(key, value);
            Assert.Single(result.Failures);
            Assert.Contains(result.Failures, f => f.Contains($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName}, allowed list"));
        }

        [Theory]
        [InlineData("AllowedActions:0:Conditions:CardStatus:Ordered", "")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:Ordered", "pinnotSett")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:Ordered", "7")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:Ordered", "1")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:Ordered", "0")]
        public void Validate_UnknownPinStatus_Failure(string key, string? value)
        {
            ValidateOptionsResult result = ValidateWithOverride(key, value);
            Assert.Single(result.Failures);
            Assert.Contains(result.Failures, f => f.Contains($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName} -> PinStatus, allowed list"));
        }

        [Theory]
        [InlineData("AllowedActions:0:Conditions:CardStatus:Ordered", "Undefined")]
        public void Validate_UndefinedPinStatus_Failure(string key, string? value)
        {
            ValidateOptionsResult result = ValidateWithOverride(key, value);
            Assert.Single(result.Failures);
            Assert.Contains(result.Failures, f => f.Contains($"Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName} -> PinStatus cannot be undefined"));
        }

        [Theory]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "prepaid")]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "debit")]
        [InlineData("AllowedActions:0:Conditions:CardKind:0", "credit")]
        public void Validate_LowercaseCardType_Success(string key, string? value)
        {
            ValidateOptionsResult result = ValidateWithOverride(key, value);
            Assert.True(result.Succeeded);
        }

        [Theory]
        [InlineData("AllowedActions:0:Conditions:CardStatus:ordered", "Any")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:inactive", "Any")]
        [InlineData("AllowedActions:1:Conditions:CardStatus:active", "Any")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:restricted", "Any")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:blocked", "Any")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:expired", "Any")]
        [InlineData("AllowedActions:0:Conditions:CardStatus:closed", "Any")]
        public void Validate_LowercaseCardStatus_Success(string key, string? value)
        {
            ValidateOptionsResult result = ValidateWithOverride(key, value);
            Assert.True(result.Succeeded);
        }

        [Theory]
        [InlineData("AllowedActions:5:Conditions:CardStatus:Ordered", "any")]
        [InlineData("AllowedActions:5:Conditions:CardStatus:Inactive", "pinset")]
        [InlineData("AllowedActions:5:Conditions:CardStatus:Active", "pinnotset")]
        [InlineData("AllowedActions:5:Conditions:CardStatus:Restricted", "pinset")]
        [InlineData("AllowedActions:5:Conditions:CardStatus:Blocked", "pinnotset")]
        [InlineData("AllowedActions:5:Conditions:CardStatus:Expired", "pinset")]
        [InlineData("AllowedActions:5:Conditions:CardStatus:Closed", "pinnotset")]
        public void Validate_LowercasePinStatus_Success(string key, string? value)
        {
            ValidateOptionsResult result = ValidateWithOverride(key, value);
            Assert.True(result.Succeeded);
        }

        [Theory]
        [InlineData("AllowedActions:1:Name", "ACTION1")]
        [InlineData("AllowedActions:1:Name", "action1")]
        public void Validate_Duplicates_Failure(string key, string? value)
        {
            ValidateOptionsResult result = ValidateWithOverride(key, value);
            Assert.Contains(result.Failures, f => f.Contains("There are duplicates"));
        }

        [Fact]
        public void Validate_MultipleErrors_ReportsAll()
        {
            Dictionary<string, string?> overrides = new()
            {
                ["AllowedActions:5:Conditions:CardKind:0"] = "NotPrepaid",
                ["AllowedActions:3:Conditions:CardStatus:Inactive"] = "STOP"               
            };

            ValidateOptionsResult result = GetValidateOptionsResult(ConfigFilePath, InputType.File, overrides);
            Assert.Contains(result.Failures, f => f.Contains($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardTypeSectionName}") && f.Contains("action item number: 5"));
            Assert.Contains(result.Failures, f => f.Contains($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName} -> PinStatus") && f.Contains("action item number: 3"));
        }

        private static ValidateOptionsResult ValidateWithOverride(string key, string? value)
        {
            Dictionary<string, string?> overrides = [];
            overrides[key] = value;
            return GetValidateOptionsResult(ConfigFilePath, InputType.File, overrides);
        }

        private static ValidateOptionsResult GetValidateOptionsResult(string source, InputType inputType = InputType.String, Dictionary<string, string?>? shadowCollection = null)
        {
            IConfigurationRoot root = GetConfigurationRoot(source, inputType, shadowCollection);
            List<AllowedAction> allowedActions = root.GetSection(AllowedAction.SectionName).Get<List<AllowedAction>>() ?? [];

            AllowedActionsValidator allowedActionsValidator = new(root);

            return allowedActionsValidator.Validate(null, allowedActions);
        }

        private static IConfigurationRoot GetConfigurationRoot(string source, InputType inputType, Dictionary<string,string?>? shadowCollection = null)
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
    }
}