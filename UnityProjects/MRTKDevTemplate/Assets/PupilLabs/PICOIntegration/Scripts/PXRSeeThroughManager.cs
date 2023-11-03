using Unity.XR.PXR;
using UnityEngine;

namespace PupilLabs.PICO
{
    public class PXRSeeThroughManager : MonoBehaviour
    {
        [SerializeField]
        private bool seeThroughEnabled = false;

        public bool SeeThroughEnabled { get { return seeThroughEnabled; } }

        private void Start()
        {
            PXR_Boundary.EnableSeeThroughManual(SeeThroughEnabled);
        }

        public void EnableSeeThrough(bool enabled)
        {
            PXR_Boundary.EnableSeeThroughManual(enabled);
            seeThroughEnabled = enabled;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                PXR_Boundary.EnableSeeThroughManual(SeeThroughEnabled);
            }
        }
    }
}
