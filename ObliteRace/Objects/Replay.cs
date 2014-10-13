using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ObliteRace.Objects
{
    /// <summary>
    /// Used to capture and replay screens.
    /// </summary>
    class Replay
    {
        SpriteBatch batch;
        ResolveTexture2D pic;
        float zoom = 0;
        public Replay(SpriteBatch Batch)
        {
            batch = Batch;
        }
        public void Capture()
        {
            GraphicsDevice device = batch.GraphicsDevice;
            pic = new ResolveTexture2D(device, device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight, 1, SurfaceFormat.Color);
            device.ResolveBackBuffer(pic);
            zoom = 10;
        }
        public void Draw()
        {
            if (pic != null && zoom > 0)
            {
                batch.Begin();

                GraphicsDevice device = batch.GraphicsDevice;
                Vector2 size = new Vector2(device.PresentationParameters.BackBufferWidth / 2, 
                    device.PresentationParameters.BackBufferHeight / 2);
                Color color = new Color(255, 0, 0, (byte)(zoom * 255 / 10));
                batch.Draw(pic, size, null, color, 0, 
                    new Vector2(pic.Width / 2, pic.Height / 2), zoom, SpriteEffects.None, 0);

                batch.End();
            }
            zoom -= .1f;
        }
    }
}
