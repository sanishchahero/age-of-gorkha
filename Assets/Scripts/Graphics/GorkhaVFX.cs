using UnityEngine;

namespace Gorkha.Graphics
{
    /// <summary>
    /// Procedural visual effect generator for Gorkha: Rise of the Kingdom.
    /// Manages real-time particle systems:
    /// - Rising chimney smoke for settlement houses & palace
    /// - Flickering torch embers on fortress watchtowers & barracks
    /// - Building placement dust rings (Clash of Clans style)
    /// - Harvest sparkle bursts
    /// - Level-up celebratory confetti
    /// Works dynamically in both Play Mode and Edit Mode without external asset dependencies.
    /// </summary>
    public static class GorkhaVFX
    {
        private static GameObject _vfxContainer;

        private static Transform VFXRoot
        {
            get
            {
                if (_vfxContainer == null)
                {
                    _vfxContainer = GameObject.Find("[Gorkha_VFX_Pool]");
                    if (_vfxContainer == null)
                    {
                        _vfxContainer = new GameObject("[Gorkha_VFX_Pool]");
                    }
                }
                return _vfxContainer.transform;
            }
        }

        #region Smoke & Torches

        /// <summary>
        /// Attaches a soft continuous smoke plume rising from a chimney.
        /// </summary>
        public static GameObject AttachChimneySmoke(Transform parent, Vector3 localPos)
        {
            GameObject smokeObj = new GameObject("ChimneySmoke_VFX");
            smokeObj.transform.SetParent(parent, false);
            smokeObj.transform.localPosition = localPos;

            var ps = smokeObj.AddComponent<ParticleSystem>();
            var renderer = smokeObj.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = GorkhaMaterials.SmokeParticle;

            var main = ps.main;
            main.loop = true;
            main.startLifetime = 2.2f;
            main.startSpeed = 0.8f;
            main.startSize = 0.35f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 30;

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.08f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.4f);
            curve.AddKey(1f, 1.6f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.9f, 0.9f, 0.9f), 0f), new GradientColorKey(new Color(0.8f, 0.82f, 0.85f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.45f, 0f), new GradientAlphaKey(0.35f, 0.5f), new GradientAlphaKey(0.0f, 1f) }
            );
            colorOverLifetime.color = grad;

            var velOverLifetime = ps.velocityOverLifetime;
            velOverLifetime.enabled = true;
            velOverLifetime.x = new ParticleSystem.MinMaxCurve(0.15f); // Mountain breeze drift

            return smokeObj;
        }

        /// <summary>
        /// Attaches a warm flickering torch flame with subtle ember sparks.
        /// </summary>
        public static GameObject AttachTorchFlame(Transform parent, Vector3 localPos)
        {
            GameObject torchObj = new GameObject("TorchFlame_VFX");
            torchObj.transform.SetParent(parent, false);
            torchObj.transform.localPosition = localPos;

            var ps = torchObj.AddComponent<ParticleSystem>();
            var renderer = torchObj.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = GorkhaMaterials.FireParticle;

            var main = ps.main;
            main.loop = true;
            main.startLifetime = 0.65f;
            main.startSpeed = 1.1f;
            main.startSize = 0.22f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 25;

            var emission = ps.emission;
            emission.rateOverTime = 16f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.04f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0f), new GradientColorKey(new Color(1f, 0.3f, 0.05f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.0f, 1f) }
            );
            colorOverLifetime.color = grad;

            return torchObj;
        }

        #endregion

        #region One-Shot Burst Effects

        /// <summary>
        /// Spawns a radial dust cloud ring when a building is placed or upgraded (Clash of Clans style impact).
        /// </summary>
        public static void SpawnPlacementDust(Vector3 position, Vector2 footprintSize)
        {
            GameObject dustObj = new GameObject("PlacementDust_VFX");
            dustObj.transform.SetParent(VFXRoot, false);
            dustObj.transform.position = position + new Vector3(0, 0.1f, 0);

            var ps = dustObj.AddComponent<ParticleSystem>();
            var renderer = dustObj.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = GorkhaMaterials.DustParticle;

            var main = ps.main;
            main.loop = false;
            main.duration = 0.75f;
            main.startLifetime = 0.65f;
            main.startSpeed = 2.4f;
            main.startSize = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 28) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = Mathf.Max(footprintSize.x, footprintSize.y) * 0.9f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.3f);
            curve.AddKey(1f, 1.4f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.85f, 0.80f, 0.70f), 0f), new GradientColorKey(new Color(0.70f, 0.65f, 0.55f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            ps.Play();
        }

        /// <summary>
        /// Spawns golden harvest sparkles that fountain upwards when collecting resources.
        /// </summary>
        public static void SpawnHarvestSparkles(Vector3 position, Color color = default)
        {
            if (color == default) color = new Color(1.0f, 0.88f, 0.25f);

            GameObject sparkObj = new GameObject("HarvestSparkles_VFX");
            sparkObj.transform.SetParent(VFXRoot, false);
            sparkObj.transform.position = position;

            var ps = sparkObj.AddComponent<ParticleSystem>();
            var renderer = sparkObj.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = GorkhaMaterials.SparkleParticle;

            var main = ps.main;
            main.loop = false;
            main.duration = 0.8f;
            main.startLifetime = 0.75f;
            main.startSpeed = 3.2f;
            main.startSize = 0.28f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 24) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.4f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            ps.Play();
        }

        /// <summary>
        /// Celebratory confetti burst when building upgrades or Town Center levels up.
        /// </summary>
        public static void SpawnUpgradeConfetti(Vector3 position)
        {
            GameObject confettiObj = new GameObject("UpgradeConfetti_VFX");
            confettiObj.transform.SetParent(VFXRoot, false);
            confettiObj.transform.position = position + Vector3.up * 2f;

            var ps = confettiObj.AddComponent<ParticleSystem>();
            var renderer = confettiObj.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = GorkhaMaterials.SparkleParticle;

            var main = ps.main;
            main.loop = false;
            main.duration = 1.2f;
            main.startLifetime = 1.1f;
            main.startSpeed = 5.0f;
            main.startSize = 0.32f;
            main.gravityModifier = 0.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 45) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.6f;

            ps.Play();
        }

        #endregion
    }
}
