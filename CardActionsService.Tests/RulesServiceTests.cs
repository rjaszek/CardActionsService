using CardActionsService.Enums;
using CardActionsService.Models;
using CardActionsService.Services;
using CardActionsService.Tests.Enums;
using CardActionsService.Tests.Fakes;
using Microsoft.Extensions.Configuration;

namespace CardActionsService.Tests
{
    public class RulesServiceTests
    {
        [Fact]
        public void GetActions_PrepaidClosed_ReturnsActions3_4_9()
        {
            List<AllowedAction> allowedActions = [
                Helpers.CreateAllowedAction("ACTION1", [CardType.Prepaid, CardType.Debit, CardType.Credit],
                    new()
                    {
                        [CardStatus.Active] = PinStatus.Any
                    }
                ),
                Helpers.CreateAllowedAction("ACTION3", [CardType.Prepaid, CardType.Debit, CardType.Credit],
                    new()
                    {
                        [CardStatus.Ordered] = PinStatus.Any,
                        [CardStatus.Inactive] = PinStatus.Any,
                        [CardStatus.Active] = PinStatus.Any,
                        [CardStatus.Restricted] = PinStatus.Any,
                        [CardStatus.Blocked] = PinStatus.Any,
                        [CardStatus.Expired] = PinStatus.Any,
                        [CardStatus.Closed] = PinStatus.Any
                    }
                ),
                Helpers.CreateAllowedAction("ACTION4", [CardType.Prepaid, CardType.Debit, CardType.Credit],
                    new()
                    {
                        [CardStatus.Ordered] = PinStatus.Any,
                        [CardStatus.Inactive] = PinStatus.Any,
                        [CardStatus.Active] = PinStatus.Any,
                        [CardStatus.Restricted] = PinStatus.Any,
                        [CardStatus.Blocked] = PinStatus.Any,
                        [CardStatus.Expired] = PinStatus.Any,
                        [CardStatus.Closed] = PinStatus.Any
                    }
                ),
                Helpers.CreateAllowedAction("ACTION5", [CardType.Credit],
                    new()
                    {
                        [CardStatus.Ordered] = PinStatus.Any,
                        [CardStatus.Inactive] = PinStatus.Any,
                        [CardStatus.Active] = PinStatus.Any,
                        [CardStatus.Restricted] = PinStatus.Any,
                        [CardStatus.Blocked] = PinStatus.Any,
                        [CardStatus.Expired] = PinStatus.Any,
                        [CardStatus.Closed] = PinStatus.Any
                    }
                ),
                Helpers.CreateAllowedAction("ACTION7", [CardType.Prepaid, CardType.Debit, CardType.Credit],
                    new()
                    {
                        [CardStatus.Ordered] = PinStatus.PinNotSet,
                        [CardStatus.Inactive] = PinStatus.PinNotSet,
                        [CardStatus.Active] = PinStatus.PinNotSet,
                        [CardStatus.Blocked] = PinStatus.PinSet,
                        [CardStatus.Closed] = PinStatus.PinNotSet
                    }
                ),
                Helpers.CreateAllowedAction("ACTION9", [CardType.Prepaid, CardType.Debit, CardType.Credit],
                    new()
                    {
                        [CardStatus.Ordered] = PinStatus.Any,
                        [CardStatus.Inactive] = PinStatus.Any,
                        [CardStatus.Active] = PinStatus.Any,
                        [CardStatus.Restricted] = PinStatus.Any,
                        [CardStatus.Blocked] = PinStatus.Any,
                        [CardStatus.Expired] = PinStatus.Any,
                        [CardStatus.Closed] = PinStatus.Any
                    }
                )
            ];

            FakeOptionsMonitor fakeOptionsMonitor = new(allowedActions);
            RulesService rulesService = new(fakeOptionsMonitor);
            CardDetails cardDetails = new("Card17", CardType.Prepaid, CardStatus.Closed, true);

            var result = rulesService.GetActions(cardDetails);
            Assert.Equal(["ACTION3", "ACTION4", "ACTION9"], result);
        }

        [Theory]
        [InlineData(PinStatus.PinSet, true, true)]
        [InlineData(PinStatus.PinSet, false, false)]
        [InlineData(PinStatus.PinNotSet, true, false)]
        [InlineData(PinStatus.PinNotSet, false, true)]
        [InlineData(PinStatus.Any, true, true)]
        [InlineData(PinStatus.Any, false, true)]
        public void GetActions_PinRequirement_MatchesCardPin(PinStatus requiredPin, bool isPinSet, bool expectedMatch)
        {
            List<AllowedAction> allowedActions = [
                Helpers.CreateAllowedAction("ACTIONX", [CardType.Debit],
                    new()
                    {
                        [CardStatus.Active] = requiredPin
                    }
                )
            ];

            FakeOptionsMonitor fakeOptionsMonitor = new(allowedActions);
            RulesService rulesService = new(fakeOptionsMonitor);
            CardDetails cardDetails = new("Card114", CardType.Debit, CardStatus.Active, isPinSet);

            var result = rulesService.GetActions(cardDetails);
            List<string> expectedNames = expectedMatch ? ["ACTIONX"] : [];
            Assert.Equal(expectedNames, result);
        }

        [Fact]
        public void GetActions_EmptyAllowedActions_ReturnsEmpty()
        {
            List<AllowedAction> allowedActions = [];

            FakeOptionsMonitor fakeOptionsMonitor = new(allowedActions);
            RulesService rulesService = new(fakeOptionsMonitor);
            CardDetails cardDetails = new("Card17", CardType.Prepaid, CardStatus.Closed, true);

            var result = rulesService.GetActions(cardDetails);
            Assert.Empty(result);
        }

        [Fact]
        public void GetActions_NoMatchingRules_ReturnsEmpty()
        {
            List<AllowedAction> allowedActions = [
                Helpers.CreateAllowedAction("ACTIONX", [CardType.Credit],
                    new()
                    {
                        [CardStatus.Blocked] = PinStatus.PinSet
                    }
                )
            ];

            FakeOptionsMonitor fakeOptionsMonitor = new(allowedActions);
            RulesService rulesService = new(fakeOptionsMonitor);
            CardDetails cardDetails = new("Card119", CardType.Credit, CardStatus.Blocked, false);

            var result = rulesService.GetActions(cardDetails);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(true, new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" })]
        [InlineData(false, new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION8", "ACTION9" })]
        public void GetActions_CreditBlockedFromFileConfig_MatchResult(bool isPinSet, string[] expectedResults)
        {
            IConfigurationRoot root = Helpers.GetConfigurationRoot(Helpers.ConfigFile, InputType.File);
            List<AllowedAction>? allowedActions = root.GetSection(AllowedAction.SectionName).Get<List<AllowedAction>>();
            Assert.NotNull(allowedActions);

            FakeOptionsMonitor fakeOptionsMonitor = new(allowedActions);
            RulesService rulesService = new(fakeOptionsMonitor);

            CardDetails cardDetails = new("Card119", CardType.Credit, CardStatus.Blocked, isPinSet);
            var result = rulesService.GetActions(cardDetails);
            Assert.Equal(expectedResults, result);
        }
    }
}
