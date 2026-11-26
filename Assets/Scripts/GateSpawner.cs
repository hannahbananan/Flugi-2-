using UnityEngine;

public class GateSpawner : MonoBehaviour
{
    public CloudGate[] gates;
    public float distanceBetweenGates = 8;
    public float minDistance = 5;
    public float randomGap = 1.5f;

    float distanceTravelled;
    int gatesSpawned = 0;
    int lastIndex = -1;
    float extraGap = 0;

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

        // in der pause zwischen den phasen keine neuen wolken
        if (GameManager.instance.inBreak)
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

        // abstand etwas zufällig, aber nie kleiner als minDistance
        float gap = currentDistance + extraGap;
        if (gap < minDistance)
        {
            gap = minDistance;
        }

        if (distanceTravelled >= gap)
        {
            distanceTravelled = 0;
            extraGap = Random.Range(-randomGap, randomGap);
            SpawnGate();
        }
    }

    void SpawnGate()
    {
        if (gates.Length == 0)
        {
            return;
        }

        // alle wolken von anfang an, nur nicht zweimal die gleiche hintereinander
        int index = Random.Range(0, gates.Length);
        for (int i = 0; i < 20; i++)
        {
            index = Random.Range(0, gates.Length);

            if (index != lastIndex)
            {
                break;
            }
        }

        lastIndex = index;
        Instantiate(gates[index], transform.position, Quaternion.identity);
        gatesSpawned++;
    }
}
