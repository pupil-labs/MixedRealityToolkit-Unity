using PupilLabs;
using System;
using System.Collections.Concurrent;
using UnityEngine;

public class GazeDataLogging : MonoBehaviour
{
    [SerializeField]
    private NeonGazeDataProvider gazeDataProvider;
    [SerializeField]
    private string fileName = "rData.csv";
    [SerializeField]
    private Transform stim;
    [SerializeField]
    protected Transform reference;

    private CsvLogger logger = null;
    private RTSPClient rtspClient = null;

    private Plane plane;

    private long lastGazeDiff = 0;

    private ConcurrentQueue<GazeData> gazeDataQueue = new ConcurrentQueue<GazeData>();

    private void Start()
    {
        logger = new CsvLogger(DataUtils.GetDataPath(fileName));
        if (gazeDataProvider == null)
        {
            gazeDataProvider = (NeonGazeDataProvider)ServiceLocator.Instance.GazeDataProvider;
        }
    }
    public void OnGazeDataReceived(object sender, EventArgs e)
    {
        GazeData data = ((RTSPClient)sender).GazeData;
        gazeDataQueue.Enqueue(data);
    }

    public void Log(int type, long ts, double latency, float x, float y)
    {
        logger?.EnqueueRow($"{type}; {ts}; {latency:F3}; {x:F3}; {y:F3}{Environment.NewLine}");
    }

    private void LateUpdate()
    {
        if (rtspClient == null)
        {
            rtspClient = gazeDataProvider.RTSPClient;
            if (rtspClient == null)
            {
                return;
            }
            rtspClient.GazeDataReceived += OnGazeDataReceived;
        }

        if (reference == null)
        {
            reference = Camera.main.transform;
        }

        while (gazeDataQueue.TryDequeue(out GazeData data))
        {
            Vector2 gp = data.gazePoint;
            gazeDataProvider.ForceUpdateRawGazeDir(gp);
            Ray gazeRay = gazeDataProvider.GazeRay;
            gazeRay.origin = reference.TransformPoint(gazeRay.origin);
            gazeRay.direction = reference.TransformDirection(gazeRay.direction);
            Transform sp = stim.parent;
            plane.SetNormalAndPosition(sp.forward, sp.position);
            if (plane.Raycast(gazeRay, out float enter))
            {
                Vector3 hitPoint = gazeRay.GetPoint(enter);
                Vector3 localHitPoint = sp.InverseTransformPoint(hitPoint);
                Vector2 hp = localHitPoint;
                Debug.Log(hp);
                Log(1, data.timestampMs, lastGazeDiff, hp.x, hp.y);
            }
        }

        long millisecondsSinceEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Vector3 p = stim.localPosition;
        double predicted = Unity.XR.PXR.PXR_System.GetPredictedDisplayTime();
        long current = AndroidClock.UptimeMillis();
        long currentAbs = AndroidClock.UtcNowMillis();
        double diff = predicted - current;
        long recorded = gazeDataProvider.RawGazeData.timestampMs;
        lastGazeDiff = currentAbs - recorded;

        Log(0, currentAbs, diff, p.x, p.y);
    }

    private void OnDestroy()
    {
        logger.Dispose();
        gazeDataProvider.RTSPClient.GazeDataReceived -= OnGazeDataReceived;
    }
}
