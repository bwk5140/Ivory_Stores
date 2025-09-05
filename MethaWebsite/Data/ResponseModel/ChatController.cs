using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared;

namespace MethaWebsite.Data.ResponseModel
{
    [ApiController]
    [Route("[controller]")]

    public class ChatController : ControllerBase
    {
        private readonly ResponseEngine _responseEngine;
        private readonly EntityRecognizer _entityRecognizer;
        private readonly ILogger<ChatController> _logger;
        private readonly IConversationIdProvider _idProvider;

        public ChatController(ResponseEngine responseEngine, EntityRecognizer entityRecognizer, ILogger<ChatController> logger, IConversationIdProvider idProvider)
        {
            _responseEngine = responseEngine;
            _entityRecognizer = entityRecognizer;
            _logger = logger;
            _idProvider = idProvider;
        }

        [HttpPost("respond")]
        public IActionResult GenerateResponse([FromBody] ChatMessage message)
        {
            var conversationId = _idProvider.GetConversationId(HttpContext);
            var ResponseRequest = new ResponseRequest
            {
                Utterance = message.Text,
                IntentId = message.Intent,
                IntentConfidence = 0.62,
                Locale = "en-KE",
                Entities = _entityRecognizer.ExtractEntities(message.Text, "en-KE", conversationId)
            };
            var action = _responseEngine.Generate(ResponseRequest, conversationId);
            var responseText = _responseEngine.GenerateResponse(action, ResponseRequest, conversationId).Text;

            return Ok(responseText);

        }

    }
}
