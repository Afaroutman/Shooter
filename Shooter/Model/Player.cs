using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter.View;


namespace Shooter.Model
{
   public class Player
    {
        // Animation representing the player
       private Animation playerAnimation;

        public Animation PlayerAnimation
        {
            get { return playerAnimation; }
            set { playerAnimation = value; }
        }
        // Position of the Player relative to the upper left side of the screen
        public Vector2 Position;

        // State of the player
        private bool active;
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        // Amount of hit points that player has
        public int Health;

        // Get the width of the player ship
        public int Width
        {
            get { return PlayerAnimation.FrameWidth; }
        }

        // Get the height of the player ship
        public int Height
        {
            get { return PlayerAnimation.FrameHeight; }
        }


        public void Initialize(Animation animation, Vector2 position)
        {
            this.playerAnimation = animation;

            // Set the starting position of the player around the middle of thescreen and to the back
            Position = position;

            // Set the player to be active
            Active = true;

            // Set the player health
            Health = 100;
        }


        public void Update(GameTime gameTime)
        {
            playerAnimation.Position = Position;
            PlayerAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PlayerAnimation.Draw(spriteBatch);
        }
    }
}

