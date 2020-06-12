using System;
using System.Runtime.Serialization;

namespace DiscordBot.Exceptions
{
    [Serializable]
    internal class EncryptionArgumentException : Exception
    {
        public EncryptionArgumentException() { }
        public EncryptionArgumentException(string message) : base(message) { }
        public EncryptionArgumentException(string message, Exception innerException) : base(message, innerException) { }
        protected EncryptionArgumentException (SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
