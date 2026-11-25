using UnityEngine;
using TMPro;

public class InfoPopup : MonoBehaviour
{
    TMP_Text text;
    float timer = 1;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.alpha = 0;
    }

    public void Show(string message)
    {
        text.text = message;
        timer = 0;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
    }

    void Update()
    {
        if (timer >= 1)
        {
            return;
        }

        timer += Time.deltaTime;

        // text wird kleiner und weniger sichtbar
        float size = Mathf.Lerp(1.5f, 1f, timer * 4);
        transform.localScale = new Vector3(size, size, 1);
        text.alpha = Mathf.Clamp01(1 - timer);
    }
}
