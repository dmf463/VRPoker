﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [CustomEditor(typeof(VolumetricLightBeam))]
    [CanEditMultipleObjects]
    public class VolumetricLightBeamEditor : EditorCommon
    {
        SerializedProperty trackChangesDuringPlaytime;
        SerializedProperty colorFromLight;
        SerializedProperty color;
        SerializedProperty alphaInside;
        SerializedProperty alphaOutside;
        SerializedProperty fresnelPow;
        SerializedProperty glareFrontal;
        SerializedProperty glareBehind;
        SerializedProperty spotAngleFromLight;
        SerializedProperty spotAngle;
        SerializedProperty coneRadiusStart;
        SerializedProperty geomSides;
        SerializedProperty geomCap;
        SerializedProperty fadeEndFromLight;
        SerializedProperty fadeStart;
        SerializedProperty fadeEnd;
        SerializedProperty depthBlendDistance;
        SerializedProperty cameraClippingDistance;

        // NOISE
        SerializedProperty noiseEnabled;
        SerializedProperty noiseIntensity;
        SerializedProperty noiseScaleUseGlobal;
        SerializedProperty noiseScaleLocal;
        SerializedProperty noiseVelocityUseGlobal;
        SerializedProperty noiseVelocityLocal;

        List<VolumetricLightBeam> m_Entities;
        static bool ms_ShowTips = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Entities = new List<VolumetricLightBeam>();
            foreach (var ent in targets)
            {
                if (ent is VolumetricLightBeam)
                    m_Entities.Add(ent as VolumetricLightBeam);
            }
            Debug.Assert(m_Entities.Count > 0);

            colorFromLight = FindProperty((VolumetricLightBeam x) => x.colorFromLight);
            color = FindProperty((VolumetricLightBeam x) => x.color);

            alphaInside = FindProperty((VolumetricLightBeam x) => x.alphaInside);
            alphaOutside = FindProperty((VolumetricLightBeam x) => x.alphaOutside);

            fresnelPow = FindProperty((VolumetricLightBeam x) => x.fresnelPow);

            glareFrontal = FindProperty((VolumetricLightBeam x) => x.glareFrontal);
            glareBehind = FindProperty((VolumetricLightBeam x) => x.glareBehind);

            spotAngleFromLight = FindProperty((VolumetricLightBeam x) => x.spotAngleFromLight);
            spotAngle = FindProperty((VolumetricLightBeam x) => x.spotAngle);

            coneRadiusStart = FindProperty((VolumetricLightBeam x) => x.coneRadiusStart);

            geomSides = FindProperty((VolumetricLightBeam x) => x.geomSides);
            geomCap = FindProperty((VolumetricLightBeam x) => x.geomCap);

            fadeEndFromLight = FindProperty((VolumetricLightBeam x) => x.fadeEndFromLight);
            fadeStart = FindProperty((VolumetricLightBeam x) => x.fadeStart);
            fadeEnd = FindProperty((VolumetricLightBeam x) => x.fadeEnd);

            depthBlendDistance = FindProperty((VolumetricLightBeam x) => x.depthBlendDistance);
            cameraClippingDistance = FindProperty((VolumetricLightBeam x) => x.cameraClippingDistance);

            // NOISE
            noiseEnabled = FindProperty((VolumetricLightBeam x) => x.noiseEnabled);
            noiseIntensity = FindProperty((VolumetricLightBeam x) => x.noiseIntensity);
            noiseScaleUseGlobal = FindProperty((VolumetricLightBeam x) => x.noiseScaleUseGlobal);
            noiseScaleLocal = FindProperty((VolumetricLightBeam x) => x.noiseScaleLocal);
            noiseVelocityUseGlobal = FindProperty((VolumetricLightBeam x) => x.noiseVelocityUseGlobal);
            noiseVelocityLocal = FindProperty((VolumetricLightBeam x) => x.noiseVelocityLocal);

            trackChangesDuringPlaytime = FindProperty((VolumetricLightBeam x) => x.trackChangesDuringPlaytime);
        }

        static void PropertyThickness(SerializedProperty sp)
        {
            sp.FloatSlider(
                new GUIContent("Side Thickness", "Thickness of the beam when looking at it from the side.\n1 = the beam is fully visible (no difference between the center and the edges), but produces hard edges.\nLower values produce softer transition at beam edges."),
                0, 1,
                (value) => Mathf.Clamp01(1 - (value / Consts.FresnelPowMaxValue)),    // conversion value to slider
                (value) => (1 - value) * Consts.FresnelPowMaxValue                    // conversion slider to value
                );
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Debug.Assert(m_Entities.Count > 0);

            bool hasLightSpot = false;
            var light = m_Entities[0].GetComponent<Light>();
            if (light)
            {
                hasLightSpot = light.type == LightType.Spot;
                if (!hasLightSpot)
                {
                    EditorGUILayout.HelpBox("To bind properties from the Light and the Beam together, this component must be attached to a Light of type 'Spot'", MessageType.Warning);
                }
            }

            Header("Basic");
            if (hasLightSpot)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(spotAngleFromLight.boolValue);
            }
            EditorGUILayout.PropertyField(spotAngle, new GUIContent("Spot Angle", "Define the angle (in degrees) at the base of the beam's cone"));
            if (hasLightSpot)
            {
                EditorGUI.EndDisabledGroup();
                spotAngleFromLight.ToggleFromLight();
                EditorGUILayout.EndHorizontal();
            }

            if (hasLightSpot)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(colorFromLight.boolValue);
            }
            EditorGUILayout.PropertyField(color, new GUIContent("Color", "Use the color picker to set the RGBA color of the beam (takes account of the alpha value)."));
            if (hasLightSpot)
            {
                EditorGUI.EndDisabledGroup();
                colorFromLight.ToggleFromLight();
                EditorGUILayout.EndHorizontal();
            }

            PropertyThickness(fresnelPow);

            trackChangesDuringPlaytime.ToggleLeft(
                new GUIContent(
                        "Track changes during Playtime",
                        "Check this box to be able to modify properties during Playtime via Script, Animator and/or Timeline.\nEnabling this feature is at very minor performance cost. So keep it disabled if you don't plan to modify this light beam during playtime.")
                );

            Header("Inside");
            EditorGUILayout.PropertyField(alphaInside, new GUIContent("Alpha", "Modulate the opacity of the inside geometry of the beam. Is multiplied to Color's alpha."));
            EditorGUILayout.PropertyField(glareFrontal, new GUIContent("Glare (frontal)", "Boost intensity factor when looking at the beam from the inside directly at the source."));

            Header("Outside");
            EditorGUILayout.PropertyField(alphaOutside, new GUIContent("Alpha", "Modulate the opacity of the outside geometry of the beam. Is multiplied to Color's alpha."));
            EditorGUILayout.PropertyField(glareBehind, new GUIContent("Glare (from behind)", "Boost intensity factor when looking at the beam from behind."));


            Header("Fading Distances");
            EditorGUILayout.PropertyField(fadeStart, new GUIContent("Start", "Distance from the light source (in units) the beam will start to fade out."));
            if (hasLightSpot)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(fadeEndFromLight.boolValue);
            }
            EditorGUILayout.PropertyField(fadeEnd, new GUIContent("End", "Distance from the light source (in units) the beam is entirely faded out (alpha = 0, no more cone mesh)."));
            if (hasLightSpot)
            {
                EditorGUI.EndDisabledGroup();
                fadeEndFromLight.ToggleFromLight();
                EditorGUILayout.EndHorizontal();
            }

            using (new EditorGUI.DisabledGroupScope(!Noise3D.isSupported))
            {
                Header("3D Noise");
                EditorGUILayout.PropertyField(noiseEnabled, new GUIContent("Enabled", "Enable 3D Noise effect"));

                if (noiseEnabled.boolValue)
                {
                    EditorGUILayout.PropertyField(noiseIntensity, new GUIContent("Intensity", "Higher intensity means the noise contribution is stronger and more visible"));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledGroupScope(noiseScaleUseGlobal.boolValue))
                        {
                            EditorGUILayout.PropertyField(noiseScaleLocal, new GUIContent("Scale", "3D Noise texture scaling: higher scale make the noise more visible, but potentially less realistic"));
                        }
                        noiseScaleUseGlobal.ToggleUseGlobalNoise();
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledGroupScope(noiseVelocityUseGlobal.boolValue))
                        {
                            EditorGUILayout.PropertyField(noiseVelocityLocal, new GUIContent("Velocity", "World Space direction and speed of the noise scrolling, simulating the fog/smoke movement"));
                        }
                        noiseVelocityUseGlobal.ToggleUseGlobalNoise();
                    }

                    if (GUILayout.Button(new GUIContent("Open Global Config"), EditorStyles.miniButton))
                        Config.EditorSelectInstance();
                }
            }

            if (noiseEnabled.boolValue && !Noise3D.isProperlyLoaded)
            {
                EditorGUILayout.HelpBox("Fail to load 3D noise texture. Please check your Config.", MessageType.Error);
            }

            Header("Soft Intersections Blending Distances");
            EditorGUILayout.PropertyField(cameraClippingDistance, new GUIContent("Camera", "Distance from the camera the beam will fade.\n0 = hard intersection\nHigher values produce soft intersection when the camera is near the cone triangles."));
            EditorGUILayout.PropertyField(depthBlendDistance, new GUIContent("Opaque geometry", "Distance from the world geometry the beam will fade.\n0 = hard intersection\nHigher values produce soft intersection when the beam intersects other opaque geometry."));

            Header("Cone Geometry");
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(coneRadiusStart, new GUIContent("Truncated Radius", "Radius (in units) at the beam's source (the top of the cone).\n0 will generate a perfect cone geometry.\nHigher values will generate truncated cones."));
                EditorGUI.BeginChangeCheck();
                {
                    geomCap.ToggleLeft(new GUIContent("Cap Geom", "Generate Cap Geometry (only visible from inside)"), GUILayout.MaxWidth(80.0f));
                }
                if (EditorGUI.EndChangeCheck()) { foreach (var entity in m_Entities) entity._EditorSetMeshDirty(); }
            }

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(
                    geomSides,
                    new GUIContent("Sides", "Number of Sides of the cone. Higher values give better looking results, but require more memory and graphic performance."));
            }
            if (EditorGUI.EndChangeCheck()) { foreach (var entity in m_Entities) entity._EditorSetMeshDirty(); }

            if (m_Entities.Count == 1)
            {
                EditorGUILayout.HelpBox(m_Entities[0].meshStats, MessageType.Info);
            }

            EditorGUILayout.Separator();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Default values", "Reset properties to their default values."), EditorStyles.miniButtonLeft))
                {
                    UnityEditor.Undo.RecordObjects(m_Entities.ToArray(), "Reset Light Beam Properties");
                    foreach (var entity in m_Entities) { entity.Reset(); entity.GenerateGeometry(); }
                }
                if (GUILayout.Button(new GUIContent("Regenerate geometry", "Force to re-create the Beam Geometry GameObject."), EditorStyles.miniButtonRight))
                {
                    foreach (var entity in m_Entities) entity.GenerateGeometry();
                }
            }

            if (m_Entities.Count == 1)
            {
                if (depthBlendDistance.floatValue > 0f || !Noise3D.isSupported || trackChangesDuringPlaytime.boolValue)
                {
                    ms_ShowTips = EditorGUILayout.Foldout(ms_ShowTips, "Infos");
                    if (ms_ShowTips)
                    {
                        if (depthBlendDistance.floatValue > 0f)
                        {
                            EditorGUILayout.HelpBox("To support 'Soft Intersection with Opaque Geometry', your camera must use 'DepthTextureMode.Depth'.", MessageType.Info);
#if UNITY_IPHONE || UNITY_IOS || UNITY_ANDROID
                        EditorGUILayout.HelpBox("On mobile platforms, the depth buffer precision can be pretty low. Try to keep a small depth range on your cameras: the difference between the near and far clip planes should stay as low as possible.", MessageType.Info);
#endif
                        }

                        if (!Noise3D.isSupported)
                            EditorGUILayout.HelpBox("3D Noise feature can't be supported on this platform.", MessageType.Info);

                        if (trackChangesDuringPlaytime.boolValue)
                            EditorGUILayout.HelpBox("This beam will keep track of the changes of its own properties and the spotlight attached to it (if any) during playtime. You can modify every properties except 'geomSides'.", MessageType.Info);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
