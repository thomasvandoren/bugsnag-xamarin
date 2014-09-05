using System;

namespace Bugsnag.Util
{
    public static class Log
    {
        const string Tag = "[Bugsnag] ";

        public static void WriteLine (string msg)
        {
            Console.WriteLine (String.Concat (Tag, msg));
        }

        public static void WriteLine (string format, object arg0)
        {
            Console.WriteLine (String.Concat (Tag, format), arg0);
        }

        public static void WriteLine (string format, object arg0, object arg1)
        {
            Console.WriteLine (String.Concat (Tag, format), arg0, arg1);
        }

        public static void WriteLine (string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine (String.Concat (Tag, format), arg0, arg1, arg2);
        }

        public static void WriteLine (string format, object arg0, object arg1, object arg2, object arg3)
        {
            Console.WriteLine (String.Concat (Tag, format), arg0, arg1, arg2, arg3);
        }

        public static void WriteLine (string format, params object[] arg)
        {
            Console.WriteLine (String.Concat (Tag, format), arg);
        }
    }
}

