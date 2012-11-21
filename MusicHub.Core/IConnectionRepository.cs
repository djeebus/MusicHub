using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface IConnectionRepository
    {
        void ClientConnected(string userId, string connectionId);
        void ClientDisconnected(string connectionId);
    }
}
