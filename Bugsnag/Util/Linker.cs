using System;

namespace Bugsnag.Util
{
    public static class Linker
    {
        public static void CheckStripped ()
        {
            // Check that constructor exists for converters
            try {
                Activator.CreateInstance (typeof(Bugsnag.Json.ErrorSeverityConverter));
            } catch {
                throw new NotSupportedException ("You need to add Bugsnag;Bugsnag.Android;Bugsnag.iOS to Xamarin Linker skip list.");
            }
        }
    }
}
