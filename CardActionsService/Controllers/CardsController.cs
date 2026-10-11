using CardActionsService.Models;
using CardActionsService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CardActionsService.Controllers
{
    [Route("api")]
    [ApiController]
    public class CardsController(ICardService cardService, IRulesService rulesService) : ControllerBase
    {
        private readonly ICardService _cardService = cardService;
        private readonly IRulesService _rulesService = rulesService;

        /// <summary>
        /// Gets the actions allowed to be done on user's payment card.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cardNumber">The card number.</param>
        /// <returns>List of actions or error code</returns>
        /// <response code="200">List of actions found - can be empty</response> 
        /// <response code="404">Card for user not found</response>
        [HttpGet("users/{userId}/cards/{cardNumber}/actions")]
        [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActions([FromRoute] string userId, [FromRoute] string cardNumber)
        {
            CardDetails? cardDetails = await _cardService.GetCardDetails(userId, cardNumber);

            if (cardDetails is null) return NotFound();

            return Ok(_rulesService.GetActions(cardDetails));
        }
    }
}
