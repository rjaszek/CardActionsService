using CardActionsService.Enums;
using CardActionsService.Models;
using Microsoft.Extensions.Options;

namespace CardActionsService.Validators
{
    public class AllowedActionsValidator(IConfiguration config) : IValidateOptions<List<AllowedAction>>
    {
        private static readonly string AllowedCardTypes = string.Join(", ", Enum.GetNames<CardType>());
        private static readonly string AllowedCardStatuses = string.Join(", ", Enum.GetNames<CardStatus>());
        private static readonly string AllowedPinStatuses = string.Join(", ", Enum.GetNames<PinStatus>().Where(x => x != nameof(PinStatus.Undefined)));

        public ValidateOptionsResult Validate(string? name, List<AllowedAction> allowedActions)
        {
            List<string> errors = [];

            if (allowedActions.Count == 0)
            {
                errors.Add($"There is no allowed action or configuration section is missing, section: {AllowedAction.SectionName}");
            }
            else
            {
                for (int actionIndex = 0; actionIndex < allowedActions.Count; actionIndex++)
                {
                    var item = allowedActions[actionIndex];

                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        errors.Add($"Action Name is required (action item number: {actionIndex})");
                    }

                    if (item.Conditions == null)
                    {
                        errors.Add($"Action Conditions are required (action item number: {actionIndex})");
                    }
                    else
                    {
                        CheckCardTypeSection(item.Conditions, actionIndex, errors);
                        CheckCardStatusSection(item.Conditions, actionIndex, errors);
                    }
                }

                CheckDuplicates(allowedActions, errors);
            }

            return errors.Count > 0
                    ? ValidateOptionsResult.Fail(errors)
                    : ValidateOptionsResult.Success;
        }

        private void CheckCardTypeSection(Conditions conditions, int i, List<string> errors)
        {
            IConfigurationSection cardTypeSection = config.GetSection(BuildSectionPath(Conditions.CardTypeSectionName, i));
            IReadOnlyCollection<IConfigurationSection> cardTypeSectionChildren = cardTypeSection.GetChildren().ToList();

            if (cardTypeSectionChildren.Count == 0)
            {
                errors.Add($"Action {Conditions.SectionName} -> {Conditions.CardTypeSectionName} cannot be empty (action item number: {i})");
            }
            else if (conditions.CardTypes != null)
            {
                foreach (var cardType in conditions.CardTypes)
                {
                    if (!Enum.IsDefined<CardType>(cardType))
                    {
                        errors.Add($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardTypeSectionName}, allowed list: {AllowedCardTypes} (action item number: {i}, {Conditions.CardTypeSectionName}: {cardType})");
                    }
                }
            }

            if (cardTypeSectionChildren.Count != (conditions.CardTypes?.Count ?? 0))
            {
                foreach (var child in cardTypeSectionChildren)
                {
                    if (!IsValidEnumName<CardType>(child.Value))
                    {
                        errors.Add($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardTypeSectionName}, allowed list: {AllowedCardTypes} (action item number: {i}, {Conditions.CardTypeSectionName}: {child.Value})");
                    }
                }
            }
        }

        private void CheckCardStatusSection(Conditions conditions, int i, List<string> errors)
        {
            IConfigurationSection cardStatusSection = config.GetSection(BuildSectionPath(Conditions.CardStatusSectionName, i));
            IReadOnlyCollection<IConfigurationSection> cardStatusSectionChildren = cardStatusSection.GetChildren().ToList();

            if (cardStatusSectionChildren.Count == 0)
            {
                errors.Add($"Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName} cannot be empty (action item number: {i})");
            }
            else if (conditions.CardAndPinStatus != null)
            {
                foreach (var cardAndPinStatus in conditions.CardAndPinStatus)
                {
                    if (!Enum.IsDefined<CardStatus>(cardAndPinStatus.Key))
                    {
                        errors.Add($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName}, allowed list: {AllowedCardStatuses} (action item number: {i}, {Conditions.CardStatusSectionName}: {cardAndPinStatus.Key})");
                    }

                    if (cardAndPinStatus.Value == PinStatus.Undefined)
                    {
                        errors.Add($"Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName} -> PinStatus cannot be undefined, allowed list: {AllowedPinStatuses} (action item number: {i}, {Conditions.CardStatusSectionName}: {cardAndPinStatus.Key})");
                    }
                    else if (!Enum.IsDefined<PinStatus>(cardAndPinStatus.Value))
                    {
                        errors.Add($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName} -> PinStatus, allowed list: {AllowedPinStatuses} (action item number: {i}, {Conditions.CardStatusSectionName}: {cardAndPinStatus.Key})");
                    }
                }
            }

            if (cardStatusSectionChildren.Count != (conditions.CardAndPinStatus?.Count ?? 0))
            {
                foreach (var child in cardStatusSectionChildren)
                {
                    if (!IsValidEnumName<CardStatus>(child.Key))
                    {
                        errors.Add($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName}, allowed list: {AllowedCardStatuses} (action item number: {i}, {Conditions.CardStatusSectionName}: {child.Key})");
                    }

                    if (!IsValidEnumName<PinStatus>(child.Value))
                    {
                        errors.Add($"Unknown value of Action {Conditions.SectionName} -> {Conditions.CardStatusSectionName} -> PinStatus, allowed list: {AllowedPinStatuses} (action item number: {i}, {Conditions.CardStatusSectionName}: {child.Key}: {child.Value})");
                    }
                }
            }
        }

        private static void CheckDuplicates(List<AllowedAction> options, List<string> errors)
        {
            var duplicates = options
                                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                                .Where(g => g.Count() > 1)
                                .Select(x => x.Key)
                                .ToList();

            if (duplicates.Any())
            {
                errors.Add($"There are duplicates in allowed actions: {string.Join(";", duplicates)}");
            }
        }

        private static bool IsValidEnumName<TEnum>(string? name) where TEnum : struct, Enum
        {
            TEnum outVal;
            return Enum.TryParse<TEnum>(name, true, out outVal) && Enum.IsDefined<TEnum>(outVal);
        }

        private static string BuildSectionPath(string sectionName, int itemNo) => $"{AllowedAction.SectionName}:{itemNo}:{Conditions.SectionName}:{sectionName}";
    }
}
