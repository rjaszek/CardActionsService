using CardActionsService.Enums;
using CardActionsService.Models;
using Microsoft.Extensions.Options;

namespace CardActionsService.Services
{
    public class RulesService(IOptionsMonitor<List<AllowedAction>> optionsMonitor) : IRulesService
    {
        private readonly IOptionsMonitor<List<AllowedAction>> _optionsMonitor = optionsMonitor;

        public List<string> GetActions(CardDetails cardDetails)
        {
            List<AllowedAction> allowedActions = _optionsMonitor.CurrentValue;
            PinStatus pinStatus = cardDetails.IsPinSet ? PinStatus.PinSet : PinStatus.PinNotSet;

            return allowedActions
                        .Where(a => a.Conditions.CardTypes.Contains(cardDetails.CardType))
                        .Where(a => a.Conditions.CardAndPinStatuses.ContainsKey(cardDetails.CardStatus))
                        .Where(a => a.Conditions.CardAndPinStatuses[cardDetails.CardStatus] == pinStatus
                                 || a.Conditions.CardAndPinStatuses[cardDetails.CardStatus] == PinStatus.Any)
                        .Select(a => a.Name)
                        .ToList();
        }
    }
}
