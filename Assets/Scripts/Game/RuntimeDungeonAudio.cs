using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeDungeonAudio : MonoBehaviour
{
    private AudioSource sfx;
    private AudioSource music;
    private Dictionary<RuntimeSfx, AudioClip> clips;
    private AudioClip menuLoop;
    private AudioClip gameLoop;
    private AudioClip bossLoop;

    private void Awake()
    {
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.volume = 0.7f;

        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.volume = 0.18f;

        clips = new Dictionary<RuntimeSfx, AudioClip>
        {
            { RuntimeSfx.Attack, Tone("Attack", 420f, 0.08f, 0.4f) },
            { RuntimeSfx.Hit, Tone("Hit", 150f, 0.12f, 0.55f) },
            { RuntimeSfx.Dash, Tone("Dash", 760f, 0.1f, 0.28f) },
            { RuntimeSfx.Coin, Tone("Coin", 980f, 0.16f, 0.42f) },
            { RuntimeSfx.Door, Tone("Door", 260f, 0.3f, 0.42f) },
            { RuntimeSfx.PlayerDamage, Tone("Damage", 90f, 0.22f, 0.55f) },
            { RuntimeSfx.Cast, Tone("Cast", 520f, 0.16f, 0.34f) },
            { RuntimeSfx.Trap, Tone("Trap", 620f, 0.08f, 0.33f) },
            { RuntimeSfx.FloorFall, Tone("FloorFall", 70f, 0.35f, 0.45f) },
            { RuntimeSfx.GameOver, Tone("GameOver", 120f, 0.55f, 0.5f) },
            { RuntimeSfx.Victory, Tone("Victory", 740f, 0.65f, 0.45f) }
        };
        menuLoop = Loop("MenuLoop", 160f, 6f, 0.12f);
        gameLoop = Loop("GameLoop", 110f, 7f, 0.14f);
        bossLoop = BossLoop("BossLoop", 78f, 7.6f, 0.16f);
    }

    public void PlaySfx(RuntimeSfx sfxType)
    {
        if (clips.TryGetValue(sfxType, out AudioClip clip))
        {
            sfx.pitch = Random.Range(0.96f, 1.04f);
            sfx.PlayOneShot(clip);
        }
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuLoop);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameLoop);
    }

    public void PlayBossMusic()
    {
        PlayMusic(bossLoop);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (music.clip == clip && music.isPlaying)
        {
            return;
        }

        music.clip = clip;
        music.Play();
    }

    private static AudioClip Tone(string name, float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - t / duration;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip Loop(string name, float baseFrequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];
        float[] notes = { baseFrequency, baseFrequency * 1.33f, baseFrequency * 1.5f, baseFrequency * 1.33f };
        float beat = duration / notes.Length;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            int note = Mathf.FloorToInt(t / beat) % notes.Length;
            data[i] = Mathf.Sin(2f * Mathf.PI * notes[note] * t) * volume;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip BossLoop(string name, float baseFrequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];
        float[] notes = { baseFrequency, baseFrequency * 0.94f, baseFrequency * 1.19f, baseFrequency * 0.84f, baseFrequency * 1.5f, baseFrequency * 0.94f };
        float beat = duration / notes.Length;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            int noteIndex = Mathf.FloorToInt(t / beat) % notes.Length;
            float fundamental = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t);
            float fifth = 0.45f * Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * 1.5f * t);
            float drone = 0.35f * Mathf.Sin(2f * Mathf.PI * baseFrequency * 0.5f * t);
            float pulse = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 3.4f * t);
            data[i] = (fundamental + fifth + drone) * volume * pulse * 0.55f;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

public enum RuntimeSfx
{
    Attack,
    Hit,
    Dash,
    Coin,
    Door,
    PlayerDamage,
    Cast,
    Trap,
    FloorFall,
    GameOver,
    Victory
}

