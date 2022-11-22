﻿using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        List<User> ReadAllUsers();
        User CreateUser(string username, string password, string email, string usertype);
        User GetUser(int id);
        User DeleteUser(int id);
        User UpdateUser(int id, User user);
    }
}