using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;


public class CameraSettings : MonoBehaviour
{

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
    }

    private void OnVuforiaStarted()
    {

        VuforiaBehaviour.Instance.CameraDevice.SetCameraMode(Vuforia.CameraMode.MODE_DEFAULT);
        bool focusModeSet = VuforiaBehaviour.Instance.CameraDevice.SetFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        if (!focusModeSet)
        {
            Debug.Log("Failed to set focus mode" + focusModeSet);
        }
        var exposureMode = ExposureMode.EXPOSURE_MODE_CONTINUOUSAUTO;

        if (VuforiaBehaviour.Instance.CameraDevice.IsExposureModeSupported(exposureMode))
        {
            VuforiaBehaviour.Instance.CameraDevice.SetExposureMode(ExposureMode.EXPOSURE_MODE_CONTINUOUSAUTO);

        }


    }

}
