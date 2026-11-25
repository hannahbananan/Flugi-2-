using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    // speed 1 = normal, höher = schneller
    public AudioClip engine;
    public float engineVolume = 0.5f;
    public float engineSpeed = 1f;

    public AudioClip wind;
    public float windVolume = 0.4f;
    public float windSpeed = 1f;

    public AudioClip storm2;
    public float storm2Volume = 0.4f;
    public float storm2Speed = 1f;

    public AudioClip whooshShort;
    public float whooshShortVolume = 1f;
    public float whooshShortSpeed = 1f;

    public AudioClip whooshLong;
    public float whooshLongVolume = 1f;
    public float whooshLongSpeed = 1.5f;

    public AudioClip cloudHit;
    public float cloudHitVolume = 1f;
    public float cloudHitSpeed = 0.05f;

    public AudioClip crashBlend;
    public float crashBlendVolume = 1f;
    public float crashBlendSpeed = 1f;

    public float effectVolume = 1f;

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
        if (wind != null)
        {
            windSource.Play();
        }
        if (storm2 != null)
        {
            storm2Source.Play();
        }
    }

    void Update()
    {
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

    public void StartFlight()
    {
        if (engine != null)
        {
            engineSource.Play();
        }
    }

    public void StopFlight()
    {
        engineSource.Stop();
    }

    public void PlayTilt(int newStep)
    {
        if (Mathf.Abs(newStep) == 3)
        {
            Play(whooshLongSource, whooshLong, whooshLongVolume, whooshLongSpeed);
        }
        else
        {
            Play(whooshShortSource, whooshShort, whooshShortVolume, whooshShortSpeed);
        }
    }

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
        if (clip == null)
        {
            return;
        }

        source.pitch = speed;
        source.PlayOneShot(clip, volume * effectVolume);
    }
}
