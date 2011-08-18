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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using WiimoteLib;
using bob_foo.Components;
using System.Xml.Serialization;
using System.IO;
using bob_foo.DrawableContents3D;

namespace bob_foo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Engine : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        private Wiimote balanceBoard;
        private Texture2D background;
        public PlayScreen level;
        private SaveScore saveScore;
        private Menu menu;
        private int status;
        public Vector2 sensibility;
        private int lastScore;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            balanceBoard = new Wiimote();
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1000;
            Content.RootDirectory = "Content";
            level = new PlayScreen(this,balanceBoard);
            menu = new Menu(this, balanceBoard);
            saveScore = new SaveScore(this);
            this.Components.Add(menu);
            this.Components.Add(saveScore);
            this.Components.Add(level);
            status = 0;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            this.sensibility = new Vector2(0.7f, 0.5f);
            this.InitScores();
            base.Initialize();
        }

        private void InitScores()
        {

            string fullpath =  "highscore.xml";

            // Check to see if the save exists
            if (!File.Exists(fullpath))
            {
                //If the file doesn't exist, make a fake one...
                // Create the data to save
                ScoreData.HighScoreData data = new ScoreData.HighScoreData(10);
                data.PlayerName[0] = "ALEX";

                data.Score[0] = 200500;

                data.PlayerName[1] = "PAOLA";

                data.Score[1] = 187000;

                data.PlayerName[2] = "LELE";

                data.Score[2] = 113300;

                data.PlayerName[3] = "TEO";

                data.Score[3] = 95100;

                data.PlayerName[4] = "FRANCO";

                data.Score[4] = 1000;

                data.PlayerName[5] = "GIULIANO";

                data.Score[5] = 900;

                data.PlayerName[6] = "TOMMASO";

                data.Score[6] = 800;

                data.PlayerName[7] = "NICO";

                data.Score[7] = 500;

                data.PlayerName[8] = "MANUEL";

                data.Score[8] = 400;

                data.PlayerName[9] = "ANDREA";

                data.Score[9] = 100;

                ScoreData.SaveHighScores(data, "highscore.xml");
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            background = this.Content.Load<Texture2D>("Resources/background");
            //level.LoadContent();
            // TODO: use this.Content to load your game content here
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
            switch (status)
            {
                case 0:
                    {
                        menu.Enabled = true;
                        menu.Visible = true;
                    } break;


                case 1:
                    {
                        level.Enabled = true;
                        level.Visible = true;
                    } break;
                case 2:
                    {
                        saveScore.Enabled = true;
                        saveScore.Visible = true;
                    } break;



            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            base.Draw(gameTime);
        }

        public void SetStatus(int status)
        {
            this.status = status;
        }

        public void SetScore(int score)
        {
            this.lastScore = score;
        }
        public int GetScore()
        {
            return lastScore;
        }
    }
}
