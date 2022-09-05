using System;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;


public class audio : MonoBehaviour
{
    // when using four channel engine crossfading, the four clips should be:
    // lowAccelClip : The engine at low revs, with throttle open (i.e. begining acceleration at very low speed)
    // highAccelClip : Thenengine at high revs, with throttle open (i.e. accelerating, but almost at max speed)
    // lowDecelClip : The engine at low revs, with throttle at minimum (i.e. idling or engine-braking at very low speed)
    // highDecelClip : Thenengine at high revs, with throttle at minimum (i.e. engine-braking at very high speed)

    // For proper crossfading, the clips pitches should all match, with an octave offset between low and high.


    public enum EngineAudioOptions {
        Simple, // 1 audio
        FourChannel // 4 audio
    }

    public EngineAudioOptions engineSoundStyle = EngineAudioOptions.FourChannel;// Set the default audio options to be four channel
    public AudioClip lowAccelClip;                                              // Audio clip for low acceleration
    public AudioClip lowDecelClip;                                              // Audio clip for low deceleration
    public AudioClip highAccelClip;                                             // Audio clip for high acceleration
    public AudioClip highDecelClip;                                             // Audio clip for high deceleration
    public AudioClip changeGearUpClip;
    public AudioClip changeGearDownClip;
    public AudioClip backFireClip1;
    public AudioClip backFireClip2;
    public AudioClip backFireClip3;
    public AudioClip backFireClip4;
    public AudioClip burbleClip;
    [Range(0, 1)]public float engineVolume = 0.25f;
    [Range(0, 1)]public float changeGearVolume = 0.25f;
    [Range(0, 2)]public float backFireVolume = 1;
    [Range(0, 2)]public float burbleVolume = 1;
    private float m_Burblevolume;
    public float pitchMultiplier = 1f;                                          // Used for altering the pitch of audio clips
    public float lowPitchMin = 0.47f;                                           // The lowest possible pitch for the low sounds
    public float lowPitchMax = 1.5f;                                            // The highest possible pitch for the low sounds
    public float highPitchMultiplier = 0.75f;                                   // Used for altering the pitch of high sounds
    public float maxEngineRolloffDistance = 120;                                // The maximum distance where rollof starts to take place
    public float maxRolloffDistance = 120;
    public float maxFXRolloffDistance = 20;
    public float dopplerLevel = 1;                                              // The mount of doppler effect used in the audio
    public bool useDoppler = false;                                             // Toggle for using doppler

    private AudioSource m_LowAccel;    // Source for the low acceleration sounds
    private AudioSource m_LowDecel;    // Source for the low deceleration sounds
    private AudioSource m_HighAccel;   // Source for the high acceleration sounds
    private AudioSource m_HighDecel;   // Source for the high deceleration sounds
    private AudioSource m_ChangeGearUp;  // Source for the change gears
    private AudioSource m_ChangeGearDown;
    private AudioSource m_BackFire1;
    private AudioSource m_BackFire2;
    private AudioSource m_BackFire3;
    private AudioSource m_BackFire4;
    private AudioSource m_Burble;
    private bool m_StartedSound;       // flag for knowing if we have started sounds
    public carController m_CarController;
    public inputs input;
    
    //private AIcontroller aicontroler;
    
    private void StartSound() {
        if(SceneManager.GetActiveScene().name == "mainMenu") {
            engineVolume = 0;
            changeGearVolume = 0;
            backFireVolume = 0;
            burbleVolume = 0;
        }

        // setup simple audio source
        m_HighAccel = SetUpEngineAudioSource(highAccelClip);

        // setup 4 audio source
        if(engineSoundStyle == EngineAudioOptions.FourChannel) {
            m_LowAccel = SetUpEngineAudioSource(lowAccelClip);
            m_LowDecel = SetUpEngineAudioSource(lowDecelClip);
            m_HighDecel = SetUpEngineAudioSource(highDecelClip);
            m_ChangeGearUp = SetUpFXAudioSource(changeGearUpClip);
            m_ChangeGearDown = SetUpFXAudioSource(changeGearDownClip);
            m_BackFire1 = SetUpFXAudioSource(backFireClip1);
            m_BackFire2 = SetUpFXAudioSource(backFireClip2);
            m_BackFire3 = SetUpFXAudioSource(backFireClip3);
            m_BackFire4 = SetUpFXAudioSource(backFireClip4);
            m_Burble = SetUpFXAudioSource(burbleClip);
            m_Burble.Play();
        }
        // flag that we have started the sounds playing
        m_StartedSound = true;
    }


    private void StopSound() {
        //Destroy all audio sources on this object:
        foreach (var source in GetComponents<AudioSource>()) {
            Destroy(source);
        }
        m_StartedSound = false;
    }


    // Update is called once per frame
    private void FixedUpdate() {
        // get the distance to main camera
        float camDist = (Camera.main.transform.position - transform.position).sqrMagnitude;
        // stop sound if the object is beyond the maximum roll off distance
        if (m_StartedSound && camDist > maxRolloffDistance*maxRolloffDistance) {
            StopSound();
        }

        // start the sound if not playing and it is nearer than the maximum distance
        if (!m_StartedSound && camDist < maxRolloffDistance*maxRolloffDistance) {
            StartSound();
        }

        if (m_StartedSound) {
            // The pitch is interpolated between the min and max values, according to the car's revs.
            float pitch = ULerp(lowPitchMin, lowPitchMax, m_CarController.engineRPM / m_CarController.maxRPM);
            // clamp to minimum pitch (note, not clamped to max for high revs while burning out)
            pitch = Mathf.Min(lowPitchMax, pitch);

            if (engineSoundStyle == EngineAudioOptions.Simple) {
                // for 1 channel engine sound, it's oh so simple:
                m_HighAccel.pitch = pitch*pitchMultiplier*highPitchMultiplier;
                m_HighAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                m_HighAccel.volume = 1;
            } else {
                // for 4 channel engine sound, it's a little more complex:
                // adjust the pitches based on the multipliers
                m_LowAccel.pitch = pitch*pitchMultiplier;
                m_LowDecel.pitch = pitch*pitchMultiplier;
                m_HighAccel.pitch = pitch*highPitchMultiplier*pitchMultiplier;
                m_HighDecel.pitch = pitch*highPitchMultiplier*pitchMultiplier;
                float accFade = 0;
                // get values for fading the sounds based on the acceleration

                accFade = Mathf.Abs((input.vertical > 0 && !m_CarController.test ) ? input.vertical : 0);

                float decFade = 1 - accFade;

                // get the high fade value based on the cars revs
                float highFade = Mathf.InverseLerp(0.2f, 0.8f,  m_CarController.engineRPM / 10000);
                float medFade = highFade / 2;
                float lowFade = 1 - highFade;

                // adjust the values to be more realistic
                highFade = 1 - ((1 - highFade)*(1 - highFade));
                lowFade = 1 - ((1 - lowFade)*(1 - lowFade));
                accFade = 1 - ((1 - accFade)*(1 - accFade));
                decFade = 1 - ((1 - decFade)*(1 - decFade));

                // adjust the source volumes based on the fade values
                m_LowAccel.volume = (lowFade*accFade)*engineVolume;
                m_LowDecel.volume = (lowFade*decFade)*engineVolume;
                m_HighAccel.volume = (highFade*accFade)*engineVolume;
                m_HighDecel.volume = (highFade*decFade)*engineVolume;
                m_ChangeGearUp.volume = changeGearVolume;
                m_ChangeGearDown.volume = changeGearVolume;
                m_BackFire1.volume = backFireVolume;
                m_BackFire2.volume = backFireVolume;
                // m_BackFire3.volume = backFireVolume;
                // m_BackFire4.volume = backFireVolume;
                m_Burble.volume = (m_CarController.activateBurbleSound == true) ? m_Burble.volume = burbleVolume : 0;
                m_Burble.loop = true;

                // adjust the doppler levels
                m_HighAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                m_LowAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                m_HighDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                m_LowDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
            }
        }
    }

    // sets up and adds new audio source to the gane object
    private AudioSource SetUpEngineAudioSource(AudioClip clip) {
        // create the new audio source component on the game object and set up its properties
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        source.spatialBlend = 1;
        source.loop = true;

        // start the clip from a random point
        source.time = Random.Range(0f, clip.length);
        source.Play();
        source.minDistance = 5;
        source.maxDistance = maxEngineRolloffDistance;
        source.dopplerLevel = 0;
        return source;
    }

    private AudioSource SetUpFXAudioSource(AudioClip clip) {
        // create the new audio source component on the game object and set up its properties
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        source.spatialBlend = 1;
        source.loop = false;
        source.minDistance = 5;
        source.maxDistance = maxFXRolloffDistance;
        source.dopplerLevel = 0;
        return source;
    }

    public void gearUpAudio() {
        m_ChangeGearUp.Play();
    }

    public void gearDownAudio() {
        m_ChangeGearDown.Play();
    }

    public void backFireAudio() {
        int s;
        s = Random.Range(0, 6);
        if(s == 1){
            m_BackFire1.Play();
        } else if(s == 2){
            m_BackFire2.Play();
        } else if(s == 3){
            m_BackFire3.Play();
        } else if(s == 4){
            m_BackFire4.Play();
        }
    }

    // unclamped versions of Lerp and Inverse Lerp, to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value)*from + value*to;
    }
}
