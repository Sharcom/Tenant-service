using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RabbitMQ_Messenger_Lib;
using RabbitMQ_Messenger_Lib.Types;
using System.Reflection;

namespace Post_service.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    public class ErrorController : Controller
    {
        private Sender sender;

        public ErrorController(MessengerConfig messengerConfig) 
        {
            sender = new Sender(messengerConfig, new List<Queue> { new Queue(name: "LOG") });
        }


        [Route("")]
        public IActionResult UnhandledError(ExceptionContext exception)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("message", exception.Exception.Message);

            int priority;
            switch (exception.Exception)
            {
                case ArgumentNullException: priority = 1; break;
                case NullReferenceException: priority = 1; break;
                case ArgumentOutOfRangeException: priority = 3; break;
                case ArgumentException: priority = 5; break;
                case StackOverflowException: priority = 4; break;
                case OutOfMemoryException: priority = 4; break;
                default: priority = 2; break;
            }            
            dict.Add("priority", priority);

            Message _message = new Message(dict, MessageType.LOG, $"{Assembly.GetExecutingAssembly().GetName().Name}");
            sender.Send(_message, "LOG");
            return Ok();
        }
    }
}
