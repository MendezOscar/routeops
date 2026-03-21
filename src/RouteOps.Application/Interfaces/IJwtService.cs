using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RouteOps.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string id, string email, string name, string role);
    }
}