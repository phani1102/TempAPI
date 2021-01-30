using System;
using System.Collections.Generic;
using System.Text;
using WorkRewards.DTO.Request;
using WorkRewards.DTO.Response;

namespace WorkRewards.Data.Interface
{
    public interface ILoginData
    {
       User Register(UserRequest userRequest);
    }
}
