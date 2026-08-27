using UnityEngine;

namespace SurakshaAR.Scene
{
    [DisallowMultipleComponent]
    public sealed class TrainingTarget : MonoBehaviour
    {
        [SerializeField]
        private string interactionId = string.Empty;

        [SerializeField]
        private string actionKind = "select";

        [SerializeField]
        private string targetId = string.Empty;

        public string InteractionId => interactionId;

        public string ActionKind => actionKind;

        public string TargetId => targetId;
    }
}
