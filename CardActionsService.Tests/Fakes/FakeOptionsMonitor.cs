using CardActionsService.Models;
using Microsoft.Extensions.Options;

namespace CardActionsService.Tests.Fakes
{
    public class FakeOptionsMonitor : IOptionsMonitor<List<AllowedAction>>
    {
        public List<AllowedAction> CurrentValue {  get; set; }

        public FakeOptionsMonitor(List<AllowedAction> allowedActions) => CurrentValue = allowedActions;

        public List<AllowedAction> Get(string? name)
        {
            throw new NotImplementedException();
        }

        public IDisposable? OnChange(Action<List<AllowedAction>, string?> listener)
        {
            throw new NotImplementedException();
        }
    }
}
