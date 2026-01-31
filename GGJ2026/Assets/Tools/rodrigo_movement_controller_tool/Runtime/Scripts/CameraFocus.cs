using UnityEngine;
using Unity.Cinemachine;

namespace MovementController
{
    public class CameraFocus : MonoBehaviour
    {
        public CinemachineBrain Brain;
        public ICinemachineCamera CamA;
        public ICinemachineCamera CamB;

        private void Start()
        {
            CamA= GetComponent<CinemachineCamera>();
            CamB= GetComponent<CinemachineCamera>();

            int layer = 1;
            int priority = 1;
            float weight = 1f;
            float blendtime = 0f;
            Brain.SetCameraOverride(layer, priority, CamA, CamB, weight, blendtime);
        }

    }
}
