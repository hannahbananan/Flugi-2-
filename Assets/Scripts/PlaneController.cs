using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    public int step = 0;        // -3 bis 3  (minus = links gekippt, plus = rechts gekippt)
    public int maxStep = 3;

    // Breite vom Flugzeug bei 0°, 30°, 60°, 90°
    public float[] widths = { 4.4f, 3.8f, 2.2f, 0.5f };

    // Später eure Bilder für 0°, 30°, 60°, 90°. Solange leer, wird der Balken skaliert.
    public Sprite[] planeImages;

    public SpriteRenderer planeRenderer;
    public BoxCollider2D planeCollider;

    [Header("Absturz")]
    public float crashDuration = 1.5f;  // wie lange der Absturz dauert
    public float crashForward = 4;      // wie weit er höchstens nach vorne wegfliegt
    public float crashSideways = 1.5f;  // wie weit er höchstens zur Seite abdriftet
    public float crashShake = 0.4f;     // wie stark er zittert
    public float crashWobble = 10;      // wie stark er hin und her kippt (Grad)

    bool crashing = false;
    float crashTimer = 0;
    Vector3 crashStart;
    Vector3 crashTarget;
    float noiseSeed;

    void Start()
    {
        // Sonst "schläft" das Flugzeug ein und merkt die Wolken nicht mehr
        GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.NeverSleep;

        UpdatePlane();
    }

    // Wird vom GameManager bei Game Over aufgerufen
    public void Crash()
    {
        crashing = true;
        crashTimer = 0;
        crashStart = transform.position;

        // Jedes Mal ein anderes Ziel: nach vorne, etwas links oder rechts
        float x = Random.Range(-crashSideways, crashSideways);
        float y = Random.Range(crashForward * 0.6f, crashForward);
        crashTarget = crashStart + new Vector3(x, y, 0);

        noiseSeed = Random.Range(0f, 100f);

        // Physik aus, damit nichts die Bewegung festhält
        GetComponent<Rigidbody2D>().simulated = false;
    }

    void Update()
    {
        // Absturz: nach vorne wegfliegen, wackeln und immer kleiner werden (fällt in die Tiefe)
        if (crashing && crashTimer < crashDuration)
        {
            crashTimer += Time.deltaTime;
            float t = crashTimer / crashDuration;      // läuft von 0 bis 1
            if (t > 1) t = 1;

            // erst langsam, dann immer schneller kleiner
            float size = 1 - t * t;
            transform.localScale = new Vector3(size, size, 1);

            // nach vorne weg zum zufälligen Ziel, dazu unregelmässiges Zittern
            Vector3 position = Vector3.Lerp(crashStart, crashTarget, t);
            float shakeX = (Mathf.PerlinNoise(noiseSeed, crashTimer * 6) - 0.5f) * crashShake;
            float shakeY = (Mathf.PerlinNoise(noiseSeed + 50, crashTimer * 6) - 0.5f) * crashShake;
            transform.position = position + new Vector3(shakeX, shakeY, 0);

            // unregelmässig hin und her kippen
            float wobble = (Mathf.PerlinNoise(noiseSeed + 100, crashTimer * 8) - 0.5f) * 2 * crashWobble;
            transform.rotation = Quaternion.Euler(0, 0, wobble);
        }

        bool leftPressed = Keyboard.current.aKey.wasPressedThisFrame;
        bool rightPressed = Keyboard.current.dKey.wasPressedThisFrame;

        if (leftPressed || rightPressed)
        {
            bool canTilt = GameManager.instance.ButtonPressed();

            if (canTilt)
            {
                if (rightPressed) step++;
                else step--;

                step = Mathf.Clamp(step, -maxStep, maxStep);
                UpdatePlane();
            }
        }
    }

    void UpdatePlane()
    {
        int index = Mathf.Abs(step);

        planeCollider.size = new Vector2(widths[index], planeCollider.size.y);

        if (planeImages.Length > index && planeImages[index] != null)
        {
            // Echte Bilder: nach rechts = gespiegelt
            planeRenderer.sprite = planeImages[index];
            planeRenderer.flipX = step > 0;
        }
        else
        {
            // Platzhalter: Balken einfach breiter oder schmaler machen
            Vector3 size = planeRenderer.transform.localScale;
            size.x = widths[index];
            planeRenderer.transform.localScale = size;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CloudFlash flash = other.GetComponent<CloudFlash>();
        if (flash != null) flash.BigStrike();

        CloudGate gate = other.GetComponentInParent<CloudGate>();

        if (gate != null)
        {
            GameManager.instance.PlaneHitCloud(gate);
        }
    }
}