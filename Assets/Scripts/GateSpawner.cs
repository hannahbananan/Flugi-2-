using UnityEngine;

public class GateSpawner : MonoBehaviour
{
    public CloudGate[] gates;
    public float distanceBetweenGates = 8;
    public float minDistance = 5;
    public int gatesUntilHarder = 4;

    float distanceTravelled;
    int gatesSpawned = 0;
    int lastIndex = -1;

    void Start()
    {
        distanceTravelled = distanceBetweenGates;
    }

    void Update()
    {
        if (GameManager.instance.state != GameManager.GameState.Playing)
        {
            return;
        }

        distanceTravelled += GameManager.instance.speed * Time.deltaTime;

        // abstand wird immer kleiner
        float currentDistance = distanceBetweenGates - gatesSpawned * 0.08f;
        if (currentDistance < minDistance)
        {
            currentDistance = minDistance;
        }

        if (distanceTravelled >= currentDistance)
        {
            distanceTravelled = 0;
            SpawnGate();
        }
    }

    void SpawnGate()
    {
        if (gates.Length == 0)
        {
            return;
        }

        int maxStepAllowed = 1 + gatesSpawned / gatesUntilHarder;

        int index = Random.Range(0, gates.Length);
        for (int i = 0; i < 20; i++)
        {
            index = Random.Range(0, gates.Length);

            bool notTooHard = gates[index].requiredStep <= maxStepAllowed;
            bool notSameAsLast = index != lastIndex;
            if (notTooHard && notSameAsLast)
            {
                break;
            }
        }

        lastIndex = index;
        Instantiate(gates[index], transform.position, Quaternion.identity);
        gatesSpawned++;
    }
}
