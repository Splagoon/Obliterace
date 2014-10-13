using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ObliteRace.Objects
{
    class PowerupManager
    {
        Texture2D turbo;
        Texture2D bulldozer;
        Texture2D healthPack;
        SpriteBatch batch;
        Rectangle bounds;
        Timer spawnTimer;
        List<Powerup> powerups = new List<Powerup>();
        public Powerup[] PowerupCollection
        {
            get { return powerups.ToArray(); }
        }
        public PowerupManager(ContentManager content, SpriteBatch Batch, Level level)
        {
            turbo = content.Load<Texture2D>("Sprites/Powerups/Turbo");
            bulldozer = content.Load<Texture2D>("Sprites/Powerups/Bulldozer");
            healthPack = content.Load<Texture2D>("Sprites/Powerups/HealthPack");
            batch = Batch;
            bounds = level.Bounds;
            spawnTimer = new Timer(0, 5);
        }
        public void Update()
        {
            if (spawnTimer.IsFinished)
            {
                spawnTimer = new Timer(0, 5);
                Random r = new Random();
                Powerups[] powers = (Powerups[])Enum.GetValues(typeof(Powerups));
                powerups.Add(new Powerup(new Vector2(r.Next(bounds.Width), r.Next(bounds.Height)),
                    powers[r.Next(powers.Length - 1)]));
            }
            spawnTimer.Update();
            for (int I = 0; I < powerups.Count; I++)
            {
                Powerup power = powerups[I];
                power.Location += powerups[I].Heading;
                power.Rotation += .05f;
                if (power.Location.X <= 0 || power.Location.X >= bounds.Width ||
                    power.Location.Y <= 0 || power.Location.Y >= bounds.Height)
                {
                    Random r = new Random();
                    power.Heading = new Vector2((float)r.NextDouble(), (float)r.NextDouble());
                }
                power.Location = Vector2.Clamp(power.Location, Vector2.Zero, new Vector2(bounds.Width, bounds.Height));
                powerups[I] = power;
                CheckForCollision(I);
            }
        }
        void CheckForCollision(int I)
        {
            Powerup power = powerups[I];
            foreach (Ship s in ObliteRaceGame.Ships)
            {
                if (s.Bounds.Intersects(new Rectangle((int)power.Location.X - 25, (int)power.Location.Y - 25, 50, 50))
                    && !s.Dead)
                {
                    InvokePowerup(s.UI, power.Power);
                    if (powerups.Contains(power))
                        powerups.RemoveAt(I);
                }
            }
        }
        public void InvokePowerup(int ShipUI, Powerups Power)
        {
            if (Power == Powerups.Bulldozer)
                ObliteRaceGame.Ships[ShipUI].Bulldozer();
            if (Power == Powerups.Turbo)
                ObliteRaceGame.Ships[ShipUI].Turbo();
            if (Power == Powerups.HealthPack)
                ObliteRaceGame.Ships[ShipUI].HealthPack();
        }
        public void Draw(Vector2 camera)
        {
            batch.Begin();
            for (int I = 0; I < powerups.Count; I++)
            {
                Powerup power = powerups[I];
                Texture2D sprite = null;
                if (power.Power == Powerups.Turbo)
                    sprite = turbo;
                if (power.Power == Powerups.Bulldozer)
                    sprite = bulldozer;
                if (power.Power == Powerups.HealthPack)
                    sprite = healthPack;
                if (sprite != null)
                    batch.Draw(sprite, power.Location - camera, null, Color.White, power.Rotation, new Vector2(sprite.Width / 2,
                        sprite.Height / 2), 1, SpriteEffects.None, 0);
            }
            batch.End();
        }
    }
    public struct Powerup
    {
        public Vector2 Location;
        public Vector2 Heading;
        public float Rotation;
        public Powerups Power;
        public Powerup(Vector2 location, Powerups power)
        {
            Random r = new Random();
            Rotation = 0;
            Location = location;
            Heading = new Vector2((float)r.NextDouble(), (float)r.NextDouble());
            Power = power;
        }
    }
    public enum Powerups
    {
        Respawn,
        Turbo,
        Bulldozer,
        HealthPack,
        None,
    }
}
