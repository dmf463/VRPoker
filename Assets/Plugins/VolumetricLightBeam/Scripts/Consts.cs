using UnityEngine;

namespace VLB
{
    public static class Consts
    {
        public const float FresnelPowMaxValue = 10f;

        public const float FresnelPow = 8f;

        public const float GlareFrontal = 0.5f;
        public const float GlareBehind = 0.5f;

        public const float NoiseIntensityDefault = 0.5f;

        public const float NoiseScaleMin = 0.01f;
        public const float NoiseScaleMax = 2f;
        public const float NoiseScaleDefault = 0.5f;

        public static Vector3 NoiseVelocityDefault = new Vector3(0.07f, 0.18f, 0.05f);
    }
}