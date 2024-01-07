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
    public class MemberController : ControllerBase
    {
        private Sender sender;        
        private readonly ManagementAPIConfig? managementAPIConfig;

        public MemberController(MessengerConfig messengerConfig, TenantContext _tenantContext, ManagementAPIConfig? _managementAPIConfig = null)
        {
            sender = new Sender(messengerConfig, new List<Queue> { new Queue(name: "LOG"), new Queue(name: "CREATE_BOARD") });            
            managementAPIConfig = _managementAPIConfig;
        }

        [HttpPatch]        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("Add")]
        public async Task<IActionResult> AddMember(int boardID, string memberEmail)
        {
            if (managementAPIConfig == null)
                throw new ArgumentNullException(nameof(managementAPIConfig));

            if (!await AuthorizationHelper.CheckAdminPermission(managementAPIConfig, AuthorizationHelper.GetRequestSub(Request), boardID))
                return Unauthorized("This user does not have administrative access to this board");

            string? userID = await AuthorizationHelper.GetSubByEmail(managementAPIConfig, memberEmail);
            if (userID == null)
                return BadRequest("A user with given email was not found");

            if (await AuthorizationHelper.AddUserToBoard(managementAPIConfig, userID, boardID))
                return Ok();
            else
                return BadRequest("User could not be added to board");
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("Remove")]
        public async Task<IActionResult> RemoveMember(int boardID, string memberEmail)
        {
            if (managementAPIConfig == null)
                throw new ArgumentNullException(nameof(managementAPIConfig));

            if (!await AuthorizationHelper.CheckAdminPermission(managementAPIConfig, AuthorizationHelper.GetRequestSub(Request), boardID))
                return Unauthorized("This user does not have administrative access to this board");

            string? userID = await AuthorizationHelper.GetSubByEmail(managementAPIConfig, memberEmail);
            if (userID == null)
                return BadRequest("A user with given email was not found");

            if (await AuthorizationHelper.RemoveUserFromBoard(managementAPIConfig, userID, boardID))
                return Ok();
            else
                return BadRequest("User could not be added to board");
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("Admin/Add")]
        public async Task<IActionResult> AddAdmin(int boardID, string memberEmail)
        {
            if (managementAPIConfig == null)
                throw new ArgumentNullException(nameof(managementAPIConfig));

            if (!await AuthorizationHelper.CheckAdminPermission(managementAPIConfig, AuthorizationHelper.GetRequestSub(Request), boardID))
                return Unauthorized("This user does not have administrative access to this board");

            string? userID = await AuthorizationHelper.GetSubByEmail(managementAPIConfig, memberEmail);
            if (userID == null)
                return BadRequest("A user with given email was not found");

            if (await AuthorizationHelper.AddAdminToBoard(managementAPIConfig, userID, boardID))
                return Ok();
            else
                return BadRequest("User could not be added to board");
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("Admin/Remove")]
        public async Task<IActionResult> RemoveAdmin(int boardID, string memberEmail)
        {
            if (managementAPIConfig == null)
                throw new ArgumentNullException(nameof(managementAPIConfig));

            if (!await AuthorizationHelper.CheckAdminPermission(managementAPIConfig, AuthorizationHelper.GetRequestSub(Request), boardID))
                return Unauthorized("This user does not have administrative access to this board");

            string? userID = await AuthorizationHelper.GetSubByEmail(managementAPIConfig, memberEmail);
            if (userID == null)
                return BadRequest("A user with given email was not found");

            if (await AuthorizationHelper.RemoveAdminFromBoard(managementAPIConfig, userID, boardID))
                return Ok();
            else
                return BadRequest("User could not be added to board");
        }
    }
}