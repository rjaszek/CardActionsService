using CardActionsService.Models;

namespace CardActionsService.Services
{
    public interface ICardService
    {
        Task<CardDetails?> GetCardDetails(string userId, string cardNumber);
    }
}
