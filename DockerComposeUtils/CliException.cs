using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace DockerComposeUtils
{
    public class CliException : Exception
    {
        public string StdOut { get; set; }
        public string StdErr { get; set; }

        public CliException()
        {

        }

        public CliException(string message, string stdOut, string stdErr) : base(message)
        {
            StdOut = stdOut;
            StdErr = stdErr;
        }

        public CliException(string message) : base(message)
        {
        }

        public CliException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CliException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            StdOut = info.GetString(nameof(StdOut));
            StdErr = info.GetString(nameof(StdErr));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(StdOut), StdOut);
            info.AddValue(nameof(StdErr), StdErr);
            base.GetObjectData(info, context);
        }
    }
}
