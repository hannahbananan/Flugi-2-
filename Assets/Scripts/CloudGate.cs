using UnityEngine;

public class CloudGate : MonoBehaviour
{
    public int requiredStep = 1;
    public float gateHeight = 3;
    public bool randomMirror = true;

    public GameObject[] cloudPrefabs;
    public float minSize = 0.85f;
    public float maxSize = 1.1f;
    public float maxHeightOffset = 0.3f;

    public bool wasHit = false;
    public int maxStepUsed = 0;

    // breite der lücke für jede stufe
    float[] gaps = { 3.9f, 2.96f, 2.15f, 1.34f };

    PlaneController plane;
    bool passed = false;

    void Start()
    {
        plane = FindFirstObjectByType<PlaneController>();

        if (cloudPrefabs.Length > 0)
        {
            float halfGap = gaps[requiredStep] / 2;
            SpawnCloud(-1, halfGap);
            SpawnCloud(1, halfGap);
        }

        if (randomMirror && Random.value < 0.5f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void SpawnCloud(int side, float halfGap)
    {
        int prefabIndex = Random.Range(0, cloudPrefabs.Length);
        GameObject cloud = Instantiate(cloudPrefabs[prefabIndex], transform);

        float size = Random.Range(minSize, maxSize);
        float flip = 1;
        if (Random.value < 0.5f)
        {
            flip = -1;
        }
        cloud.transform.localScale = new Vector3(size * flip, size, 1);

        PolygonCollider2D collider = cloud.GetComponent<PolygonCollider2D>();
        float leftEdge = 999;
        float rightEdge = -999;

        for (int i = 0; i < collider.pathCount; i++)
        {
            Vector2[] points = collider.GetPath(i);

            for (int j = 0; j < points.Length; j++)
            {
                float x = (points[j].x + collider.offset.x) * size * flip;

                if (x < leftEdge)
                {
                    leftEdge = x;
                }
                if (x > rightEdge)
                {
                    rightEdge = x;
                }
            }
        }

        float posX;
        if (side < 0)
        {
            posX = -halfGap - rightEdge;
        }
        else
        {
            posX = halfGap - leftEdge;
        }

        float posY = Random.Range(-maxHeightOffset, maxHeightOffset);
        cloud.transform.localPosition = new Vector3(posX, posY, 0);
    }

    void Update()
    {
        if (GameManager.instance.state != GameManager.GameState.Playing)
        {
            return;
        }

        transform.position += Vector3.down * GameManager.instance.speed * Time.deltaTime;

        float distance = transform.position.y - plane.transform.position.y;

        if (!passed && Mathf.Abs(distance) < gateHeight / 2)
        {
            int currentStep = Mathf.Abs(plane.step);
            if (currentStep > maxStepUsed)
            {
                maxStepUsed = currentStep;
            }
        }

        if (!passed && distance < -gateHeight / 2)
        {
            passed = true;
            GameManager.instance.GatePassed(this);
        }

        if (transform.position.y < -15)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (requiredStep < 0 || requiredStep >= gaps.Length)
        {
            return;
        }

        float half = gaps[requiredStep] / 2;
        Vector3 pos = transform.position;
        Vector3 up = new Vector3(0, gateHeight / 2, 0);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos + new Vector3(-half, 0, 0) - up, pos + new Vector3(-half, 0, 0) + up);
        Gizmos.DrawLine(pos + new Vector3(half, 0, 0) - up, pos + new Vector3(half, 0, 0) + up);
    }
}
