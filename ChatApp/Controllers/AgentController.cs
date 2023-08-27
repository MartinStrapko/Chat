﻿using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        [HttpPost("register")]
        public IActionResult RegisterAgent([FromBody] Agent agent)
        {
            if (agent == null || string.IsNullOrEmpty(agent.Name))
            {
                return BadRequest("Invalid agent details.");
            }

            _agentService.AddAgent(agent);

            return Ok();
        }

        //[HttpGet("chats/{agentId}")]
        //public IActionResult GetChatsAssigned(Guid agentId)
        //{
        //    var chats = _agentService.GetAssignedChats(agentId);

        //    if (chats == null)
        //    {
        //        return NotFound("Agent not found.");
        //    }

        //    return Ok(chats);
        //}

        //[HttpPost("update-status/{agentId}")]
        //public IActionResult UpdateStatus(Guid agentId, [FromBody] AgentStatus status)
        //{
        //    if (!_agentService.UpdateStatus(agentId, status))
        //    {
        //        return NotFound("Agent not found or status not updated.");
        //    }

        //    return Ok("Status updated successfully.");
        //}
    }
}