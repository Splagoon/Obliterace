using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace ObliteRace.Objects
{
    public class Level
    {
        //The level to draw
        Texture2D level;
        //SpriteBatch used to render our sprites
        SpriteBatch batch;
        GraphicsDevice Device;
        //The bounds of the level.
        Rectangle bounds;
        //Color of the level.
        Color lvlColor;
        public Rectangle Bounds
        {
            get { return bounds; }
        }
        public Level(ContentManager content, GraphicsDevice device, Rectangle window, SpriteBatch Batch)
        {
            level = content.Load<Texture2D>("Sprites/BG");
            batch = Batch;
            Device = device;
            bounds = new Rectangle(0, 0, 20000, 20000);
            byte[] rgb = new byte[3];
            Random r = new Random();
            r.NextBytes(rgb);
            lvlColor = new Color(rgb[0], rgb[1], rgb[2]);
        }
        public void Update()
        {

        }
        public void Draw(Vector2 camera, Rectangle window)
        {
            DrawTiledSprite(level, -camera, new Rectangle(0, 0, window.Width * 4, window.Height * 4), lvlColor);
        }
        void DrawTiledSprite(Texture2D sprite, Vector2 offset, Rectangle tileArea, Color color)
        {
            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            Rectangle source = new Rectangle(tileArea.X - (int)offset.X, tileArea.Y - (int)offset.Y, tileArea.Width,
                tileArea.Height);
            Vector2 pos = new Vector2(tileArea.X + source.Width / 2, tileArea.Y + source.Height / 2);

            batch.Draw(sprite, pos, source, color, 0, new Vector2(tileArea.Width / 2, tileArea.Height / 2), 1.0f, 
                SpriteEffects.None, 1.0f);

            batch.End();
        }
    }
}
