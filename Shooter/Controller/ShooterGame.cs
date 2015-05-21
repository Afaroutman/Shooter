using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Shooter.Model;
using Shooter.View;


namespace Shooter.Controller
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShooterGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Represents the player 
        Player player;

        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        float playerMoveSpeed;

        // Image used to display the static background
        Texture2D mainBackground;
        Texture2D explosionTexture;
        List<Animation> explosions;
        Texture2D endMenu;

        SoundEffect laserSound;
        SoundEffect explosionSound;
        Song gameplayMusic;

        int score;

        SpriteFont font; 

        // Parallaxing Layers
        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;

        Texture2D enemyTexture;
        List<Enemy> enemies;

        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        Random random;

        Texture2D megaLazerTexture;
        Texture2D projectileTexture;
        List<Projectile> projectiles;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime;

        public ShooterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Initialize the player class
            player = new Player();
          
            playerMoveSpeed = 12.0f;

            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();
            // Load the parallaxing background
            bgLayer1.Initialize(Content, "Images/bgLayer1", GraphicsDevice.Viewport.Width, -1);
            bgLayer2.Initialize(Content, "Images/bgLayer2", GraphicsDevice.Viewport.Width, -2);
           
            enemyTexture = Content.Load<Texture2D>("Images/mineAnimation");

            explosionTexture = Content.Load<Texture2D>("Images/explosion");
           

            gameplayMusic = Content.Load<Song>("sounds/SkyFight");
            laserSound = Content.Load<SoundEffect>("sounds/laserFire");
            explosionSound = Content.Load<SoundEffect>("sounds/explosion");
            PlayMusic(gameplayMusic);
            font = Content.Load<SpriteFont>("Fonts/gameFont");

            projectileTexture = Content.Load<Texture2D>("Images/laser");
            endMenu = Content.Load<Texture2D>("Images/endMenu");
          

            mainBackground = Content.Load<Texture2D>("Images/mainbackground");

            enemies = new List<Enemy>();

            previousSpawnTime = TimeSpan.Zero;
            explosions = new List<Animation>();
            score = 0;
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            random = new Random();

            projectiles = new List<Projectile>();

            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);






            base.Initialize();

        }

        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Animation playerAnimation = new Animation();

            Texture2D playerTexture = Content.Load<Texture2D>("Images/Sprite");

            playerAnimation.Initialize(playerTexture, Vector2.Zero, 44, 40, 5, 90, Color.White, 1f, true);

            // Load the player resources            
            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
                + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);

            player.Initialize(playerAnimation, playerPosition);
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
        /// 
        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
            projectiles.Add(projectile);
        }
       
        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
            explosions.Add(explosion);
        }
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
                (Keyboard.GetState().IsKeyDown(Keys.P)))
            {
                this.Exit();
            }
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            bgLayer1.Update();
            bgLayer2.Update();
            UpdateEnemies(gameTime);
            UpdateCollisioin();
            UpdateProjectiles();
            UpdateExplosions(gameTime);

            UpdatePlayer(gameTime);

            base.Update(gameTime);
        }

        private void UpdateCollisioin()
        {
            Rectangle rectangle1;
            Rectangle rectangle2;

            rectangle1 = new Rectangle((int)player.Position.X,
                (int)player.Position.Y,
                player.Width,
                player.Height);
            for (int i = 0; i < enemies.Count; i++)
            {
                rectangle2 = new Rectangle((int)enemies[i].Position.X,
                (int)enemies[i].Position.Y,
                enemies[i].Width,
                enemies[i].Height);

                // Determine if the two objects collided with each
                // other
                if (rectangle1.Intersects(rectangle2))
                {
                    // Subtract the health from the player based on
                    // the enemy damage
                    player.Health -= enemies[i].Damage;

                    // Since the enemy collided with the player
                    // destroy it
                    enemies[i].Health = 0;

                    // If the player health is less than zero we died
                    if (player.Health <= 0)
                        player.Active = false;
                }

            }
            for (int i = 0; i < projectiles.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)projectiles[i].Position.X -
                    projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
                    projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2,
                    (int)enemies[j].Position.Y - enemies[j].Height / 2,
                    enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= projectiles[i].Damage;
                        projectiles[i].Active = false;
                    }
                   
                    if (player.Health >= 70)
                    {
                        projectileTexture = Content.Load<Texture2D>("Images/laser");
                        projectiles[i].Damage = 2;
                    }
                    else
                    {
                        projectileTexture = Content.Load<Texture2D>("Images/weak Lazer");
                        projectiles[i].Damage = 5;
                    }

                    if (player.Health <= 20)
                    {
                        AddExplosion(player.Position);
                        projectileTexture = Content.Load<Texture2D>("Images/LaserRage");
                        projectiles[i].Damage = 20;
                    }



                    
                }
            }
        }


        private void AddEnemey()
        {
            Animation enemyAnimation = new Animation();

            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next
                (100, GraphicsDevice.Viewport.Height - 100));

            Enemy enemy = new Enemy();

            enemy.Initialize(enemyAnimation, position);

            enemies.Add(enemy);
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                AddEnemey();
            }


            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(gameTime);
               
                if (enemies[i].Active == false)
                {
                    if (enemies[i].Health <= 0)
                    {
                        // Add an explosion
                        AddExplosion(enemies[i].Position);
                        explosionSound.Play();
                        score += enemies[i].Value;
                    }
                    enemies.RemoveAt(i);
                }
            }

        }
        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(gameTime);
                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }
        }
        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);
            player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
            player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.Left) || currentGamePadState.DPad.Left == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                player.Position.X -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentGamePadState.DPad.Right == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                player.Position.X += playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Up) || currentGamePadState.DPad.Up == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                player.Position.Y -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down) || currentGamePadState.DPad.Down == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                player.Position.Y += playerMoveSpeed;
            }
            if (gameTime.TotalGameTime - previousFireTime > fireTime)
            {
                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                if (currentKeyboardState.IsKeyDown(Keys.Space) || currentGamePadState.Buttons.A == ButtonState.Pressed)
                {
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                    laserSound.Play();
                }

                
            }
            if (player.Health >= 70)
            {
                playerMoveSpeed = 2.0f;
            }
            else
            {
                playerMoveSpeed = 8.0f;
            }

            player.Position.X = MathHelper.Clamp(player.Position.X,
                0, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y,
                0, GraphicsDevice.Viewport.Height - player.Height);
           
         
        }
        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (player.Health > 0)
            {
                // Start drawing
                spriteBatch.Begin();

                // Draw the moving background
                spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

                bgLayer1.Draw(spriteBatch);

                bgLayer2.Draw(spriteBatch);

                for (int i = 0; i < enemies.Count; i++)
                {
                    enemies[i].Draw(spriteBatch);
                }
                for (int i = 0; i < projectiles.Count; i++)
                {
                    projectiles[i].Draw(spriteBatch);
                }
                for (int i = 0; i < explosions.Count; i++)
                {
                    explosions[i].Draw(spriteBatch);
                }


                spriteBatch.DrawString(font, "score: " + score, new Vector2
                    (GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
                // Draw the player health
                spriteBatch.DrawString(font, "health: " + player.Health, new Vector2
                    (GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);

                // Draw the Player
                player.Draw(spriteBatch);

                //Stop drawing
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.Draw(endMenu, Vector2.Zero, Color.White);

                spriteBatch.DrawString(font, "score: " + score, new Vector2
                    (GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.Green);
                // Draw the player health
                
                spriteBatch.End();
                if(currentKeyboardState.IsKeyDown(Keys.Enter))
                {
                    this.Exit();
                }
            }
            base.Draw(gameTime);

           

            }



        
    }
    // protected override void EndDraw(GameTime gameTime)
 //   {
   //  spriteBatch.Begin();
          
            // Draw the moving background
         //   spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

            //spriteBatch.DrawString(font, "score: " + score, new Vector2
             //   (GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            // Draw the player health
           // spriteBatch.DrawString(font, "health: " + player.Health, new Vector2
            //    (GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);

        

    }



