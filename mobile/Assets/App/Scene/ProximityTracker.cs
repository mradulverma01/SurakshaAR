using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Scene
{
    internal sealed class ProximityTracker
    {
        private const float EnterRadius = 0.8f;
        private const float ExitRadius = 0.85f;

        private readonly Dictionary<string, bool> inside = new Dictionary<string, bool>(System.StringComparer.Ordinal);

        public bool ShouldEmitOnEnter(string interactionId, Vector3 cameraPosition, Vector3 targetPosition)
        {
            var distance = Vector3.Distance(cameraPosition, targetPosition);
            var wasInside = inside.TryGetValue(interactionId, out var prior) && prior;
            var isInside = distance <= EnterRadius;
            if (isInside == wasInside)
            {
                if (!isInside && distance > ExitRadius)
                {
                    inside[interactionId] = false;
                }
                return false;
            }

            inside[interactionId] = isInside;
            return isInside;
        }

        public void Clear()
        {
            inside.Clear();
        }
    }
}
