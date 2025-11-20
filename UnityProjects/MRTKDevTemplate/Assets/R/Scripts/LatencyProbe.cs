using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class LatencyProbe : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro target;

    private XRDisplaySubsystem display = null;

    void Awake()
    {
        var displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(displays);
        //display = displays.Find(d => d.running);
        for (int i = 0; i < displays.Count; i++)
        {
            Debug.Log($"[LatencyProbe] Display instance: {displays[i]}");
            if (displays[i].running)
            {
                Debug.Log("[LatencyProbe] Active display set");
                display = displays[i];
            }
        }
    }

    void Update()
    {
        if (display != null && display.TryGetMotionToPhoton(out float mtpMs))
        {
            if (target != null)
            {
                target.SetText(mtpMs.ToString("F2"));
            }

            Debug.Log($"[LatencyProbe] OpenXR MTP: {mtpMs} ms");
        }
    }
}
