#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnalysisOverdraw
{
    private const string SHADER_NAME = "SceneView/SceneViewShowOverdraw.shader";
    private const CameraClearFlags CLEAR_FLAGS = CameraClearFlags.Color;
    private static readonly Color BACKGROUND_COLOR = Color.black;
    private const RenderingPath RENDERING_PATH = RenderingPath.Forward;

    private class CameraSettings
    {
        public CameraClearFlags cameraClearFlags;
        public Color backgroundColor;
        public RenderingPath renderingPath;

        public CameraSettings(CameraClearFlags clearFlags, Color backgroundColor, RenderingPath renderingPath)
        {
            this.cameraClearFlags = clearFlags;
            this.backgroundColor = backgroundColor;
            this.renderingPath = renderingPath;
        }
    }
    
    private static Dictionary<Camera, CameraSettings> m_cameraSettings = new Dictionary<Camera, CameraSettings>();

    [MenuItem("Analysis/Overdraw/Enable")]
    private static void Enalbe()
    {
        m_cameraSettings.Clear();

        Shader overdrawShader = EditorGUIUtility.LoadRequired(SHADER_NAME) as Shader;
        List<Camera> cameras = new List<Camera>();
        foreach(Camera camera in Camera.allCameras)
        {
            if(camera.targetTexture != null)
            {
                continue;
            }

            cameras.Add(camera);
        }

        cameras.Sort(delegate (Camera x, Camera y)
        {
            return x.depth.CompareTo(y.depth);
        });

        Camera tempCamera = null;
        for(int i = 0; i < cameras.Count; i++)
        {
            tempCamera = cameras[i];

            m_cameraSettings.Add(tempCamera, new CameraSettings(tempCamera.clearFlags, tempCamera.backgroundColor, tempCamera.renderingPath));
            tempCamera.clearFlags = i == 0 ? CLEAR_FLAGS : CameraClearFlags.Depth;
            tempCamera.backgroundColor = BACKGROUND_COLOR;
            tempCamera.renderingPath = RENDERING_PATH;
            tempCamera.SetReplacementShader(overdrawShader, "");
        }
    }

    [MenuItem("Analysis/Overdraw/Enable", true)]
    private static bool EnableValidate()
    {
        return Application.isPlaying && m_cameraSettings.Count == 0;
    }

    [MenuItem("Analysis/Overdraw/Disable")]
    private static void Disable()
    {
        foreach(KeyValuePair<Camera, CameraSettings> kvp in m_cameraSettings)
        {
            kvp.Key.clearFlags = kvp.Value.cameraClearFlags;
            kvp.Key.backgroundColor = kvp.Value.backgroundColor;
            kvp.Key.renderingPath = kvp.Value.renderingPath;
            kvp.Key.ResetReplacementShader();
        }

        m_cameraSettings.Clear();
    }

    [MenuItem("Analysis/Overdraw/Disable", true)]
    private static bool DisableValidate()
    {
        return Application.isPlaying && m_cameraSettings.Count > 0;
    }
}
#endif