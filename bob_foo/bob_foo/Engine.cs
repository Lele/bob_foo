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
        private Wiimote balanceBoard;//old
        private WiimoteCollection balanceBoards; //multiple balanceBoards!!!
        private Texture2D background;
        public PlayScreen level;
        private SaveScore saveScore;
        private Menu menu;
        private int status;
        public float sensibility;
        private int lastScore;
        public Boolean usingBalanceBoard;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            balanceBoard = new Wiimote();
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1000;
            //graphics.ToggleFullScreen();

            Content.RootDirectory = "Content";
            usingBalanceBoard = false;
            //old
            //level = new PlayScreen(this,balanceBoard);
            level = new PlayScreen(this, balanceBoards);
            //old
            //menu = new Menu(this, balanceBoard);
            menu = new Menu(this, balanceBoards);
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
            
            this.sensibility = 0.4f;
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
                data.level[0].PlayerName[0] = "ALEX";

                data.level[0].Score[0] = 180000;

                data.level[0].PlayerName[1] = "PAOLA";

                data.level[0].Score[1] = 180000;

                data.level[0].PlayerName[2] = "LELE";

                data.level[0].Score[2] = 180000;

                data.level[0].PlayerName[3] = "TEO";

                data.level[0].Score[3] = 180000;

                data.level[0].PlayerName[4] = "FRANCO";

                data.level[0].Score[4] = 180000;

                data.level[0].PlayerName[5] = "GIULIANO";

                data.level[0].Score[5] = 180000;

                data.level[0].PlayerName[6] = "TOMMASO";

                data.level[0].Score[6] = 180000;

                data.level[0].PlayerName[7] = "NICO";

                data.level[0].Score[7] = 180000;

                data.level[0].PlayerName[8] = "MANUEL";

                data.level[0].Score[8] = 180000;

                data.level[0].PlayerName[9] = "ANDREA";

                data.level[0].Score[9] = 180000;

                data.level[1] = data.level[2] = data.level[0];

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
            GraphicsDevice.Clear(Color.Black);
            
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
        public int GetLevel()
        {
            return level.currLevel;
        }
    }
}
