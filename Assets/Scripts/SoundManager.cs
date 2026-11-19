using UnityEngine;

// Spielt alle Sounds vom Spiel ab (Soundliste Version 2).
// Einfach auf ein leeres GameObject in der Szene ziehen.
// Im Editor werden die Sounds automatisch aus "Assets/Sounds Flugi2 1" eingetragen.
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Atmosphäre (Loop)")]
    public AudioClip engine;            // 1  Flugzeug-Geräusch: Motor während des Flugs
    public AudioClip wind;              // 2  Wind: Hintergrund

    [Header("Steuerung")]
    public AudioClip whooshShort;       // 5  Whoosh kurz: Flugzeug dreht eine Stufe
    public AudioClip whooshLong;        // 6  Whoosh lang: starke Drehung (auf 90°)

    [Header("Ereignisse")]
    public AudioClip fallShort;         // 7  Fall kurz: Wolke berührt
    public AudioClip crash;             // 9  Flugzeugsturz lang: Game Over

    [Header("Lautstärke")]
    [Range(0, 1)] public float engineVolume = 0.5f;
    [Range(0, 1)] public float windVolume = 0.4f;
    [Range(0, 1)] public float effectVolume = 1f;

    [Header("Tempo")]
    [Range(0.5f, 3f)] public float whooshShortSpeed = 1f;    // 1 = normal, 2 = doppelt so schnell (klingt auch höher)
    [Range(0.5f, 3f)] public float whooshLongSpeed = 1.5f;   // 1 = normal, 2 = doppelt so schnell (klingt auch höher)

    AudioSource engineSource;
    AudioSource windSource;
    AudioSource effectSource;
    AudioSource whooshShortSource;
    AudioSource whooshLongSource;

    void Awake()
    {
        instance = this;

        engineSource = CreateSource(engine, true, engineVolume);
        windSource = CreateSource(wind, true, windVolume);
        effectSource = CreateSource(null, false, effectVolume);
        whooshShortSource = CreateSource(null, false, effectVolume);
        whooshLongSource = CreateSource(null, false, effectVolume);
    }

    void Start()
    {
        // Wind läuft immer, auch auf dem Startbildschirm
        if (wind != null) windSource.Play();
    }

    AudioSource CreateSource(AudioClip clip, bool loop, float volume)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.volume = volume;
        source.playOnAwake = false;
        return source;
    }

    // ---------- Atmosphäre ----------

    public void StartFlight()
    {
        if (engine != null) engineSource.Play();
    }

    public void StopFlight()
    {
        engineSource.Stop();
    }

    // ---------- Steuerung ----------

    public void PlayTilt(int newStep)
    {
        // Ganz auf die Seite (90°) = starke Drehung, sonst eine Stufe
        if (Mathf.Abs(newStep) == 3) PlayWhooshLong();
        else PlayWhooshShort();
    }

    void PlayWhooshShort()
    {
        if (whooshShort == null) return;

        // Eigene AudioSource, damit nur dieser Sound schneller läuft
        whooshShortSource.pitch = whooshShortSpeed;
        whooshShortSource.PlayOneShot(whooshShort, effectVolume);
    }

    void PlayWhooshLong()
    {
        if (whooshLong == null) return;

        // Eigene AudioSource, damit nur dieser Sound schneller läuft.
        // Tempo wird bei jedem Abspielen neu gelesen, also live im Inspector testbar.
        whooshLongSource.pitch = whooshLongSpeed;
        whooshLongSource.PlayOneShot(whooshLong, effectVolume);
    }

    // ---------- Ereignisse ----------

    public void PlayCloudHit()
    {
        Play(fallShort);
    }

    public void PlayCrash()
    {
        Play(crash);
    }

    void Play(AudioClip clip)
    {
        if (clip == null) return;
        effectSource.PlayOneShot(clip, effectVolume);
    }

#if UNITY_EDITOR
    // Trägt leere Felder automatisch mit den Sounds aus dem Projekt ein
    void OnValidate()
    {
        if (engine == null) engine = FindClip("Flugzeug Geräusch");
        if (wind == null) wind = FindClip("Wind Background oder einzelne SoundEffects");
        if (whooshShort == null) whooshShort = FindClip("whoosh1");
        if (whooshLong == null) whooshLong = FindClip("Whoosch");
        if (fallShort == null) fallShort = FindClip("Fall kurz ");
        if (crash == null) crash = FindClip("Flugzeugsturz lang Retro");
    }

    AudioClip FindClip(string fileName)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Sounds Flugi2 1" });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == fileName)
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
        }
        return null;
    }
#endif
}
