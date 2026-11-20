using UnityEngine;

// Spielt alle Sounds vom Spiel ab (Soundliste Version 2).
// Einfach auf ein leeres GameObject in der Szene ziehen.
// Im Editor werden die Sounds automatisch aus "Assets/Sounds Flugi2 1" eingetragen.
//
// Jeder Sound hat einen Lautstärke- und einen Tempo-Regler.
// Tempo: 1 = normal, 2 = doppelt so schnell (klingt dann auch höher), 0.5 = halb so schnell (tiefer).
// Alle Regler kann man während dem Spielen im Inspector ausprobieren.
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("1  Flugzeug-Geräusch (Loop, während dem Flug)")]
    public AudioClip engine;
    [Range(0, 1)] public float engineVolume = 0.5f;
    [Range(0.5f, 3f)] public float engineSpeed = 1f;

    [Header("2  Wind (Loop, immer)")]
    public AudioClip wind;
    [Range(0, 1)] public float windVolume = 0.4f;
    [Range(0.5f, 3f)] public float windSpeed = 1f;

    [Header("4  Gewitter 2 (Loop, immer)")]
    public AudioClip storm2;
    [Range(0, 1)] public float storm2Volume = 0.4f;
    [Range(0.5f, 3f)] public float storm2Speed = 1f;

    [Header("5  Whoosh kurz (eine Stufe kippen)")]
    public AudioClip whooshShort;
    [Range(0, 1)] public float whooshShortVolume = 1f;
    [Range(0.5f, 3f)] public float whooshShortSpeed = 1f;

    [Header("6  Whoosh lang (auf 90° kippen)")]
    public AudioClip whooshLong;
    [Range(0, 1)] public float whooshLongVolume = 1f;
    [Range(0.5f, 3f)] public float whooshLongSpeed = 1.5f;

    [Header("7  Wolke Blitz (Wolke berührt)")]
    public AudioClip cloudHit;
    [Range(0, 1)] public float cloudHitVolume = 1f;
    [Range(0.01f, 3f)] public float cloudHitSpeed = 0.05f;   // unter 1 = langsamer und tiefer

    [Header("9  Absturz: Blitz + Helicopter (Game Over)")]
    public AudioClip crashBlend;        // Blitz-Treffer, geht nahtlos in den Helicopter-Absturz über
    [Range(0, 1)] public float crashBlendVolume = 1f;
    [Range(0.5f, 3f)] public float crashBlendSpeed = 1f;

    [Header("Alle Effekte (5, 6, 7, 9) zusammen")]
    [Range(0, 1)] public float effectVolume = 1f;

    AudioSource engineSource;
    AudioSource windSource;
    AudioSource storm2Source;
    AudioSource whooshShortSource;
    AudioSource whooshLongSource;
    AudioSource cloudHitSource;
    AudioSource crashSource;

    void Awake()
    {
        instance = this;

        // Jeder Sound bekommt eine eigene AudioSource, damit jedes Tempo einzeln geht
        engineSource = CreateSource(engine, true);
        windSource = CreateSource(wind, true);
        storm2Source = CreateSource(storm2, true);
        whooshShortSource = CreateSource(null, false);
        whooshLongSource = CreateSource(null, false);
        cloudHitSource = CreateSource(null, false);
        crashSource = CreateSource(null, false);

        UpdateLoops();
    }

    void Start()
    {
        // Wind und Gewitter laufen immer, auch auf dem Startbildschirm
        if (wind != null) windSource.Play();
        if (storm2 != null) storm2Source.Play();
    }

    void Update()
    {
        // Damit die Regler für die Loops sofort wirken, auch während dem Spielen
        UpdateLoops();
    }

    void UpdateLoops()
    {
        engineSource.volume = engineVolume;
        engineSource.pitch = engineSpeed;
        windSource.volume = windVolume;
        windSource.pitch = windSpeed;
        storm2Source.volume = storm2Volume;
        storm2Source.pitch = storm2Speed;
    }

    AudioSource CreateSource(AudioClip clip, bool loop)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
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
        if (Mathf.Abs(newStep) == 3) Play(whooshLongSource, whooshLong, whooshLongVolume, whooshLongSpeed);
        else Play(whooshShortSource, whooshShort, whooshShortVolume, whooshShortSpeed);
    }

    // ---------- Ereignisse ----------

    public void PlayCloudHit()
    {
        Play(cloudHitSource, cloudHit, cloudHitVolume, cloudHitSpeed);
    }

    public void PlayCrash()
    {
        Play(crashSource, crashBlend, crashBlendVolume, crashBlendSpeed);
    }

    void Play(AudioSource source, AudioClip clip, float volume, float speed)
    {
        if (clip == null) return;

        // Regler werden bei jedem Abspielen neu gelesen
        source.pitch = speed;
        source.PlayOneShot(clip, volume * effectVolume);
    }

#if UNITY_EDITOR
    // Trägt leere Felder automatisch mit den Sounds aus dem Projekt ein
    void OnValidate()
    {
        if (engine == null) engine = FindClip("Flugzeug Geräusch");
        if (wind == null) wind = FindClip("Wind Background oder einzelne SoundEffects");
        if (storm2 == null) storm2 = FindClip("Gewitter 2");
        if (whooshShort == null) whooshShort = FindClip("whoosh1");
        if (whooshLong == null) whooshLong = FindClip("Whoosch");
        if (cloudHit == null) cloudHit = FindClip("wolke_blitz");
        if (crashBlend == null) crashBlend = FindClip("Absturz Blitz Helicopter");
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
