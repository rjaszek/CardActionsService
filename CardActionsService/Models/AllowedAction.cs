namespace CardActionsService.Models
{
    public class AllowedAction
    {
        public const string SectionName = "AllowedActions";

        public required string Name { get; set; }
        public required Conditions Conditions { get; set; }
    }
}
