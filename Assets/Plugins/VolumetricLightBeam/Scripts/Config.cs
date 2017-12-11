using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    public class Config : ScriptableObject
    {
        /// <summary>
        /// The layer the procedural geometry gameObject is in
        /// </summary>
        public int geometryLayerID = 1;

        [Range(Consts.NoiseScaleMin, Consts.NoiseScaleMax)] public float globalNoiseScale = Consts.NoiseScaleDefault;
        public Vector3 globalNoiseVelocity = Consts.NoiseVelocityDefault;
        public Vector4 globalNoiseParam { get { return new Vector4(globalNoiseVelocity.x, globalNoiseVelocity.y, globalNoiseVelocity.z, globalNoiseScale); } }

        [FormerlySerializedAs("BeamShader")]
        public Shader beamShader = null;
        public TextAsset noise3DData = null;
        public int noise3DSize = 64;

        public void Reset()
        {
            geometryLayerID = 1;
            globalNoiseScale = Consts.NoiseScaleDefault;
            globalNoiseVelocity = Consts.NoiseVelocityDefault;

            beamShader = Shader.Find("VolumetricLightBeam/Beam");
            noise3DData = Resources.Load("Noise3D_64x64x64") as TextAsset;
            noise3DSize = 64;

#if UNITY_EDITOR
            VolumetricLightBeam._EditorSetAllMeshesDirty();
#endif
        }

#if UNITY_EDITOR
        public static void EditorSelectInstance()
        {
            Selection.activeObject = Config.Instance;
            if(Selection.activeObject == null)
                Debug.LogErrorFormat("Cannot find any Config resource");
        }
#endif

        // Singleton management
        static Config m_Instance = null;
        public static Config Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    var found = Resources.LoadAll<Config>("Config");
                    Debug.Assert(found.Length != 0, string.Format("Can't find any resource of type '{0}'. Make sure you have a ScriptableObject of this type in a 'Resources' folder.", typeof(Config)));
                    m_Instance = found[0];
                }
                return m_Instance;
            }
        }
    }
}
