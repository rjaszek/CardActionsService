using CardActionsService.Models;

namespace CardActionsService.Services
{
    public interface IRulesService
    {
        List<string> GetActions(CardDetails cardDetails);
    }
}