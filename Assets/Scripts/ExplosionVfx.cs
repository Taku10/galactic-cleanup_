using UnityEngine;

public static class ExplosionVfx
{
    static Material particleMaterial;
    static float lastPlayerExplosionTime = -10f;

    public static void PlayEnemy(Vector3 position)
    {
        GameObject root = CreateRoot("EnemyExplosion", position);
        AddBurst(root.transform, "Core", 20, 0.45f, 0.08f, 0.28f, 1.5f, 4.5f,
            new Color(1f, 0.72f, 0.18f, 1f), new Color(1f, 0.12f, 0.02f, 0.9f), 20);
        AddBurst(root.transform, "Sparks", 16, 0.55f, 0.025f, 0.08f, 4f, 8f,
            new Color(1f, 0.95f, 0.35f, 1f), new Color(1f, 0.35f, 0.03f, 0.9f), 21);
        Object.Destroy(root, 1.2f);
    }

    public static void PlayBoss(Vector3 position)
    {
        GameObject root = CreateRoot("BossExplosion", position);
        AddBurst(root.transform, "BossCore", 95, 1.25f, 0.25f, 0.75f, 4f, 10f,
            new Color(1f, 0.25f, 0.05f, 1f), new Color(0.45f, 0.02f, 0.7f, 0.95f), 30);
        AddBurst(root.transform, "BossShockwave", 48, 0.8f, 0.45f, 1.2f, 7f, 13f,
            new Color(1f, 0.85f, 0.35f, 0.9f), new Color(1f, 0.02f, 0.08f, 0.7f), 31);
        AddBurst(root.transform, "BossSmoke", 55, 1.8f, 0.7f, 1.6f, 0.8f, 2.8f,
            new Color(0.25f, 0.05f, 0.12f, 0.7f), new Color(0.05f, 0.02f, 0.08f, 0.35f), 29);
        Object.Destroy(root, 3f);
    }

    public static void PlayPlayer(Vector3 position)
    {
        if (Time.unscaledTime - lastPlayerExplosionTime < 0.5f)
            return;

        GameObject root = CreateRoot("PlayerExplosion", position);
        AddBurst(root.transform, "PlayerFlash", 34, 0.7f, 0.14f, 0.5f, 2.5f, 7f,
            new Color(0.85f, 1f, 1f, 1f), new Color(0.08f, 0.65f, 1f, 0.9f), 25);
        AddBurst(root.transform, "PlayerDebris", 26, 1.0f, 0.04f, 0.13f, 4f, 9f,
            new Color(0.45f, 0.95f, 1f, 1f), new Color(0.04f, 0.18f, 0.85f, 0.9f), 26);
        Object.Destroy(root, 1.8f);
        lastPlayerExplosionTime = Time.unscaledTime;
    }

    static GameObject CreateRoot(string name, Vector3 position)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;
        return root;
    }

    static void AddBurst(
        Transform parent,
        string name,
        short count,
        float lifetime,
        float minSize,
        float maxSize,
        float minSpeed,
        float maxSpeed,
        Color startColor,
        Color endColor,
        int sortingOrder)
    {
        GameObject effect = new GameObject(name);
        effect.transform.SetParent(parent, false);

        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        main.duration = lifetime;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.45f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = new ParticleSystem.MinMaxGradient(startColor, endColor);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.18f;
        shape.arc = 360f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(endColor, 0.55f),
                new GradientColorKey(Color.black, 1f)
            },
            new[]
            {
                new GradientAlphaKey(startColor.a, 0f),
                new GradientAlphaKey(endColor.a, 0.45f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = sortingOrder;
        renderer.material = GetParticleMaterial();

        particles.Play();
    }

    static Material GetParticleMaterial()
    {
        if (particleMaterial != null)
            return particleMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        particleMaterial = new Material(shader);
        return particleMaterial;
    }
}
