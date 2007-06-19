using System;

namespace SvnBridge.Utility
{
    public static class Clock
    {
        static bool _timeFrozen = false;
        static DateTime _frozenTime;

        public static void FreezeTime(DateTime time)
        {
            _timeFrozen = true;
            _frozenTime = time;
        }

        public static DateTime GetDate()
        {
            if (_timeFrozen == true)
                return _frozenTime;
            return DateTime.Now;
        }
    }
}