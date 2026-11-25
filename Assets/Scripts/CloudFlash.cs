using System.Collections;
using UnityEngine;

public class CloudFlash : MonoBehaviour
{
    public SpriteRenderer lightCloud;
    public GameObject smallBolt;
    public GameObject bigBolt;

    public float minWait = 2;
    public float maxWait = 6;
    public float bigChance = 0.3f;

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
        float breath = 1 + Mathf.Sin(Time.time * 1.5f + breathOffset) * 0.015f;
        transform.localScale = startScale * breath;

        if (isFlashing)
        {
            return;
        }

        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0)
        {
            float chance = Random.value;

            if (chance < bigChance)
            {
                StartCoroutine(Storm(true));
            }
            else if (chance < 0.7f)
            {
                StartCoroutine(Storm(false));
            }
            else
            {
                StartCoroutine(Glow());
            }

            waitTimer = Random.Range(minWait, maxWait);
        }
    }

    // blitz, wenn das flugzeug die wolke trifft
    public void BigStrike()
    {
        StopAllCoroutines();
        StartCoroutine(Storm(true));
    }

    IEnumerator Storm(bool big)
    {
        isFlashing = true;

        SetLight(0.5f);
        yield return new WaitForSeconds(0.05f);
        SetLight(0.1f);
        yield return new WaitForSeconds(0.06f);

        SetLight(1);
        smallBolt.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        if (big)
        {
            smallBolt.SetActive(false);
            bigBolt.SetActive(true);
            yield return new WaitForSeconds(0.08f);

            bigBolt.SetActive(false);
            SetLight(0.4f);
            yield return new WaitForSeconds(0.04f);

            bigBolt.SetActive(true);
            SetLight(1);
            yield return new WaitForSeconds(0.12f);
        }

        smallBolt.SetActive(false);
        bigBolt.SetActive(false);
        yield return FadeOut();

        isFlashing = false;
    }

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
