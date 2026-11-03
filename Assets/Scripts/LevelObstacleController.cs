using UnityEngine;

public class LevelObstacleController : MonoBehaviour
{
    Sprite squareSprite;
    Sprite circleSprite;
    GameObject obstacleRoot;

    public void BuildForLevel(int level, Transform player)
    {
        Clear();

        if (player == null)
            return;

        obstacleRoot = new GameObject("Level" + level + "Obstacles");

        switch (level)
        {
            case 2:
                BuildCometTrail(player.position);
                break;
            case 4:
                BuildMinefield(player.position);
                break;
            case 5:
                BuildMeteorStorm(player.position);
                break;
            case 6:
                BuildFinalScrapyard(player.position);
                break;
        }
    }

    public void Clear()
    {
        if (obstacleRoot != null)
            Destroy(obstacleRoot);

        obstacleRoot = null;
    }

    void BuildCometTrail(Vector3 center)
    {
        for (int i = 0; i < 14; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(7f, 17f);
            GameObject asteroid = CreateObstacle("ShootableAsteroid", center + (Vector3)offset, Random.Range(0.9f, 1.8f), new Color(0.52f, 0.48f, 0.42f, 1f), true);
            asteroid.AddComponent<WallHealth>().SetMaxHealth(4);

            Rigidbody2D rb = asteroid.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = Random.insideUnitCircle.normalized * Random.Range(0.35f, 0.9f);
            rb.angularVelocity = Random.Range(-45f, 45f);
        }
    }

    void BuildMinefield(Vector3 center)
    {
        for (int i = 0; i < 18; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(6f, 19f);
            GameObject mine = CreateObstacle("VolatileMine", center + (Vector3)offset, 0.75f, new Color(1f, 0.12f, 0.18f, 1f), false);

            LevelHazard hazard = mine.AddComponent<LevelHazard>();
            hazard.damage = 0.5f;
            hazard.destroyOnPlayerHit = true;
            hazard.explodeOnHit = true;
            hazard.rotateSpeed = Random.Range(-150f, 150f);
        }
    }

    void BuildMeteorStorm(Vector3 center)
    {
        for (int i = 0; i < 12; i++)
        {
            Vector3 position = center + new Vector3(Random.Range(-22f, 22f), Random.Range(-18f, 18f), 0f);
            GameObject meteor = CreateObstacle("DriftingMeteor", position, Random.Range(0.7f, 1.35f), new Color(0.75f, 0.32f, 0.18f, 1f), false);

            LevelHazard hazard = meteor.AddComponent<LevelHazard>();
            hazard.damage = 0.5f;
            hazard.rotateSpeed = Random.Range(-220f, 220f);

            Rigidbody2D rb = meteor.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(Random.Range(-2.5f, 2.5f), Random.Range(-2.5f, 2.5f));
            rb.angularVelocity = Random.Range(-90f, 90f);
        }
    }

    void BuildFinalScrapyard(Vector3 center)
    {
        for (int i = 0; i < 24; i++)
        {
            float angle = i * Mathf.PI * 2f / 24f;
            float radius = i % 2 == 0 ? 12f : 17f;
            Vector3 position = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            GameObject scrap = CreateObstacle("FinalScrapWall", position, Random.Range(0.9f, 1.6f), new Color(0.38f, 0.6f, 0.72f, 1f), true);
            scrap.AddComponent<WallHealth>().SetMaxHealth(6);
        }

        for (int i = 0; i < 8; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(5f, 14f);
            GameObject mine = CreateObstacle("FinalMine", center + (Vector3)offset, 0.65f, new Color(1f, 0.24f, 0.5f, 1f), false);
            LevelHazard hazard = mine.AddComponent<LevelHazard>();
            hazard.damage = 0.5f;
            hazard.destroyOnPlayerHit = true;
            hazard.explodeOnHit = true;
            hazard.rotateSpeed = 180f;
        }
    }

    GameObject CreateObstacle(string name, Vector3 position, float size, Color color, bool shootableWall)
    {
        GameObject obstacle = new GameObject(name);
        obstacle.transform.SetParent(obstacleRoot.transform, false);
        obstacle.transform.position = position;
        obstacle.transform.localScale = Vector3.one * size;
        if (shootableWall)
            obstacle.tag = "Wall";

        SpriteRenderer renderer = obstacle.AddComponent<SpriteRenderer>();
        renderer.sprite = shootableWall ? GetSquareSprite() : GetCircleSprite();
        renderer.color = color;
        renderer.sortingOrder = 20;

        Collider2D collider;
        if (shootableWall)
            collider = obstacle.AddComponent<BoxCollider2D>();
        else
            collider = obstacle.AddComponent<CircleCollider2D>();

        collider.isTrigger = !shootableWall;
        return obstacle;
    }

    Sprite GetSquareSprite()
    {
        if (squareSprite != null)
            return squareSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        squareSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        return squareSprite;
    }

    Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        Texture2D texture = new Texture2D(32, 32);
        Vector2 center = new Vector2(15.5f, 15.5f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance < 14f ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 32f);
        return circleSprite;
    }
}
