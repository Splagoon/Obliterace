using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace ObliteRace.Objects
{
    public class Ship
    {
        //Colors of the ships.
        Color[] shipColors = {Color.Blue, Color.Red, Color.Green, Color.Yellow, Color.Orange, Color.Purple, Color.Pink,
            Color.LimeGreen, Color.LightBlue, Color.HotPink, Color.Indigo, Color.Maroon, Color.Beige, Color.White, Color.Violet,
            Color.Brown, Color.Cyan, Color.DarkOliveGreen, Color.Gold, Color.Silver, Color.Gray, Color.GreenYellow, Color.Ivory,
            Color.Lavender, Color.LemonChiffon, Color.LightGoldenrodYellow, Color.MediumSeaGreen, Color.Moccasin, Color.MistyRose,
            Color.Firebrick, Color.DarkGray};
        //The ship's rotation.
        float orientation = MathHelper.ToRadians(-90);
        public float Rotation
        {
            get { return orientation; }
            set { orientation = value; }
        }
        //The ship's heading.
        Vector2 heading = Vector2.Zero;
        //The ship's location.
        Vector2 location = Vector2.Zero;
        public Vector2 Location
        {
            get { return location; }
            set { location = value; }
        }
        //The ship's speed.
        float speed = 0;
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        //The ship, itself
        Texture2D ship;
        //The SpriteBatch used to render our sprites.
        SpriteBatch batch;
        //The fire from the engine.
        Texture2D fire;
        int fireFlicker = 0;
        //How is this ship controlled?
        ControlType control;
        public ControlType ControlType
        {
            get { return control; }
        }
        //Bounds of the ship
        Rectangle bounds;
        public Rectangle Bounds
        {
            get { return bounds; }
        }
        //Are we dead?
        private bool dead = false;
        public bool Dead
        {
            get { return dead; }
        }
        //How many hit points 'till death
        int hp = 100;
        int maxHP = 100;
        public int HP
        {
            get { return hp; }
            set { hp = value; }
        }
        //Distinguishes between ships.
        int num;
        public int UI
        {
            get { return num; }
        }
        //Maximum speed.
        int maxSpeed = 50;
        //The level the ship is in.
        Rectangle levelBounds;
        //Respawn Timer
        Timer timer;
        public Timer RespawnTimer
        {
            get { return timer; }
        }
        //Total stars earned
        int stars = 0;
        public int Stars
        {
            get { return stars; }
            set { stars = value; }
        }
        //For animations, how many stars were just earned.
        double justEarnedStars = 0;
        byte justEarnedStarsFade = 0;
        //STARS!!!
        Texture2D star;
        //HUD font.
        SpriteFont hudFont;
        //Speedometer
        Texture2D speedometer;
        Texture2D needle;
        //Health Bar
        Texture2D healthBar;
        //Engine sound.
        Cue engine;
        //Powerup sound
        Cue PowerUpCue;
        //Color of the ship.
        Color shipColor = Color.White;

        public Ship(ContentManager content, GraphicsDevice device, SpriteBatch Batch, ControlType type, int UI, Level level)
        {
            ship = content.Load<Texture2D>("Sprites/Ship0");
            fire = content.Load<Texture2D>("Sprites/Fire");
            star = content.Load<Texture2D>("Sprites/Star");
            speedometer = content.Load<Texture2D>("Sprites/Speedometer");
            needle = content.Load<Texture2D>("Sprites/Needle");
            healthBar = content.Load<Texture2D>("Sprites/HealthBar");
            hudFont = content.Load<SpriteFont>("Fonts/HUD");
            engine = Sound.GetCue("engine_3");
            PowerUpCue = Sound.GetCue("engine_3");
            batch = Batch;
            control = type;
            bounds = new Rectangle(0, 0, ship.Width, ship.Width);
            int startingPoint = level.Bounds.Width / 2 - 3100 / 2;
            location.X = startingPoint + UI * 100;
            location.Y = 10000;
            num = UI;
            levelBounds = level.Bounds;
            timer = new Timer(0, 5);
            //Randomize the color.
            byte[] rgb = new byte[3];
            shipColor = shipColors[UI];
        }
        //Update logic.
        public void Update(Vector2 camera, GameTime gameTime)
        {
            if (!dead)
            {
                //Update heading.
                //We only do this while we are alive, because it's really hard to steer once you're dead.
                heading = new Vector2(
                    (float)Math.Cos(orientation), (float)Math.Sin(orientation));
                if (control == ControlType.LocalPlayer)
                    HandleInput();
                if (control == ControlType.Computer)
                    ControlAI();
            }
            else
            {
                speed = MathHelper.SmoothStep(speed, 0, .05f);
                //SPINOUT!!!
                orientation += speed / 50;
                //Countdown to respawn.
                timer.Update();
                if (timer.IsFinished)
                    Respawn();
            }
            if (hp <= 0)
                Die();
            location += heading * speed;
            location = Vector2.Clamp(location, Vector2.Zero, new Vector2(levelBounds.Width, levelBounds.Height));
            if (speed > maxSpeed)
                speed = MathHelper.SmoothStep(speed, maxSpeed, .01f);
            Random r = new Random();
            fireFlicker = r.Next(-5, 5);
            bounds.X = (int)location.X;
            bounds.Y = (int)location.Y;
            if (justEarnedStarsFade > 0)
                justEarnedStarsFade -= 1;
            CheckForCollision();
            powerTimer.Update();
            engine.SetVariable("Volume", MathHelper.Clamp(6 - Vector2.Distance(ObliteRaceGame.Ships[0].Location, location),
                -96, 6));
            //Nothing is more annoying than hearing the engine.
            //
            //if (speed > 0 && engine.IsPrepared)
            //    engine.Play();
            //if (speed > 0 && engine.IsPaused)
            //    engine.Resume();
            //if (speed <= 0)
            //    engine.Pause();
            PowerUpCue.SetVariable("Volume", MathHelper.Max(-Vector2.Distance(ObliteRaceGame.Ships[0].Location,
                            location), -96));
            if (powerTimer.IsFinished)
            {
                power = Powerups.None;
                PowerUpCue.Stop(AudioStopOptions.AsAuthored);
            }
            if (power == Powerups.Turbo)
                speed = 100;

            speed = MathHelper.Clamp(speed, 0, 100);

            hp = (int)MathHelper.Clamp(hp, 0, maxHP);
        }
        //Handle input
        void HandleInput()
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Right))
            {
                orientation += .05f;
                speed -= .5f;
            }
            if (state.IsKeyDown(Keys.Left))
            {
                orientation -= .05f;
                speed -= .5f;
            }
            if (state.IsKeyDown(Keys.Up) && speed < maxSpeed)
                speed += 0.1f;
            else
                speed = MathHelper.SmoothStep(speed, 0, .05f);

            if (state.IsKeyDown(Keys.Down))
                speed = MathHelper.SmoothStep(speed, 0, .05f);
        }
        //If AI, do automatic controlling
        void ControlAI()
        {
            //Find the most desirable target.
            Vector2 target = Vector2.Zero;
            float distance = -1;
            foreach (Ship s in ObliteRaceGame.Ships)
            {
                if (s.num != num && !s.dead)
                {
                    if (Vector2.Distance(s.Location, location) < distance || distance < 0)
                    {
                        target = s.location;
                        distance = Vector2.Distance(s.Location, location);
                    }
                }
            }
            if (power == Powerups.None || power == Powerups.Respawn)
            {
                foreach (Powerup powerX in ObliteRaceGame.Powerups)
                {
                    if (Vector2.Distance(powerX.Location, location) < distance * 2)
                        target = powerX.Location;
                }
            }
            float newOrientation = TurnToFace(location, target, orientation, .05f);
            orientation = newOrientation;
            speed -= Math.Abs(orientation - newOrientation) * 10;
            speed += 0.1f;
        }
        List<int> colliding = new List<int>();
        void CheckForCollision()
        {
            IList<Ship> ships = ObliteRaceGame.Ships;
            List<int> ints = new List<int>();
            for (int I = 0; I < ships.Count; I++)
            {
                if (I != num)
                {
                    if (bounds.Intersects(ships[I].Bounds))
                    {
                        //Crash sound.
                        ints.Add(I);
                        if (!colliding.Contains(I))
                        {
                            Cue crash = Sound.GetCue("Crash");

                            crash.SetVariable("Volume", MathHelper.Max(-Vector2.Distance(ObliteRaceGame.Ships[0].Location, 
                                location), -96));
                            crash.Play();
                            float damage = speed;
                            if (power == Powerups.Bulldozer)
                                damage = 100;
                            if (!dead && ships[I].power != Powerups.Bulldozer && ships[I].power != Powerups.Respawn)
                                ships[I].HP -= (int)damage;
                            ships[I].Bump(location, damage / 10);
                            //Star time!!!
                            if ((speed >= ships[I].Speed || power == Powerups.Bulldozer) && !ships[I].Dead)
                            {
                                //The max damage should be the max speed.
                                justEarnedStars = (damage * 5 / maxSpeed);
                                justEarnedStarsFade = 255;
                                stars += (int)justEarnedStars;
                            }
                        }
                    }
                }
            }
            colliding = ints;
        }
        private static float TurnToFace(Vector2 position, Vector2 faceThis,
            float currentAngle, float turnSpeed)
        {
            // consider this diagram:
            //         C 
            //        /|
            //      /  |
            //    /    | y
            //  / o    |
            // S--------
            //     x
            // 
            // where S is the position of the spot light, C is the position of the cat,
            // and "o" is the angle that the spot light should be facing in order to 
            // point at the cat. we need to know what o is. using trig, we know that
            //      tan(theta)       = opposite / adjacent
            //      tan(o)           = y / x
            // if we take the arctan of both sides of this equation...
            //      arctan( tan(o) ) = arctan( y / x )
            //      o                = arctan( y / x )
            // so, we can use x and y to find o, our "desiredAngle."
            // x and y are just the differences in position between the two objects.
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            float desiredAngle = (float)Math.Atan2(y, x);

            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float difference = WrapAngle(desiredAngle - currentAngle);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            return WrapAngle(currentAngle + difference);
        }

        /// <summary>
        /// What to do when the player dies.
        /// </summary>
        public void Die()
        {
            dead = true;
        }
        /// <summary>
        /// Revives the ship in a random location.
        /// </summary>
        public void Respawn()
        {
            hp = 100;
            Random r = new Random();
            location = new Vector2(r.Next(levelBounds.Width), r.Next(levelBounds.Height));
            orientation = (float)(r.NextDouble() * Math.PI);
            speed = 100;
            dead = false;
            power = Powerups.Respawn;
            powerTimer = new Timer(0, 5);
            timer = new Timer(0, 5);
        }
        /// <summary>
        /// Throws the ship off course.
        /// </summary>
        public void Bump(Vector2 bumpPos, float amount)
        {
            if (power != Powerups.Bulldozer)
                orientation = TurnToFace(location, bumpPos, -orientation, amount);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// </summary>
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }
        //Draw the ship.
        public void Draw(Vector2 camera, GameTime gameTime)
        {
            batch.Begin();

            byte a = (byte)((speed * 255) / maxSpeed);

            Color color = shipColor;
            if (dead)
                color = new Color(255, 0, 0, (byte)(timer.Time.TotalSeconds * 255 / 5));
            if (power == Powerups.Bulldozer)
            {
                Random r = new Random();
                byte[] bytes = new byte[3];
                r.NextBytes(bytes);
                color = new Color(bytes[0], bytes[1], bytes[2]);
                scale = MathHelper.SmoothStep(scale, 3, .15f);
            }
            else
                scale = MathHelper.SmoothStep(scale, 1, .15f);
            if (power == Powerups.Respawn)
                color = new Color((byte)(shipColor.R - powerTimer.Time.TotalSeconds * shipColor.R / 5), 
                    (byte)(shipColor.G - powerTimer.Time.TotalSeconds * shipColor.G / 5), 
                    (byte)(shipColor.B - powerTimer.Time.TotalSeconds * shipColor.B / 5));

            batch.Draw(fire, location - camera, null, new Color(255, 255, 255, a), orientation + MathHelper.ToRadians(90), 
                new Vector2(fire.Width / 2, fire.Height / 2 - ship.Height / 2 + fireFlicker), scale, SpriteEffects.None, 0);

            batch.Draw(ship, location - camera, null, color, orientation, new Vector2(ship.Width /
                2, ship.Height / 2), scale, SpriteEffects.None, 0);

            bounds.Width = (int)(ship.Height * scale);
            bounds.Height = (int)(ship.Height * scale);

            batch.End();
        }
        float scale = 1;
        double healthBarWidth = 0;
        public void DrawHUD()
        {
            batch.Begin();
            Rectangle screen = new Rectangle(0, 0, batch.GraphicsDevice.PresentationParameters.BackBufferWidth,
                batch.GraphicsDevice.PresentationParameters.BackBufferHeight);
            //Stars!  YAY!
            int earned = (int)justEarnedStars;
            float starPos = (float)(batch.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 -
                ((50 * earned) / 2));
            for (int I = 0; I < earned; I++)
            {
                Color color = new Color(255, 255, 255, justEarnedStarsFade);
                batch.Draw(star, new Vector2(starPos + 50 * I, 100), color);
            }
            //Total stars
            batch.Draw(star, new Vector2(screen.Width - 50, 0), Color.White);
            batch.DrawString(hudFont, stars.ToString(), new Vector2(MathHelper.Min(screen.Width - 25 - 
                (hudFont.MeasureString(stars.ToString()).X / 2), screen.Width - hudFont.MeasureString(stars.ToString()).X), 0), 
                Color.White);
            //Speedometer
            Color hudColor = new Color(255, 255, 255, 200);
            batch.Draw(speedometer, new Vector2(screen.Width - speedometer.Width, screen.Height - speedometer.Height),
                hudColor);
            batch.Draw(needle, new Vector2(screen.Width, screen.Height + 1), null, 
                hudColor, (float)(speed * Math.PI / 200 - Math.PI / 2), new Vector2(needle.Width / 2, needle.Height - 1), 1, 
                SpriteEffects.None, 0);
            //Health Bar
            healthBarWidth = (double)MathHelper.SmoothStep((float)healthBarWidth, (hp * (screen.Width - speedometer.Width) / maxHP), 0.15f);
            batch.Draw(healthBar, new Rectangle(0, screen.Height - 50, screen.Width - speedometer.Width, 50), Color.Black);
            batch.Draw(healthBar, new Rectangle(0, screen.Height - 50, (int)Math.Round(healthBarWidth, 0), 50), 
                Color.Green);
            //Time
            string time = new TimeSpan(ObliteRaceGame.GameTimer.Time.Hours, ObliteRaceGame.GameTimer.Time.Minutes,
                ObliteRaceGame.GameTimer.Time.Seconds).ToString().Remove(0, 4);
            byte red = (byte)(ObliteRaceGame.GameTimer.Time.Ticks * 255 / ObliteRaceGame.GameTimer.SetTime.Ticks);
            batch.DrawString(hudFont, time, new Vector2(screen.Width / 2 - hudFont.MeasureString(time).X / 2, 0), 
                new Color(255, red, red));
            batch.End();
        }
        Powerups power = Powerups.None;
        Timer powerTimer = new Timer(0, 0);
        public void Turbo()
        {
            power = Powerups.Turbo;
            powerTimer = new Timer(0, 2);
            //REV UP THE ENGINE
            if (PowerUpCue.IsPlaying)
                PowerUpCue.Stop(AudioStopOptions.Immediate);
            PowerUpCue = Sound.GetCue("engine_3");
            PowerUpCue.Play();
        }
        public void Bulldozer()
        {
            power = Powerups.Bulldozer;
            powerTimer = new Timer(0, 10);
            //Bulldozer is so devastating, it even has its own sound effect.
            if (PowerUpCue.IsPlaying)
                PowerUpCue.Stop(AudioStopOptions.Immediate);
            PowerUpCue = Sound.GetCue("Bulldozer");
            PowerUpCue.Play();
        }
        public void HealthPack()
        {
            hp += 50;
            //It's a Health Pack, so no need for timers or changing the power.
        }
    }
    public enum ControlType
    {
        LocalPlayer,
        Computer,
        RemotePlayer,
    }
}
