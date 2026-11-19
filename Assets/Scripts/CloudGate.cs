using UnityEngine;

public class CloudGate : MonoBehaviour
{
    public int requiredStep = 1;        // 0 = gerade, 1 = 30°, 2 = 60°, 3 = 90°
    public float gateHeight = 3;
    public bool randomMirror = true;

    // Die Wolken-Prefabs. Das Tor nimmt links und rechts zufällig eine davon.
    public GameObject[] cloudPrefabs;
    public float minSize = 0.85f;       // zufällige Grösse der Wolken
    public float maxSize = 1.1f;
    public float maxHeightOffset = 0.3f; // zufällig etwas höher oder tiefer

    public bool wasHit = false;
    public int maxStepUsed = 0;

    // So breit ist die Lücke je nach Stufe (passend zu den Widths vom Flugzeug)
    float[] gaps = { 3.9f, 2.96f, 2.15f, 1.34f };

    PlaneController plane;
    bool passed = false;

    void Start()
    {
        plane = FindFirstObjectByType<PlaneController>();

        if (cloudPrefabs.Length > 0)
        {
            float halfGap = gaps[requiredStep] / 2;
            SpawnCloud(-1, halfGap);    // links
            SpawnCloud(1, halfGap);     // rechts
        }

        if (randomMirror && Random.value < 0.5f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void SpawnCloud(int side, float halfGap)
    {
        GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        GameObject cloud = Instantiate(prefab, transform);

        // Zufällige Grösse und zufällig gespiegelt
        float size = Random.Range(minSize, maxSize);
        float flip = 1;
        if (Random.value < 0.5f) flip = -1;
        cloud.transform.localScale = new Vector3(size * flip, size, 1);

        // Linken und rechten Rand der Wolke aus dem Collider ausrechnen
        PolygonCollider2D col = cloud.GetComponent<PolygonCollider2D>();
        float leftEdge = 999;
        float rightEdge = -999;
        for (int i = 0; i < col.pathCount; i++)
        {
            foreach (Vector2 point in col.GetPath(i))
            {
                float x = (point.x + col.offset.x) * size * flip;
                if (x < leftEdge) leftEdge = x;
                if (x > rightEdge) rightEdge = x;
            }
        }

        // Wolke so hinschieben, dass ihre innere Kante genau an der Lücke liegt
        float posX;
        if (side < 0) posX = -halfGap - rightEdge;
        else posX = halfGap - leftEdge;

        float posY = Random.Range(-maxHeightOffset, maxHeightOffset);
        cloud.transform.localPosition = new Vector3(posX, posY, 0);
    }

    void Update()
    {
        if (GameManager.instance.state != GameManager.GameState.Playing) return;

        transform.position += Vector3.down * GameManager.instance.speed * Time.deltaTime;

        float distance = transform.position.y - plane.transform.position.y;

        // Merken, wie schräg der Spieler im Tor war
        if (passed == false && Mathf.Abs(distance) < gateHeight / 2)
        {
            int currentStep = Mathf.Abs(plane.step);
            if (currentStep > maxStepUsed) maxStepUsed = currentStep;
        }

        if (passed == false && distance < -gateHeight / 2)
        {
            passed = true;
            GameManager.instance.GatePassed(this);
        }

        if (transform.position.y < -15)
        {
            Destroy(gameObject);
        }
    }

    // Gelbe Hilfslinien im Editor
    void OnDrawGizmos()
    {
        if (requiredStep < 0 || requiredStep >= gaps.Length) return;

        float half = gaps[requiredStep] / 2;
        Vector3 p = transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p + new Vector3(-half, -gateHeight / 2, 0), p + new Vector3(-half, gateHeight / 2, 0));
        Gizmos.DrawLine(p + new Vector3(half, -gateHeight / 2, 0), p + new Vector3(half, gateHeight / 2, 0));
    }
}