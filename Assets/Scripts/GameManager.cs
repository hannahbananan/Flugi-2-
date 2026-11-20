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
            if (gameOverTime > 1) Restart();
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

        if (gameOverScreen != null) gameOverScreen.SetActive(true);
        if (gameOverText != null)
        {
            gameOverText.text = "abgestürzt\n\n" + meters + " m geflogen\nrekord " + best + " m";
        }
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