using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ObliteRace.Objects
{
    class Radar
    {
        List<Ship> ships = new List<Ship>();
        Powerup[] powers = new Powerup[0];
        SpriteBatch batch;
        Texture2D pixel;
        Texture2D marker;
        Rectangle bounds;
        public Radar(ContentManager Content, SpriteBatch Batch, Level level)
        {
            pixel = Content.Load<Texture2D>("Sprites/Pixel");
            batch = Batch;
            bounds = level.Bounds;
        }
        public void Update()
        {
            ships = ObliteRaceGame.Ships;
            powers = ObliteRaceGame.Powerups;
        }
        public void Draw()
        {
            batch.Begin();

            batch.Draw(pixel, new Rectangle(0, 0, 150, 150), new Color(0, 0, 0, 100));

            foreach (Powerup power in powers)
            {
                Color color = new Color(255, 255, 0, 100);
                batch.Draw(pixel, new Rectangle((int)(power.Location.X * 147 / bounds.Width), 
                    (int)(power.Location.Y * 147 / bounds.Height), 3, 3), color);
            }
            foreach (Ship s in ships)
            {
                Color color = Color.Red;
                if (s.ControlType == ControlType.LocalPlayer)
                    color = Color.Blue;
                if (s.Dead)
                    color = Color.Gray;
                color = new Color(color.R, color.G, color.B, 100);
                batch.Draw(pixel, new Rectangle((int)(s.Location.X * 147 / bounds.Width), (int)(s.Location.Y * 147 / bounds.Height),
                    3, 3), color);
            }

            batch.End();
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
            return WrapAngle(desiredAngle);
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
    }
}
