using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.GooglePlay
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
            : base("Invalid credentials")
        {
        }
    }
}
