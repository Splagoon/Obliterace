using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ObliteRace.Objects
{
    class Sound
    {
        static AudioEngine audioEngine;
        static WaveBank wave;
        static SoundBank sound;
        public static void Initialize()
        {
            audioEngine = new AudioEngine("Content/Audio/Sound.xgs");
            wave = new WaveBank(audioEngine, "Content/Audio/Wave Bank.xwb");
            sound = new SoundBank(audioEngine, "Content/Audio/Sound Bank.xsb");
        }
        public static void PlayCue(string cue)
        {
            sound.PlayCue(cue);
        }
        public static void PlayCue(Cue cue)
        {
            cue.Play();
        }
        public static void PauseCue(Cue cue)
        {
            cue.Pause();
        }
        public static Cue GetCue(string cue)
        {
            return sound.GetCue(cue);
        }
    }
}
