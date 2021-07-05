using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Light;

namespace CloudCam.Light
{
    public class LedAnimator
    {
        private readonly LedController _ledController;

        public LedAnimator(LedController ledController)
        {
            _ledController = ledController;
        }

        public async Task StartAsync()
        {
            await _ledController.StartAsync();
        }
    }
}
