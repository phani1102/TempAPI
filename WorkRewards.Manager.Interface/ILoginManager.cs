using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WorkRewards.DTO.Request;
using WorkRewards.DTO.Response;

namespace WorkRewards.Manager.Interface
{
    public interface ILoginManager
    {
        Task<User> Register(UserRequest userRequest);

    }
}
