using System.Threading.Tasks;
using CloudCam.Light;

namespace CloudCam
{
    public class NullLedAnimator : ILedAnimator
    {
        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public Task Animate()
        {
            return Task.CompletedTask;
        }

        public void StartFlash()
        {
        }

        public void EndFlash()
        {
        }
    }
}