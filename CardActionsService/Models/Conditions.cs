using CardActionsService.Enums;

namespace CardActionsService.Models
{
    public class Conditions
    {
        public const string SectionName = "Conditions";
        public const string CardTypeSectionName = "CardKind";
        public const string CardStatusSectionName = "CardStatus";


        [ConfigurationKeyName(CardTypeSectionName)]
        public required List<CardType> CardTypes { get; set; }

        [ConfigurationKeyName(CardStatusSectionName)]
        public required Dictionary<CardStatus, PinStatus> CardAndPinStatus { get; set; }
    }
}