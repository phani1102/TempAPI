using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WorkRewards.Data.Interface;
using WorkRewards.DTO.Request;
using WorkRewards.DTO.Response;
using WorkRewards.Manager.Interface;

namespace WorkRewards.Manager
{
    public class LoginManager : ILoginManager
    {
        private readonly ILoginData _loginData;
        ILogger<LoginManager> logger;
        public LoginManager(ILoginData loginData, ILogger<LoginManager> logger)
        {
            this.logger = logger;
            _loginData = loginData;
        }
        public Task<User> Register(UserRequest userRequest)
        {
            return Task.Run(() => _loginData.Register(userRequest));
        }
    }
}
