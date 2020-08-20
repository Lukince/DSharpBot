using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Exceptions
{
    class NumberException : Exception
    {
        public NumberException() { }
        public NumberException(int inum) : base() { this.inum = inum; }
        public NumberException(double dnum) : base() { this.dnum = dnum; }
        public NumberException(int inum, double dnum) : base() { this.inum = inum; this.dnum = dnum; }
        public NumberException(string message) : base(message) { }
        public NumberException(string message, Exception inner) : base(message, inner) { }
        public NumberException(string message, int inum) : base(message) { this.inum = inum; }
        public NumberException(string message, double dnum) : base(message) { this.dnum = dnum; }
        public NumberException(string message, int inum, double dnum) : base (message) 
        { this.inum = inum; this.dnum = dnum; }

        public int inum;
        public double dnum;
    }
}
