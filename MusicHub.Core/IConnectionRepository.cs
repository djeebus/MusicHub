using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface IConnectionRepository
    {
        User ClientConnected(string userId, string connectionId);
        User ClientDisconnected(string connectionId);
    }
}
