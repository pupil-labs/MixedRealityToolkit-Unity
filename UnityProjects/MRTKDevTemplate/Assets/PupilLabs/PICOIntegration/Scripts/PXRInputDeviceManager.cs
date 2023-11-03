using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PXR = Unity.XR.PXR;

namespace PupilLabs.PICO
{
    public class PXRInputDeviceManager : MonoBehaviour
    {
        List<InputDevice> removedDevices = new List<InputDevice>();

        void Update()
        {
            // there is weird conflict between input device (controller) and input system (hand tracking)
            // which causes pinching / select to be bound to the controller even if it is inactive
            // this is just very ugly fix
            if (PXR.PXR_HandTracking.GetActiveInputDevice() == PXR.ActiveInputDevice.HandTrackingActive)
            {
                foreach (var controller in InputSystem.devices)
                {
                    if (controller.name.StartsWith("PICOControllerRight") && controller.usages.Count > 0 && controller.usages[0] == CommonUsages.RightHand)
                    {
                        InputSystem.RemoveDevice(controller);
                        removedDevices.Add(controller);
                    }
                    else if (controller.name.StartsWith("PICOControllerLeft") && controller.usages.Count > 0 && controller.usages[0] == CommonUsages.LeftHand)
                    {
                        InputSystem.RemoveDevice(controller);
                        removedDevices.Add(controller);
                    }
                }
            }
            else if (PXR.PXR_HandTracking.GetActiveInputDevice() == PXR.ActiveInputDevice.ControllerActive)
            {
                foreach (var controller in removedDevices)
                {
                    InputSystem.AddDevice(controller);
                }
                removedDevices.Clear();
            }
        }
    }
}
