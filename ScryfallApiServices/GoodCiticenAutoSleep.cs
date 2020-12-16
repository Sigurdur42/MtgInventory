using System;
using System.Threading;

namespace ScryfallApiServices
{
    public class GoodCiticenAutoSleep
    {
        private DateTime _lastAccess = DateTime.Now;

        public void AutoSleep()
        {
            var now = DateTime.Now;
            var waitTime = 100 - (now - _lastAccess).Milliseconds;
            if (waitTime < 0)
            {
                // Enough time between calls
                _lastAccess = now;
                return;
            }

            Thread.Sleep(waitTime + 1);
            _lastAccess = DateTime.Now;
        }
    }
}