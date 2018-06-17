using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModels
{
    public static class QueueNames
    {
        public static string TestRequest(string build)
        {
            return $"{build}_request";
        }

        public static string TestResponse(string build)
        {
            return $"{build}_response";
        }

        public static string Status(string build)
        {
            return $"{build}_status";
        }

        public static string Build()
        {
            return $"build";
        }
    }
}
