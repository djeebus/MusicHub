using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface IAuthenticationService
    {
        string GetDisplayName(string username);
    }
}
