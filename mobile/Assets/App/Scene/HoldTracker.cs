using UnityEngine;

namespace SurakshaAR.Scene
{
    internal sealed class HoldTracker
    {
        private bool active;
        private string? interactionId;
        private string? targetId;
        private float startTime;

        public bool IsActive => active;

        public void Begin(string id, string target, float time)
        {
            active = true;
            interactionId = id;
            targetId = target;
            startTime = time;
        }

        public bool TryComplete(float time, out string id, out string target, out decimal duration)
        {
            if (!active || interactionId == null || targetId == null)
            {
                id = string.Empty;
                target = string.Empty;
                duration = 0;
                return false;
            }

            id = interactionId;
            target = targetId;
            duration = (decimal)(time - startTime);
            Reset();
            return true;
        }

        public bool TryCancel(out string id, out string target)
        {
            if (!active || interactionId == null || targetId == null)
            {
                id = string.Empty;
                target = string.Empty;
                return false;
            }

            id = interactionId;
            target = targetId;
            Reset();
            return true;
        }

        public void Reset()
        {
            active = false;
            interactionId = null;
            targetId = null;
            startTime = 0f;
        }
    }
}
