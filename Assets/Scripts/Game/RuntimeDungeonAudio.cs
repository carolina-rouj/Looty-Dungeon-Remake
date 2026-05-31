using System.Collections.Generic;
using UnityEngine;

// Audio 100% procedural (sin ficheros): sintetiza efectos y musica en runtime al estilo
// chiptune/arcade del Looty Dungeon original (ondas cuadrada/triangular + ruido, con
// envolventes y arpegios) en vez de simples pitidos sinusoidales.
public class RuntimeDungeonAudio : MonoBehaviour
{
    private const int SampleRate = 44100;

    private AudioSource sfx;
    private AudioSource music;
    private Dictionary<RuntimeSfx, AudioClip> clips;
    private AudioClip menuLoop;
    private AudioClip gameLoop;
    private AudioClip bossLoop;

    private enum Wave { Sine, Square, Triangle, Saw, Noise }

    private void Awake()
    {
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.volume = 0.7f;

        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.volume = 0.16f;

        clips = new Dictionary<RuntimeSfx, AudioClip>
        {
            // Golpe de espada: barrido de ruido descendente (whoosh).
            { RuntimeSfx.Attack, Sweep("Attack", 1400f, 380f, 0.12f, 0.4f, Wave.Noise) },
            // Impacto en enemigo: golpe seco (ruido corto + tono grave).
            { RuntimeSfx.Hit, Thud("Hit", 0.13f, 0.6f) },
            // Dash: whoosh ascendente.
            { RuntimeSfx.Dash, Sweep("Dash", 300f, 1100f, 0.16f, 0.3f, Wave.Noise) },
            // Moneda: arpegio brillante ascendente (el clasico "bling").
            { RuntimeSfx.Coin, Arp("Coin", new[] { 988f, 1319f }, 0.07f, 0.42f, Wave.Square) },
            // Puerta abierta: acorde ascendente suave.
            { RuntimeSfx.Door, Arp("Door", new[] { 392f, 523f, 784f }, 0.1f, 0.4f, Wave.Triangle) },
            // Daño al jugador: zumbido grave y aspero.
            { RuntimeSfx.PlayerDamage, Buzz("Damage", 150f, 0.22f, 0.55f) },
            // Hechizo: tono cristalino con vibrato.
            { RuntimeSfx.Cast, Sweep("Cast", 660f, 990f, 0.18f, 0.32f, Wave.Square) },
            // Trampa: chasquido metalico.
            { RuntimeSfx.Trap, Thud("Trap", 0.08f, 0.45f) },
            // Slime: barrido de ruido descendente (chapoteo/squelch humedo).
            { RuntimeSfx.Slime, Arp("Slime", new[] { 150f, 260f, 200f, 320f, 180f }, 0.06f, 0.45f, Wave.Triangle) },
            // Suelo que cae: retumbo grave.
            { RuntimeSfx.FloorFall, Buzz("FloorFall", 70f, 0.4f, 0.5f) },
            // Game over: arpegio descendente triste.
            { RuntimeSfx.GameOver, Arp("GameOver", new[] { 523f, 415f, 330f, 247f }, 0.16f, 0.5f, Wave.Triangle) },
            // Victoria: fanfarria ascendente.
            { RuntimeSfx.Victory, Arp("Victory", new[] { 523f, 659f, 784f, 1047f }, 0.14f, 0.45f, Wave.Square) },
            // Muerte de enemigo: "poof" descendente (vale tanto si lo mata el jugador como
            // si cae al vacio).
            { RuntimeSfx.EnemyDeath, Sweep("EnemyDeath", 540f, 120f, 0.22f, 0.5f, Wave.Square) },
            // Trampa de flecha al disparar: silbido agudo y rapido.
            { RuntimeSfx.ArrowShoot, Sweep("ArrowShoot", 1350f, 560f, 0.1f, 0.34f, Wave.Noise) },
            // Pinchos del suelo al subir: estocada metalica ascendente.
            { RuntimeSfx.Spikes, Sweep("Spikes", 480f, 1150f, 0.09f, 0.4f, Wave.Square) }
        };

        // Bucles musicales (melodia chiptune + bajo).
        menuLoop = MusicLoop("MenuLoop", new[] { 0, 4, 7, 4, 5, 9, 7, 4 }, 220f, 0.42f, 0.13f, Wave.Triangle, false);
        gameLoop = MusicLoop("GameLoop", new[] { 0, 3, 7, 10, 7, 3, 5, 2 }, 196f, 0.34f, 0.14f, Wave.Square, false);
        bossLoop = MusicLoop("BossLoop", new[] { 0, -1, 0, 3, 5, 3, 0, -2 }, 130f, 0.34f, 0.17f, Wave.Saw, true);
    }

    public void PlaySfx(RuntimeSfx sfxType)
    {
        if (clips.TryGetValue(sfxType, out AudioClip clip))
        {
            sfx.pitch = Random.Range(0.97f, 1.03f);
            sfx.PlayOneShot(clip);
        }
    }

    public void PlayMenuMusic() => PlayMusic(menuLoop);
    public void PlayGameMusic() => PlayMusic(gameLoop);
    public void PlayBossMusic() => PlayMusic(bossLoop);

    private void PlayMusic(AudioClip clip)
    {
        if (music.clip == clip && music.isPlaying)
        {
            return;
        }

        music.clip = clip;
        music.Play();
    }

    // --- Sintesis ---

    private static float Sample(Wave wave, float phase, ref float noiseState, ref int noiseHold)
    {
        switch (wave)
        {
            case Wave.Square:   return Mathf.Sin(phase) >= 0f ? 1f : -1f;
            case Wave.Triangle: return Mathf.PingPong(phase / Mathf.PI, 2f) - 1f;
            case Wave.Saw:      return 2f * (phase / (2f * Mathf.PI) - Mathf.Floor(0.5f + phase / (2f * Mathf.PI)));
            case Wave.Noise:
                if (noiseHold-- <= 0) { noiseState = Random.Range(-1f, 1f); noiseHold = 6; }
                return noiseState;
            default:            return Mathf.Sin(phase);
        }
    }

    // Tono con barrido lineal de frecuencia (whoosh / efectos).
    private static AudioClip Sweep(string name, float fStart, float fEnd, float duration, float volume, Wave wave)
    {
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        float phase = 0f, noiseState = 0f; int noiseHold = 0;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            float freq = Mathf.Lerp(fStart, fEnd, t);
            phase += 2f * Mathf.PI * freq / SampleRate;
            float env = Mathf.Sin(Mathf.PI * t);                       // ataque/caida suave
            data[i] = Sample(wave, phase, ref noiseState, ref noiseHold) * volume * env;
        }
        return Make(name, data);
    }

    // Secuencia de notas (arpegio): coin, victoria, game over, puerta.
    private static AudioClip Arp(string name, float[] freqs, float noteDuration, float volume, Wave wave)
    {
        int notes = freqs.Length;
        int perNote = Mathf.CeilToInt(SampleRate * noteDuration);
        float[] data = new float[perNote * notes];
        float noiseState = 0f; int noiseHold = 0;
        for (int n = 0; n < notes; n++)
        {
            float phase = 0f;
            for (int i = 0; i < perNote; i++)
            {
                float t = i / (float)perNote;
                phase += 2f * Mathf.PI * freqs[n] / SampleRate;
                float env = Mathf.Exp(-3.5f * t);                      // pluck con decaimiento
                data[n * perNote + i] = Sample(wave, phase, ref noiseState, ref noiseHold) * volume * env;
            }
        }
        return Make(name, data);
    }

    // Golpe seco: ruido corto mezclado con un tono grave (impactos / trampas).
    private static AudioClip Thud(string name, float duration, float volume)
    {
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        float phase = 0f, noiseState = 0f; int noiseHold = 0;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            phase += 2f * Mathf.PI * 160f / SampleRate;
            float env = Mathf.Exp(-9f * t);
            float tone = Mathf.Sin(phase);
            float noise = Sample(Wave.Noise, phase, ref noiseState, ref noiseHold);
            data[i] = (tone * 0.6f + noise * 0.4f) * volume * env;
        }
        return Make(name, data);
    }

    // Zumbido grave aspero (daño, retumbo del suelo).
    private static AudioClip Buzz(string name, float frequency, float duration, float volume)
    {
        int samples = Mathf.CeilToInt(SampleRate * duration);
        float[] data = new float[samples];
        float phase = 0f, noiseState = 0f; int noiseHold = 0;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            phase += 2f * Mathf.PI * frequency / SampleRate;
            float env = Mathf.Exp(-4f * t);
            float square = Mathf.Sin(phase) >= 0f ? 1f : -1f;
            float noise = Sample(Wave.Noise, phase, ref noiseState, ref noiseHold) * 0.3f;
            data[i] = (square * 0.7f + noise) * volume * env;
        }
        return Make(name, data);
    }

    // Bucle musical: melodia (grados de escala menor) + bajo a la octava baja.
    private static AudioClip MusicLoop(string name, int[] degrees, float rootFreq, float noteDuration, float volume, Wave wave, bool darkBass)
    {
        // Escala menor natural (semitonos): 0 2 3 5 7 8 10
        int[] minorScale = { 0, 2, 3, 5, 7, 8, 10, 12 };
        int perNote = Mathf.CeilToInt(SampleRate * noteDuration);
        float[] data = new float[perNote * degrees.Length];
        float bassPhase = 0f;
        float noiseState = 0f; int noiseHold = 0;
        for (int n = 0; n < degrees.Length; n++)
        {
            int degree = degrees[n];
            int idx = ((degree % minorScale.Length) + minorScale.Length) % minorScale.Length;
            int octave = Mathf.FloorToInt(degree / (float)minorScale.Length);
            float semis = minorScale[idx] + 12 * octave;
            float freq = rootFreq * Mathf.Pow(2f, semis / 12f);
            float bassFreq = rootFreq * 0.5f * Mathf.Pow(2f, minorScale[idx] / 12f) * (darkBass ? 0.5f : 1f);
            float melodyPhase = 0f;
            for (int i = 0; i < perNote; i++)
            {
                float t = i / (float)perNote;
                melodyPhase += 2f * Mathf.PI * freq / SampleRate;
                bassPhase += 2f * Mathf.PI * bassFreq / SampleRate;
                float env = Mathf.Min(1f, t * 12f) * Mathf.Exp(-1.6f * t);  // pequeño ataque + decaimiento
                float mel = Sample(wave, melodyPhase, ref noiseState, ref noiseHold) * env;
                float bass = (Mathf.Sin(bassPhase) >= 0f ? 1f : -1f) * 0.5f;
                data[n * perNote + i] = (mel * 0.7f + bass * 0.3f) * volume;
            }
        }
        return Make(name, data);
    }

    private static AudioClip Make(string name, float[] data)
    {
        AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
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
    Victory,
    Slime,
    EnemyDeath,
    ArrowShoot,
    Spikes
}
