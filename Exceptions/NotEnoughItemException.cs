using System;
using System.Runtime.Serialization;

namespace DiscordBot.Exceptions
{
    [Serializable]
    internal class NotEnoughItemException : Exception
    {
        public NotEnoughItemException() { }
        public NotEnoughItemException(string message) : base(message) { }
        public NotEnoughItemException(string message, Exception innerException) : base(message, innerException) { }
        protected NotEnoughItemException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
