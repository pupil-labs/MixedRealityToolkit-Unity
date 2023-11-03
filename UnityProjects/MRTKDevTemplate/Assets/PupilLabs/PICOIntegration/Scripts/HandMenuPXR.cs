using MixedReality.Toolkit.UX;
using UnityEngine;

namespace PupilLabs.PICO
{
    public class HandMenuPXR : MonoBehaviour
    {
        [SerializeField]
        private PressableButton seeThroughButton;

        private void Start()
        {
            PXRSeeThroughManager seeThroguhManager = ServiceLocator.Instance.GetComponentInChildren<PXRSeeThroughManager>();
            seeThroughButton.ForceSetToggled(seeThroguhManager.SeeThroughEnabled);
            seeThroughButton.OnClicked.AddListener(() => seeThroguhManager.EnableSeeThrough(seeThroughButton.IsToggled));
        }
    }
}
