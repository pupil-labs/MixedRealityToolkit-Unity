using UnityEngine;

namespace PupilLabs
{
    public class CalibrationTarget : MonoBehaviour
    {
        public Transform center;
        public EyeTrackingCalibration calibration;

        public void OnTargetSelected()
        {
            calibration.AddObjPoint(center.position, gameObject.GetInstanceID());
        }
    }
}

