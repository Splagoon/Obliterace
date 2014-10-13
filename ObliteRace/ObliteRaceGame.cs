using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ObliteRace.Objects;

namespace ObliteRace
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ObliteRaceGame : Microsoft.Xna.Framework.Game
    {
        static GraphicsDeviceManager graphics;
        public static GraphicsDevice Device
        {
            get { return graphics.GraphicsDevice; }
        }
        SpriteBatch spriteBatch;

        Level level;
        Radar radar;
        Replay replay;
        Texture2D title;
        SpriteFont countdownFont;
        static Timer gameTimer = new Timer(3, 6);
        bool Paused = false;
        static PowerupManager powerups;
        public static Powerup[] Powerups
        {
            get { return powerups.PowerupCollection; }
        }
        static List<Ship> ships = new List<Ship>();
        public static List<Ship> Ships
        {
            get { return ships; }
            set { ships = value; }
        }
        public static Timer GameTimer
        {
            get { return gameTimer; }
        }

        public ObliteRaceGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Content.RootDirectory = "Content";
        }
        bool titleVisible = true;

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Components.Add(new GamerServicesComponent(this));

            base.Initialize();
            Guide.NotificationPosition = NotificationPosition.BottomRight;
            graphics.ToggleFullScreen();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Sound.Initialize();
            MessageBox.Initialize(Content, spriteBatch);

            level = new Level(Content, GraphicsDevice, Window.ClientBounds, spriteBatch);

            for (int I = 0; I < 31; I++)
                ships.Add(new Ship(Content, GraphicsDevice, spriteBatch, I == 0 ? ControlType.LocalPlayer : ControlType.Computer,
                    I, level));
            
            radar = new Radar(Content, spriteBatch, level);
            replay = new Replay(spriteBatch);
            powerups = new PowerupManager(Content, spriteBatch, level);
            title = Content.Load<Texture2D>("Sprites/Title");
            countdownFont = Content.Load<SpriteFont>("Fonts/CountDown");
            Sound.PlayCue("Music");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                titleVisible = false;

            Vector2 camera = new Vector2(ships[0].Location.X - (Window.ClientBounds.Width / 2), ships[0].Location.Y -
                (Window.ClientBounds.Height / 2));

            level.Update();
            radar.Update();
            powerups.Update();
            Window.Title = gameTimer.Time.ToString();

            if (!Guide.IsVisible && !titleVisible && !gameTimer.IsFinished)
                gameTimer.Update();
            if (!Paused)
            {
                foreach (Ship ship in ships)
                    ship.Update(camera, gameTime);
            }

            if (playerDead != ships[0].Dead && ships[0].Dead)
                replay.Capture();
            playerDead = ships[0].Dead;

            base.Update(gameTime);
        }
        bool playerDead = false;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            Paused = Guide.IsVisible || titleVisible;

            Vector2 camera = new Vector2(ships[0].Location.X - (Window.ClientBounds.Width / 2), ships[0].Location.Y - 
                (Window.ClientBounds.Height / 2));

            level.Draw(camera, Window.ClientBounds);
            foreach (Ship ship in ships)
                ship.Draw(camera, gameTime);
            powerups.Draw(camera);
            ships[0].DrawHUD();
            radar.Draw();
            replay.Draw();
            if (gameTimer.IsFinished)
            {
                Paused = true;
                if (!Guide.IsVisible)
                    ShowResults();
            }
                            spriteBatch.Begin();
            if (titleVisible)
            {
                Paused = true;

                Vector2 size = new Vector2(MathHelper.Min(Window.ClientBounds.Width, title.Width),
                    MathHelper.Min(Window.ClientBounds.Height, title.Height));
                spriteBatch.Draw(title, new Rectangle(Window.ClientBounds.Width / 2 - (int)size.X / 2,
                    Window.ClientBounds.Height / 2 - (int)size.Y / 2, (int)size.X, (int)size.Y), Color.White);
            }
            else
            {
                if (gameTimer.Time.Ticks > TimeSpan.FromMinutes(3).Ticks)
                {
                    Paused = true;
                    string time = gameTimer.Time.Seconds > 0 ? gameTimer.Time.Seconds.ToString() : "GO!";
                    spriteBatch.DrawString(countdownFont, time, new Vector2(Window.ClientBounds.Width / 2 -
                        countdownFont.MeasureString(time).X / 2, Window.ClientBounds.Height / 2 -
                        countdownFont.MeasureString(time).Y / 2), Color.Red);
                }
            }
                            spriteBatch.End();

            base.Draw(gameTime);
        }
        public static Ship GetWinner()
        {
            Ship winner = null; 
            foreach (Ship s in ships)
            {
                if (winner == null)
                    winner = s;
                else
                {
                    if (s.Stars > winner.Stars)
                        winner = s;
                }
            }
            return winner;
        }
        public static void ShowResults()
        {
            List<Ship> resultList = ships;
            resultList.Sort(CompareShipsByStars);
            MessageBox.DrawResults(resultList);
        }
        static int CompareShipsByStars(Ship x, Ship y)
        {
            //0 = y == x
            //1 = y > x
            //-1 = x > y
            int comparison = 0;
            if (x.Stars > y.Stars)
                comparison = -1;
            if (y.Stars > x.Stars)
                comparison = 1;
            if (y.Stars == x.Stars)
            {
                if (x.UI > y.UI)
                    comparison = -1;
                if (y.UI > x.UI)
                    comparison = 1;
            }
            return comparison;
        }
    }
}
