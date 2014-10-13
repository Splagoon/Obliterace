using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;

namespace ObliteRace.Objects
{
    class MessageBox
    {
        static SpriteBatch batch;
        static Texture2D messageBox;
        static SpriteFont messageFont;
        static SpriteFont resultsFont;
        public static void Initialize(ContentManager Content, SpriteBatch Batch)
        {
            messageBox = Content.Load<Texture2D>("Sprites/Pixel");
            messageFont = Content.Load<SpriteFont>("Fonts/HUD");
            resultsFont = Content.Load<SpriteFont>("Fonts/Results");
            batch = Batch;
        }
        public static void Show(string Title, string Message, params string[] Buttons)
        {
            Guide.BeginShowMessageBox(PlayerIndex.One, Title, Message, Buttons, 0, MessageBoxIcon.None, null, null);
        }
        public static void DrawResults(List<Ship> results)
        {
            Rectangle box = new Rectangle(25, 25, batch.GraphicsDevice.PresentationParameters.BackBufferWidth - 50,
                batch.GraphicsDevice.PresentationParameters.BackBufferHeight - 50);
            string text = "";
            for (int I = 0; I < results.Count; I++)
                text += string.Format("|{0}{1}:  Player {2}  {3} Stars", results[I].UI == 0 ? "*" : "_", I + 1, 
                    results[I].UI + 1, results[I].Stars);
            batch.Begin();

            batch.Draw(messageBox, box, new Color(0, 0, 0, 200));
            batch.DrawString(resultsFont, "Results", new Vector2(box.X, box.Y), Color.White, 0, Vector2.Zero, 2,
                SpriteEffects.None, 0);
            float x = 0;
            float maxRowWidth = 0;
            float y = 0;
            foreach (string s in text.Split('|'))
            {
                Color color = s.StartsWith("*") ? Color.Blue : Color.White;
                string literal = s;
                if (s != "")
                    literal = s.Remove(0, 1);
                batch.DrawString(resultsFont, literal, new Vector2(100 + x, 80 + y), color);
                y += resultsFont.MeasureString(literal).Y;
                maxRowWidth = MathHelper.Max(maxRowWidth, resultsFont.MeasureString(literal).X);
                if (y > box.Height - 50 - resultsFont.MeasureString(literal).Y)
                {
                    y = 0;
                    x += box.Width / 3;
                }
            }

            batch.End();
        }
    }
}
