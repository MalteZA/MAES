using System;

namespace Dora.Utilities {
    public class TimeUtils {
        public static long CurrentTimeMillis() {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}