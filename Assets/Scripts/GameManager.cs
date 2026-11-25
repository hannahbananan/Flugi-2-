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

    public PlaneController plane;
    public RectTransform liftBar;
    public TMP_Text scoreText;
    public TMP_Text infoText;
    public TMP_Text gameOverText;
    public GameObject startScreen;
    public GameObject gameOverScreen;
    public GameObject hitEffect;
    public float slowSmokeSpeed = 0.4f;

    float distance = 0;
    float gameOverTime = 0;
    float crateTimer = 0;
    int hitCount = 0;

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

        UpdateUI();
    }

    void Update()
    {
        if (state == GameState.Playing)
        {
            distance += speed * metersPerUnit * Time.deltaTime;

            ChangeLift();
            DropCrates();

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

    public bool ButtonPressed()
    {
        if (state == GameState.Start)
        {
            StartGame();
            return false;
        }

        if (state == GameState.GameOver)
        {
            if (gameOverTime > plane.crashDuration + 0.5f)
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

        plane.Crash();
        Invoke("ShowGameOverScreen", plane.crashDuration);
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
            scoreText.text = Mathf.RoundToInt(distance) + " m\nhöhe " + Mathf.RoundToInt(lift);
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
