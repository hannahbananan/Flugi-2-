using System.Collections;
using UnityEngine;

// Gewitter-Effekt für eine Wolke:
// helle Wolke flackert über der dunklen, kleiner Blitz wird zum grossen, dann klingt alles weich aus.
// Auf jede Wolke legen. Die helle Wolke und die Blitze sind Kind-Objekte.
public class CloudFlash : MonoBehaviour
{
    public SpriteRenderer lightCloud;   // Kind-Objekt mit dem hellen Wolkenbild
    public GameObject smallBolt;        // Kind-Objekt, kleiner Blitz
    public GameObject bigBolt;          // Kind-Objekt, grosser Blitz

    public float minWait = 2;           // Sekunden bis zum nächsten Gewitter (zufällig dazwischen)
    public float maxWait = 6;
    public float bigChance = 0.3f;      // Anteil der Gewitter mit grossem Blitz

    float waitTimer;
    bool isFlashing = false;
    Vector3 startScale;
    float breathOffset;

    void Start()
    {
        startScale = transform.localScale;
        breathOffset = Random.Range(0f, 10f);
        waitTimer = Random.Range(minWait, maxWait);

        SetLight(0);
        smallBolt.SetActive(false);
        bigBolt.SetActive(false);
    }

    void Update()
    {
        // Die Wolke "atmet" ganz leicht, damit sie lebendig wirkt
        float breath = 1 + Mathf.Sin(Time.time * 1.5f + breathOffset) * 0.015f;
        transform.localScale = startScale * breath;

        if (isFlashing) return;

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            float chance = Random.value;
            if (chance < bigChance) StartCoroutine(Storm(true));     // grosser Blitz
            else if (chance < 0.7f) StartCoroutine(Storm(false));    // kleiner Blitz
            else StartCoroutine(Glow());                             // nur Wetterleuchten

            waitTimer = Random.Range(minWait, maxWait);
        }
    }

    // Wird aufgerufen, wenn das Flugzeug die Wolke trifft
    public void BigStrike()
    {
        StopAllCoroutines();
        StartCoroutine(Storm(true));
    }

    IEnumerator Storm(bool big)
    {
        isFlashing = true;

        // 1. Wolke flackert kurz auf
        SetLight(0.5f);
        yield return new WaitForSeconds(0.05f);
        SetLight(0.1f);
        yield return new WaitForSeconds(0.06f);

        // 2. Kleiner Blitz, Wolke hell
        SetLight(1);
        smallBolt.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        if (big)
        {
            // 3. Der kleine Blitz wird zum grossen
            smallBolt.SetActive(false);
            bigBolt.SetActive(true);
            yield return new WaitForSeconds(0.08f);

            // kurzes Flackern im grossen Blitz
            bigBolt.SetActive(false);
            SetLight(0.4f);
            yield return new WaitForSeconds(0.04f);
            bigBolt.SetActive(true);
            SetLight(1);
            yield return new WaitForSeconds(0.12f);
        }

        // 4. Blitze weg, Wolke wird langsam wieder dunkel
        smallBolt.SetActive(false);
        bigBolt.SetActive(false);
        yield return FadeOut();

        isFlashing = false;
    }

    // Nur leichtes Flackern, ohne Blitz (wie ein Gewitter in der Ferne)
    IEnumerator Glow()
    {
        isFlashing = true;

        SetLight(0.4f);
        yield return new WaitForSeconds(0.06f);
        SetLight(0.1f);
        yield return new WaitForSeconds(0.08f);
        SetLight(0.6f);
        yield return FadeOut();

        isFlashing = false;
    }

    IEnumerator FadeOut()
    {
        float alpha = lightCloud.color.a;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 2.5f;
            SetLight(alpha);
            yield return null;
        }
        SetLight(0);
    }

    void SetLight(float alpha)
    {
        lightCloud.color = new Color(1, 1, 1, alpha);
    }
}