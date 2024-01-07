using AL;
using DAL;
using DTO;
using FL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RabbitMQ_Messenger_Lib;
using RabbitMQ_Messenger_Lib.Types;
using System.Reflection;
using Tenant_service.AuthConfig;

namespace Tenant_service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BoardController : ControllerBase
    {
        private Sender sender;
        private readonly ITenantCollection tenantCollection;
        private readonly ManagementAPIConfig? managementAPIConfig;

        public BoardController(MessengerConfig messengerConfig, TenantContext _tenantContext, ITenantCollection? _tenantCollection = null, ManagementAPIConfig? _managementAPIConfig = null)
        {
            sender = new Sender(messengerConfig, new List<Queue> { new Queue(name: "LOG"), new Queue(name: "CREATE_BOARD") });
            tenantCollection = _tenantCollection ?? ITenantCollectionFactory.Get(_tenantContext);
            managementAPIConfig = _managementAPIConfig;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created ,Type = typeof(Board))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]        
        [Authorize]
        public IActionResult Create(BoardRestricted board)
        {
            TenantDTO _dto = board.ToDTO();
            _dto.Administrators = new List<string>() { AuthorizationHelper.GetRequestSub(Request) };
            _dto.Members = new List<string>() { AuthorizationHelper.GetRequestSub(Request) };

            TenantDTO? createdBoard = tenantCollection.Create(_dto);
            if (createdBoard != null)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("id", createdBoard.ID);
                dict.Add("board", createdBoard);

                Message message = new Message(dict, MessageType.EVENT, Assembly.GetExecutingAssembly().GetName().Name) ;
                sender.Send(message, "CREATE_BOARD");
                return Created($"/{createdBoard.ID}", new Board(createdBoard));
            }                
            else
                return BadRequest("The board could not be created");
        }

        [HttpGet]
        public async Task<IActionResult> AddMember(int boardID, string member)
        {
            if (managementAPIConfig != null)
                Console.WriteLine(await AuthorizationHelper.GetManagementToken(managementAPIConfig));
            return BadRequest();
        }
    }
}