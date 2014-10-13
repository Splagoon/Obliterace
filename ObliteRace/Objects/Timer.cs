using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace ObliteRace.Objects
{
    /// <summary>
    /// Used to calculate TimeSpans.
    /// </summary>
    public class Timer
    {
        TimeSpan time = new TimeSpan();
        TimeSpan destination = new TimeSpan();
        GameTime gameTime = new GameTime();
        public TimeSpan Time
        {
            get { return destination.Subtract(time); }
        }
        public TimeSpan SetTime
        {
            get { return destination; }
        }
        bool finished = false;
        public bool IsFinished
        {
            get { return finished; }
        }
        public Timer(int Minutes, int Seconds)
        {
            destination = new TimeSpan(0, Minutes, Seconds);
            time = new TimeSpan(0);
        }
        public void Update()
        {
            time = time.Add(TimeSpan.FromMilliseconds(15));
            if (time.Ticks >= destination.Ticks)
                finished = true;
        }
    }
}
