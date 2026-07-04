using UnityEngine;

namespace YourNamespace
{
    [ExecuteAlways]
    public class VFX_FireController : MonoBehaviour
    {
        [Header("VFX Feu")]
        [SerializeField] private Color fireColor = Color.red;
        [SerializeField, Range(0f, 2f)] private float fireIntensity = 1f;
        [SerializeField] private Vector3 fireWindDirection = Vector3.zero;

        private ParticleSystem[] fireParticleSystems;
        private float[] defaultFireRateValues;
        private float[] defaultFireStartSizeValues;
        private Light fireLight;

        private void Awake()
        {
            FindFireParticles();
            ApplyFireSettings();
        }

        private void OnValidate()
        {
            if (fireParticleSystems == null || fireParticleSystems.Length == 0 ||
                defaultFireRateValues == null || defaultFireStartSizeValues == null ||
                defaultFireRateValues.Length != fireParticleSystems.Length ||
                defaultFireStartSizeValues.Length != fireParticleSystems.Length)
            {
                FindFireParticles();
            }
            ApplyFireSettings();
        }

        private void FindFireParticles()
        {
            fireParticleSystems = GetComponentsInChildren<ParticleSystem>();
            int count = fireParticleSystems.Length;
            defaultFireRateValues = new float[count];
            defaultFireStartSizeValues = new float[count];

            for (int i = 0; i < count; i++)
            {
                ParticleSystem ps = fireParticleSystems[i];
                if (ps != null)
                {
                    var mainModule = ps.main;
                    var emissionModule = ps.emission;
                    defaultFireRateValues[i] = emissionModule.rateOverTime.constant;
                    defaultFireStartSizeValues[i] = mainModule.startSize.constant;
                }
            }

            fireLight = GetComponentInChildren<Light>();
        }

        private void ApplyFireSettings()
        {
            if (fireParticleSystems == null || fireParticleSystems.Length == 0 ||
                defaultFireRateValues == null || defaultFireStartSizeValues == null ||
                defaultFireRateValues.Length != fireParticleSystems.Length ||
                defaultFireStartSizeValues.Length != fireParticleSystems.Length)
            {
                FindFireParticles();
            }

            for (int i = 0; i < fireParticleSystems.Length; i++)
            {
                ParticleSystem ps = fireParticleSystems[i];
                if (ps == null)
                    continue;

                var mainModule = ps.main;
                var emissionModule = ps.emission;
                var velocityModule = ps.velocityOverLifetime;

                mainModule.startColor = fireColor;

                float baseRate = defaultFireRateValues[i];
                if (emissionModule.rateOverTime.mode == ParticleSystemCurveMode.Constant)
                {
                    emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(baseRate * fireIntensity);
                }
                else
                {
                    emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(baseRate * fireIntensity, baseRate * fireIntensity);
                }

                float baseSize = defaultFireStartSizeValues[i];
                mainModule.startSize = new ParticleSystem.MinMaxCurve(baseSize * fireIntensity);

                if (velocityModule.enabled)
                {
                    velocityModule.xMultiplier = fireWindDirection.x;
                    velocityModule.yMultiplier = fireWindDirection.y;
                    velocityModule.zMultiplier = fireWindDirection.z;
                }
            }

            if (fireLight != null)
            {
                fireLight.intensity = fireIntensity;
                fireLight.color = fireColor;
            }
        }

        public void SetFireColor(Color newColor)
        {
            fireColor = newColor;
            ApplyFireSettings();
        }

        public void SetFireIntensity(float newIntensity)
        {
            fireIntensity = Mathf.Clamp(newIntensity, 0f, 4f);
            ApplyFireSettings();
        }

        public void SetFireWindDirection(Vector3 newWindDirection)
        {
            fireWindDirection = newWindDirection;
            ApplyFireSettings();
        }

        public Color GetFireColor() { return fireColor; }
        public float GetFireIntensity() { return fireIntensity; }
        public Vector3 GetFireWindDirection() { return fireWindDirection; }
    }
}
