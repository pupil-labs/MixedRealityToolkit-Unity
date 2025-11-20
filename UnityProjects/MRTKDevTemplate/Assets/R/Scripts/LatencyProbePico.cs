using PupilLabs;
using System;
using TMPro;
using UnityEngine;

public static class AndroidClock
{
#if UNITY_ANDROID && !UNITY_EDITOR
    static readonly AndroidJavaClass SystemClock = new AndroidJavaClass("android.os.SystemClock");
    static readonly AndroidJavaClass Sys = new AndroidJavaClass("java.lang.System");
#endif

    public static long UtcNowMillis()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return Sys.CallStatic<long>("currentTimeMillis");
#else
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
#endif
    }

    // Milliseconds since boot (including deep sleep)
    public static long ElapsedRealtimeMillis()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return SystemClock.CallStatic<long>("elapsedRealtime");
#else
        // Editor fallback (app-relative)
        return (long)(Time.realtimeSinceStartupAsDouble * 1000.0);
#endif
    }

    public static long UptimeMillis()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return SystemClock.CallStatic<long>("uptimeMillis");
#else
        // Rough fallback
        return (long)(Time.realtimeSinceStartupAsDouble * 1000.0);
#endif
    }
}

public class LatencyProbePico : MonoBehaviour
{
    [SerializeField]
    private NeonGazeDataProvider gazeDataProvider;

    [SerializeField]
    private TextMeshPro target;

    [SerializeField]
    private TextMeshPro targetGaze;

    private void Start()
    {
        if (gazeDataProvider == null)
        {
            gazeDataProvider = (NeonGazeDataProvider)ServiceLocator.Instance.GazeDataProvider;
        }
    }

    void LateUpdate()
    {
        double predicted = Unity.XR.PXR.PXR_System.GetPredictedDisplayTime();
        long current = AndroidClock.UptimeMillis();
        double diff = predicted - current;
        target.SetText(diff.ToString("F2"));
        Debug.Log($"[LatencyProbePico] diff: {diff}, predicted: {predicted}, current: {current}");

        long recorded = gazeDataProvider.RawGazeData.timestampMs;
        current = AndroidClock.UtcNowMillis();
        long diffGaze = current - recorded;
        targetGaze.SetText(diffGaze.ToString());
        Debug.Log($"[LatencyProbePico] diffGaze: {diffGaze}, recorded: {recorded}, current: {current}");
    }
}
