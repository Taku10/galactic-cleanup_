using UnityEngine;

public class Starfield2D : MonoBehaviour
{
    public int starCount = 340;
    public int planetCount = 5;
    public float fieldSize = 95f;
    public float minStarSize = 0.025f;
    public float maxStarSize = 0.11f;
    public Color starColor = new Color(0.8f, 0.9f, 1f, 0.75f);

    Transform[] stars;
    Transform[] planets;
    Sprite starSprite;
    Sprite planetSprite;

    void Start()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        starSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);

        stars = new Transform[starCount];
        for (int i = 0; i < stars.Length; i++)
        {
            GameObject star = new GameObject("Star");
            SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
            renderer.sprite = starSprite;
            renderer.color = starColor;
            renderer.sortingOrder = -100;

            float size = Random.Range(minStarSize, maxStarSize);
            star.transform.localScale = new Vector3(size, size, 1f);
            star.transform.position = RandomPositionAroundCamera();

            stars[i] = star.transform;
        }

        planets = new Transform[planetCount];
        for (int i = 0; i < planets.Length; i++)
        {
            GameObject planet = new GameObject("DistantPlanet");
            SpriteRenderer renderer = planet.AddComponent<SpriteRenderer>();
            renderer.sprite = GetPlanetSprite();
            renderer.color = RandomPlanetColor();
            renderer.sortingOrder = -120;

            float size = Random.Range(2.5f, 7f);
            planet.transform.localScale = new Vector3(size, size, 1f);
            planet.transform.position = RandomPositionAroundCamera(-4f);

            planets[i] = planet.transform;
        }
    }

    void LateUpdate()
    {
        if (stars == null) return;

        float halfSize = fieldSize * 0.5f;
        Vector3 center = transform.position;

        foreach (Transform star in stars)
        {
            if (star == null) continue;

            Vector3 position = star.position;

            if (position.x < center.x - halfSize) position.x += fieldSize;
            if (position.x > center.x + halfSize) position.x -= fieldSize;
            if (position.y < center.y - halfSize) position.y += fieldSize;
            if (position.y > center.y + halfSize) position.y -= fieldSize;

            star.position = position;
        }

        if (planets == null) return;

        foreach (Transform planet in planets)
        {
            if (planet == null) continue;

            Vector3 position = planet.position;

            if (position.x < center.x - halfSize) position.x += fieldSize;
            if (position.x > center.x + halfSize) position.x -= fieldSize;
            if (position.y < center.y - halfSize) position.y += fieldSize;
            if (position.y > center.y + halfSize) position.y -= fieldSize;

            planet.position = position;
        }
    }

    Vector3 RandomPositionAroundCamera(float z = 5f)
    {
        Vector3 center = transform.position;
        float halfSize = fieldSize * 0.5f;

        return new Vector3(
            Random.Range(center.x - halfSize, center.x + halfSize),
            Random.Range(center.y - halfSize, center.y + halfSize),
            z
        );
    }

    Sprite GetPlanetSprite()
    {
        if (planetSprite != null)
            return planetSprite;

        Texture2D texture = new Texture2D(64, 64);
        Vector2 center = new Vector2(31.5f, 31.5f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance > 30f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float shade = Mathf.Lerp(1f, 0.35f, distance / 30f);
                if (x < 28)
                    shade *= 0.65f;

                texture.SetPixel(x, y, new Color(shade, shade, shade, 0.78f));
            }
        }

        texture.Apply();
        planetSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 32f);
        return planetSprite;
    }

    Color RandomPlanetColor()
    {
        Color[] colors =
        {
            new Color(0.28f, 0.55f, 0.95f, 0.42f),
            new Color(0.8f, 0.35f, 0.22f, 0.38f),
            new Color(0.36f, 0.82f, 0.62f, 0.36f),
            new Color(0.68f, 0.55f, 0.95f, 0.34f)
        };

        return colors[Random.Range(0, colors.Length)];
    }
}
