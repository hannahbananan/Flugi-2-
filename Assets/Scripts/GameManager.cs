using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { Start, Playing, GameOver }
    public GameState state = GameState.Start;

    [Header("Höhe")]
    public float lift = 100;
    public float maxLift = 100;
    public float[] liftPerSecond = { 9, -5, -13, -24 };   // bei 0°, 30°, 60°, 90°
    public float hitDamage = 30;

    [Header("Distanz")]
    public float metersPerUnit = 10;    // wie viele Meter eine Unity-Einheit Flug sind

    [Header("Tempo")]
    public float speed = 3;
    public float speedIncrease = 0.12f;
    public float maxSpeed = 10;

    [Header("Kisten (bei tiefer Höhe)")]
    public GameObject cratePrefab;
    public float crateHeight = 30;      // unter dieser Höhe fallen Kisten
    public float crateMinWait = 4;      // Sekunden bis zur nächsten Kiste (zufällig dazwischen)
    public float crateMaxWait = 6;

    [Header("Verweise")]
    public PlaneController plane;
    public RectTransform liftBar;       // Pivot X = 0, dann wird der Balken von links her kürzer
    public TMP_Text scoreText;          // zeigt Distanz und Höhe
    public TMP_Text infoText;
    public TMP_Text gameOverText;
    public GameObject startScreen;
    public GameObject gameOverScreen;
    public GameObject hitEffect;

    float distance = 0;
    float gameOverTime = 0;
    float crateTimer = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (startScreen != null) startScreen.SetActive(true);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (infoText != null) infoText.text = "";
        UpdateUI();
    }

    void Update()
    {
        if (state == GameState.Playing)
        {
            // Distanz zählt mit dem Tempo hoch
            distance += speed * metersPerUnit * Time.deltaTime;

            // Gerade fliegen = Höhe zurück, schräg fliegen = sinken
            int index = Mathf.Abs(plane.step);
            lift = lift + liftPerSecond[index] * Time.deltaTime;
            if (lift > maxLift) lift = maxLift;

            // Bei tiefer Höhe ab und zu eine Kiste abwerfen
            if (lift < crateHeight && cratePrefab != null)
            {
                crateTimer -= Time.deltaTime;
                if (crateTimer <= 0)
                {
                    GameObject crate = Instantiate(cratePrefab, plane.transform.position, Quaternion.identity);
                    Destroy(crate, 3);
                    crateTimer = Random.Range(crateMinWait, crateMaxWait);
                }
            }

            if (lift <= 0)
            {
                lift = 0;
                GameOver();
            }

            UpdateUI();
        }

        if (state == GameState.GameOver)
        {
            gameOverTime += Time.deltaTime;
        }
    }

    // Wird bei jedem Tastendruck aufgerufen. true = Flugzeug darf kippen.
    public bool ButtonPressed()
    {
        if (state == GameState.Start)
        {
            StartGame();
            return false;
        }

        if (state == GameState.GameOver)
        {
            // Neustart erst, wenn der Absturz vorbei ist und der Screen kurz zu sehen war
            if (gameOverTime > plane.crashDuration + 0.5f) Restart();
            return false;
        }

        return true;
    }

    void StartGame()
    {
        state = GameState.Playing;
        if (startScreen != null) startScreen.SetActive(false);

        if (SoundManager.instance != null) SoundManager.instance.StartFlight();
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PlaneHitCloud(CloudGate gate)
    {
        if (state != GameState.Playing) return;
        if (gate.wasHit) return;    // pro Tor nur einmal Schaden

        gate.wasHit = true;
        lift -= hitDamage;

        if (SoundManager.instance != null) SoundManager.instance.PlayCloudHit();

        if (hitEffect != null)
        {
            Instantiate(hitEffect, plane.transform.position, Quaternion.identity);
        }

        ShowInfo("getroffen  -" + hitDamage);
    }

    // Wird von jedem Tor aufgerufen, wenn es vorbei ist: Spiel wird schneller
    public void GatePassed(CloudGate gate)
    {
        if (state != GameState.Playing) return;

        speed = speed + speedIncrease;
        if (speed > maxSpeed) speed = maxSpeed;
    }

    void GameOver()
    {
        state = GameState.GameOver;
        gameOverTime = 0;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopFlight();
            SoundManager.instance.PlayCrash();
        }

        int meters = Mathf.RoundToInt(distance);
        int best = PlayerPrefs.GetInt("bestDistance", 0);
        if (meters > best)
        {
            best = meters;
            PlayerPrefs.SetInt("bestDistance", best);
        }

        if (gameOverText != null)
        {
            gameOverText.text = "abgestürzt\n\n" + meters + " m geflogen\nrekord " + best + " m";
        }

        // Zuerst stürzt das Flugzeug ab, danach kommt der Game-Over-Screen
        plane.Crash();
        Invoke("ShowGameOverScreen", plane.crashDuration);
    }

    void ShowGameOverScreen()
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
    }

    void UpdateUI()
    {
        if (liftBar != null) liftBar.localScale = new Vector3(lift / maxLift, 1, 1);

        if (scoreText != null)
        {
            scoreText.text = Mathf.RoundToInt(distance) + " m\nhöhe " + Mathf.RoundToInt(lift);
        }
    }

    void ShowInfo(string message)
    {
        if (infoText == null) return;

        // Mit InfoPopup: aufploppen und ausblenden
        InfoPopup popup = infoText.GetComponent<InfoPopup>();
        if (popup != null)
        {
            popup.Show(message);
            return;
        }

        infoText.text = message;
        CancelInvoke("ClearInfo");
        Invoke("ClearInfo", 1f);
    }

    void ClearInfo()
    {
        infoText.text = "";
    }
}