using PupilLabs.Serializable;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace PupilLabs
{
    public class EyeTrackingCalibration : MonoBehaviour
    {
        [SerializeField]
        private GazeDataProvider gazeDataProvider;
        [SerializeField]
        private DataStorage storage;
        [SerializeField]
        private Transform origin;
        [SerializeField]
        private Transform targets;
        [SerializeField]
        private TMPro.TextMeshProUGUI outTxt;
        [SerializeField]
        private GameObject outUi;
        [SerializeField]
        private Transform objPointParent;
        [SerializeField]
        private GameObject objPointPrefab;
        [SerializeField]
        private Transform observedPointParent;
        [SerializeField]
        private GameObject observedPointPrefab;
        public DVector3Event calibrationFinished;

        private bool canSave = true;

        private Vector3 solvedPosition;
        private Quaternion solvedRotation;

        private List<Vector3> objPoints = new List<Vector3>();
        private List<Vector2> imgPoints = new List<Vector2>();

        private int nTargets;
        private HashSet<int> processedIds = new HashSet<int>();

        private void Awake()
        {
            if (gazeDataProvider == null)
            {
                gazeDataProvider = ServiceLocator.Instance.GazeDataProvider;
            }
            if (storage == null)
            {
                storage = ServiceLocator.Instance.GetComponentInChildren<DataStorage>(true);
            }
            calibrationFinished.AddListener(gazeDataProvider.SetGazeOrigin);

            objPointParent.gameObject.SetActive(false);
            observedPointParent.gameObject.SetActive(false);
            nTargets = targets.GetComponentsInChildren<CalibrationTarget>().Length;
            for (int i = 0; i < nTargets; i++)
            {
                GameObject.Instantiate(objPointPrefab, objPointParent);
                GameObject.Instantiate(observedPointPrefab, observedPointParent);
            }
        }

        public void AddObjPoint(Vector3 worldPos, int id)
        {
            if (processedIds.Contains(id))
            {
                return;
            }
            processedIds.Add(id);

            var localPos = targets.InverseTransformPoint(worldPos);
            objPoints.Add(localPos);
            var gazeDir = gazeDataProvider.RawGazeDir;
            imgPoints.Add(gazeDir);

            Debug.Log($"[EyeTrackingCalibration] adding object point with worldpos: {worldPos} localpos: {localPos} and image point with gaze dir {gazeDir}");

            if (objPoints.Count == nTargets)
            {
                SolvePose();
                Visualize();
                imgPoints.Clear();
                objPoints.Clear();
                processedIds.Clear();
            }
        }

        private void Visualize()
        {
            observedPointParent.position = origin.position;
            objPointParent.position = targets.position;

            for (int i = 0; i < imgPoints.Count; i++)
            {
                var imgPoint = imgPoints[i];
                var objPoint = objPoints[i];

                var opTransform = objPointParent.GetChild(i);
                opTransform.localPosition = objPoints[i]; //local in targets space

                var obspTransform = observedPointParent.GetChild(i);
                var cameraDir = new Vector3(imgPoint.x, imgPoint.y, 1); //local in sensor space
                var ray = new Ray(solvedPosition, solvedRotation * cameraDir); //local in origin space
                var distance = Vector3.Distance(targets.TransformPoint(objPoint), origin.TransformPoint(solvedPosition));
                obspTransform.localPosition = ray.GetPoint(distance);
            }

            objPointParent.gameObject.SetActive(true);
            observedPointParent.gameObject.SetActive(true);
        }

        private void SolvePose()
        {
            //using direction vectors where proper cm and dcs were already applied so use identity and zero distortion here
            float[] cm = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            float[] dcs = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            if (OpenCVWrapper.GetCameraPose(objPoints.ToArray(), imgPoints.ToArray(), cm, dcs, out UnityEngine.Pose p))
            {
                solvedPosition = origin.InverseTransformPoint(targets.TransformPoint(p.position));
                Debug.Log($"[EyeTrackingCalibration] sensor local position: {solvedPosition}");

                solvedRotation = targets.rotation * p.rotation;
                Debug.Log($"[EyeTrackingCalibration] sensor world rotation: {solvedRotation.eulerAngles}");
                solvedRotation = Quaternion.Inverse(origin.rotation) * solvedRotation;
                Debug.Log($"[EyeTrackingCalibration] sensor local rotation: {solvedRotation.eulerAngles}");

                outTxt.SetText($"Pos: {solvedPosition.x}, {solvedPosition.y}, {solvedPosition.z}<br>Rot: {solvedRotation.eulerAngles.x}, {solvedRotation.eulerAngles.y}, {solvedRotation.eulerAngles.z}");
                outUi.SetActive(true);

                calibrationFinished.Invoke(solvedPosition, solvedRotation.eulerAngles);
            }
        }

        public void TriggerSave()
        {
            Save().Forget();
        }

        public void TriggerResetDefaults()
        {
            ResetDefaults().Forget();
        }

        public async Task Save()
        {
            if (canSave == false)
            {
                return;
            }
            canSave = false;

            await storage.WhenReady();
            AppConfig config = storage.Config;
            config.sensorCalibration.offset.position.x = solvedPosition.x;
            config.sensorCalibration.offset.position.y = solvedPosition.y;
            config.sensorCalibration.offset.position.z = solvedPosition.z;
            config.sensorCalibration.offset.rotation.x = solvedRotation.eulerAngles.x;
            config.sensorCalibration.offset.rotation.y = solvedRotation.eulerAngles.y;
            config.sensorCalibration.offset.rotation.z = solvedRotation.eulerAngles.z;

            await File.WriteAllTextAsync(storage.ConfigFilePath, JsonUtility.ToJson(config, true));

            outTxt.SetText($"Saved to: {storage.ConfigFilePath}");
            canSave = true;
        }

        public async Task ResetDefaults()
        {
            if (canSave == false)
            {
                return;
            }
            canSave = false;

            await storage.WhenReady();
            AppConfig configDefaults = storage.ConfigDefaults;

            var pos = configDefaults.sensorCalibration.offset.position;
            var rot = configDefaults.sensorCalibration.offset.rotation;

            solvedPosition = new Vector3(pos.x, pos.y, pos.z);
            solvedRotation.eulerAngles = new Vector3(rot.x, rot.y, rot.z);

            outTxt.SetText($"Pos: {pos.x}, {pos.y}, {pos.z}<br>Rot: {rot.x}, {rot.y}, {rot.z}");
            canSave = true;

            calibrationFinished.Invoke(new Vector3(pos.x, pos.y, pos.z), new Vector3(rot.x, rot.y, rot.z));
        }
    }
}