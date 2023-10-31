using MixedReality.Toolkit.UX;
using UnityEngine;

namespace PupilLabs.MRTK
{
    public class HandMenuGaze : MonoBehaviour
    {
        [SerializeField]
        private PressableButton gazeDebugButton;

        private void Start()
        {
            GazeDataVisualizer rayVisualizer = ServiceLocator.Instance.GetComponentInChildren<GazeDataVisualizer>();
            gazeDebugButton.ForceSetToggled(rayVisualizer.RayVisible);
            gazeDebugButton.OnClicked.AddListener(() => rayVisualizer.RayVisible = gazeDebugButton.IsToggled);
        }
    }
}
