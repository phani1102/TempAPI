using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkRewards.DTO.Request;
using WorkRewards.DTO.Response;
using WorkRewards.Manager.Interface;

namespace WorkRewardsAPI.Controllers
{
    [Route("v1")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginManager _loginManager;
        ILogger logger;
        public LoginController(ILoginManager loginManager, ILoggerFactory deploggerFactory)
        {
            this.logger = deploggerFactory.CreateLogger("Controllers.LoginController");
            _loginManager = loginManager;
        }
        [HttpPost("Register")]
        public async Task<User> Register(UserRequest userRequest)
        {
            try
            {

                return await _loginManager.Register(userRequest);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}