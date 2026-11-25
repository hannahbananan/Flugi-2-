using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    // -3 bis 3, minus is links
    public int step = 0;
    public int maxStep = 3;

    public float[] widths = { 4.4f, 3.8f, 2.2f, 0.5f };

    public Sprite[] planeImages;

    public SpriteRenderer planeRenderer;
    public BoxCollider2D planeCollider;

    public float crashDuration = 1.5f;
    public float crashForward = 4;
    public float crashSideways = 1.5f;
    public float crashShake = 0.4f;
    public float crashWobble = 10;

    bool crashing = false;
    float crashTimer = 0;
    Vector3 crashStart;
    Vector3 crashTarget;
    float noiseSeed;

    void Start()
    {
        GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.NeverSleep;
        UpdatePlane();
    }

    void Update()
    {
        if (crashing && crashTimer < crashDuration)
        {
            CrashMovement();
        }

        CheckKeys();
    }

    void CheckKeys()
    {
        bool leftPressed = Keyboard.current.aKey.wasPressedThisFrame;
        bool rightPressed = Keyboard.current.dKey.wasPressedThisFrame;

        if (!leftPressed && !rightPressed)
        {
            return;
        }

        bool canTilt = GameManager.instance.ButtonPressed();
        if (!canTilt)
        {
            return;
        }

        int oldStep = step;

        if (rightPressed)
        {
            step++;
        }
        else
        {
            step--;
        }

        step = Mathf.Clamp(step, -maxStep, maxStep);
        UpdatePlane();

        if (step != oldStep && SoundManager.instance != null)
        {
            SoundManager.instance.PlayTilt(step);
        }
    }

    void UpdatePlane()
    {
        int index = Mathf.Abs(step);

        planeCollider.size = new Vector2(widths[index], planeCollider.size.y);

        if (planeImages.Length > index && planeImages[index] != null)
        {
            planeRenderer.sprite = planeImages[index];
            planeRenderer.flipX = step > 0;
        }
        else
        {
            Vector3 size = planeRenderer.transform.localScale;
            size.x = widths[index];
            planeRenderer.transform.localScale = size;
        }
    }

    public void Crash()
    {
        crashing = true;
        crashTimer = 0;
        crashStart = transform.position;

        float x = Random.Range(-crashSideways, crashSideways);
        float y = Random.Range(crashForward * 0.6f, crashForward);
        crashTarget = crashStart + new Vector3(x, y, 0);

        noiseSeed = Random.Range(0f, 100f);

        GetComponent<Rigidbody2D>().simulated = false;
    }

    // flugzeug abstürzen
    void CrashMovement()
    {
        crashTimer += Time.deltaTime;

        float t = crashTimer / crashDuration;
        if (t > 1)
        {
            t = 1;
        }

        float size = 1 - t * t;
        transform.localScale = new Vector3(size, size, 1);

        Vector3 position = Vector3.Lerp(crashStart, crashTarget, t);
        float shakeX = (Mathf.PerlinNoise(noiseSeed, crashTimer * 6) - 0.5f) * crashShake;
        float shakeY = (Mathf.PerlinNoise(noiseSeed + 50, crashTimer * 6) - 0.5f) * crashShake;
        transform.position = position + new Vector3(shakeX, shakeY, 0);

        float wobble = (Mathf.PerlinNoise(noiseSeed + 100, crashTimer * 8) - 0.5f) * 2 * crashWobble;
        transform.rotation = Quaternion.Euler(0, 0, wobble);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CloudFlash flash = other.GetComponent<CloudFlash>();
        if (flash != null)
        {
            flash.BigStrike();
        }

        CloudGate gate = other.GetComponentInParent<CloudGate>();
        if (gate != null)
        {
            GameManager.instance.PlaneHitCloud(gate);
        }
    }
}
