using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { Start, Playing, GameOver }
    public GameState state = GameState.Start;

    public float lift = 100;
    public float maxLift = 100;
    public float[] liftPerSecond = { 9, -5, -13, -24 };
    public float hitDamage = 30;

    public float metersPerUnit = 10;

    public float speed = 3;
    public float speedIncrease = 0.12f;
    public float maxSpeed = 10;

    public GameObject cratePrefab;
    public float crateHeight = 30;
    public float crateMinWait = 4;
    public float crateMaxWait = 6;

    public float phaseLength = 2000;
    public float phaseSpeedJump = 1;
    public float breakTime = 2;
    public float radioTime = 10;
    public GameObject parachutePrefab;
    public float parachuteTime = 4;
    public float radioDelay = 1.5f;
    public AudioSource radioSound;
    public AudioClip[] radioClips;
    public GameObject radioBox;
    public TMP_Text radioText;
    [TextArea(2, 4)]
    public string[] radioMessages =
    {
        "Vindholm an Pilot: Paket ist angekommen! Danke! Aber pass auf, der Sturm wird stärker.",
        "Vindholm an Pilot: Die Medizin ist da! Halte durch, du hast schon die Hälfte geschafft.",
        "Vindholm an Pilot: Wir sehen dich schon fast! Die Blitze werden heftiger, flieg vorsichtig.",
        "Vindholm an Pilot: Wieder ein Paket gelandet! Du bist unsere Rettung!"
    };
    public int phase = 1;
    public bool inBreak = false;

    public PlaneController plane;
    public RectTransform liftBar;
    public TMP_Text scoreText;
    public TMP_Text infoText;
    public TMP_Text gameOverText;
    public GameObject startScreen;
    public GameObject gameOverScreen;
    public Transform backgroundZoom;
    public float maxZoom = 2f;
    public float zoomTime = 0.6f;
    public GameObject hitEffect;
    public float slowSmokeSpeed = 0.4f;
    public float gameOverDelay = 1f;

    float distance = 0;
    float gameOverTime = 0;
    float crateTimer = 0;
    int hitCount = 0;
    float currentZoom = 1f;
    float zoomVelocity = 0f;

    static bool skipStartScreen = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (startScreen != null)
        {
            startScreen.SetActive(true);
        }
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }
        if (infoText != null)
        {
            infoText.text = "";
        }
        if (radioBox != null)
        {
            radioBox.SetActive(false);
        }

        UpdateUI();

        if (skipStartScreen)
        {
            skipStartScreen = false;
            StartGame();
        }
    }

    void Update()
    {
        if (state == GameState.Playing)
        {
            distance += speed * metersPerUnit * Time.deltaTime;

            ChangeLift();
            ZoomBackground();
            DropCrates();
            CheckPhase();

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
            ZoomBackground();
        }
    }

    // gradeaus fliegen gibt höhe, schräg fliegen verliert höhe
    void ChangeLift()
    {
        int index = Mathf.Abs(plane.step);
        lift += liftPerSecond[index] * Time.deltaTime;

        if (lift > maxLift)
        {
            lift = maxLift;
        }
    }

    void ZoomBackground()
    {
        if (backgroundZoom != null)
        {
            float targetZoom = Mathf.Lerp(maxZoom, 1f, lift / maxLift);
            currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomTime);
            backgroundZoom.localScale = Vector3.one * currentZoom;
        }
    }

    // kisten fallen lassen, wenn höhe unter 30
    void DropCrates()
    {
        if (lift >= crateHeight || cratePrefab == null)
        {
            return;
        }

        crateTimer -= Time.deltaTime;

        if (crateTimer <= 0)
        {
            GameObject crate = Instantiate(cratePrefab, plane.transform.position, Quaternion.identity);
            Destroy(crate, 3);
            crateTimer = Random.Range(crateMinWait, crateMaxWait);
        }
    }

    // alle 2000 m: kurze pause, dann funkspruch mit paket, dann gleich nächste phase
    void CheckPhase()
    {
        if (inBreak == false && distance >= phase * phaseLength)
        {
            inBreak = true;
            Invoke("DropParachute", breakTime);
        }
    }

    // zuerst das paket abwerfen, kurz danach kommt der funkspruch
    void DropParachute()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        if (parachutePrefab != null)
        {
            GameObject parachute = Instantiate(parachutePrefab, plane.transform.position, Quaternion.identity);
            Destroy(parachute, parachuteTime);
        }

        Invoke("StartRadio", radioDelay);
    }

    void StartRadio()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        if (radioText != null && radioMessages.Length > 0)
        {
            int index = phase - 1;
            if (index >= radioMessages.Length)
            {
                index = radioMessages.Length - 1;
            }
            radioText.text = radioMessages[index];
        }

        // funkspruch abspielen (bei mehr phasen als clips wird der letzte clip wiederholt)
        if (radioSound != null && radioClips.Length > 0)
        {
            int index = phase - 1;
            if (index >= radioClips.Length)
            {
                index = radioClips.Length - 1;
            }
            radioSound.clip = radioClips[index];
            radioSound.Play();
        }

        if (radioBox != null)
        {
            radioBox.SetActive(true);
        }

        Invoke("EndRadio", radioTime);
    }

    void EndRadio()
    {
        if (radioBox != null)
        {
            radioBox.SetActive(false);
        }

        EndBreak();
    }

    // neue phase: gleich ein stück schneller und schwerere wolken
    void EndBreak()
    {
        phase++;
        speed += phaseSpeedJump;

        if (speed > maxSpeed)
        {
            speed = maxSpeed;
        }

        inBreak = false;
    }

    public void ClickButton()
    {
        ButtonPressed();
    }

    public bool ButtonPressed()
    {
        if (state == GameState.Start)
        {
            StartGame();
            return false;
        }

        if (state == GameState.GameOver)
        {
            if (gameOverTime > plane.crashDuration + gameOverDelay + 0.5f)
            {
                Restart();
            }
            return false;
        }

        return true;
    }

    void StartGame()
    {
        state = GameState.Playing;

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StartFlight();
        }
    }

    void Restart()
    {
        skipStartScreen = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PlaneHitCloud(CloudGate gate)
    {
        if (state != GameState.Playing)
        {
            return;
        }

        if (gate.wasHit)
        {
            return;
        }

        gate.wasHit = true;
        lift -= hitDamage;
        hitCount++;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayCloudHit();
        }

        if (hitEffect != null)
        {
            ShowSmoke();
        }

        ShowInfo("getroffen  -" + hitDamage);
    }

    // ab dem 2. treffer, komplette animation
    void ShowSmoke()
    {
        GameObject smoke = Instantiate(hitEffect, plane.transform);
        Animator animator = smoke.GetComponent<Animator>();
        float length = animator.runtimeAnimatorController.animationClips[0].length;

        if (hitCount >= 2)
        {
            animator.speed = slowSmokeSpeed;
            length = length / slowSmokeSpeed;
        }

        Destroy(smoke, length);
    }

    public void GatePassed(CloudGate gate)
    {
        if (state != GameState.Playing)
        {
            return;
        }

        speed += speedIncrease;

        if (speed > maxSpeed)
        {
            speed = maxSpeed;
        }
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

        if (gameOverText != null)
        {
            gameOverText.text = Mathf.RoundToInt(distance) + " m";
        }

        plane.Crash();
        Invoke("ShowGameOverScreen", plane.crashDuration + gameOverDelay);
    }

    void ShowGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
    }

    void UpdateUI()
    {
        if (liftBar != null)
        {
            liftBar.localScale = new Vector3(lift / maxLift, 1, 1);
        }

        if (scoreText != null)
        {
            scoreText.text = Mathf.RoundToInt(distance) + " m";
        }
    }

    void ShowInfo(string message)
    {
        if (infoText == null)
        {
            return;
        }

        InfoPopup popup = infoText.GetComponent<InfoPopup>();

        if (popup != null)
        {
            popup.Show(message);
        }
        else
        {
            infoText.text = message;
            CancelInvoke("ClearInfo");
            Invoke("ClearInfo", 1f);
        }
    }

    void ClearInfo()
    {
        infoText.text = "";
    }
}
