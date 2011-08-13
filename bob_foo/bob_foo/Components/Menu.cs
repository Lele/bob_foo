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

namespace bob_foo.Components
{

    public class Menu : DrawableGameComponent
    {
        Engine game;
        Wiimote balanceBoard;
        Texture2D background;
        Texture2D backgroundScore;
        Texture2D backgroundInstructions;
        Texture2D bluetoothInstruction;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        int section;
        int selection;
        int sensSelection;
        float delay;
        float timer;

        String[] option = { "NEW GAME", "SCORES", "SENSIBILITY", "INSTRUCTIONS", "EXIT" };
        SpriteFont scoreFont;
        Boolean prevButtonStatus = true;
        Boolean prevKeyStatus = true;
        ScoreData.HighScoreData scores;


        public Menu(Engine game, Wiimote balanceBoard)
            : base(game)
        {
            this.Enabled = false;
            this.Visible = false;
            this.balanceBoard = balanceBoard;
            this.game = game;
        }



        public override void Initialize()
        {
            section = -1;
            timer = 0;
            delay = 100f;
            selection = 0;
            scores = ScoreData.LoadHighScores("highscore.xml");
            base.Initialize();

        }

        public override void Update(GameTime gameTime)
        {
            switch (section)
            {
                case -1:
                    {
                        if ((balanceBoard.WiimoteState.ButtonState.A && !prevButtonStatus) || (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                        {
                            section = 0;
                            //balanceBoard.Connect();
                        }
                    } break;
                case 0:
                    {
                        MoveSelection(gameTime);
                        if ((balanceBoard.WiimoteState.ButtonState.A && !prevButtonStatus) || (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                        {
                            switch (selection)
                            {
                                case 0:
                                    game.SetStatus(1);
                                    this.Visible = false;
                                    this.Enabled = false;
                                    break;
                                case 1: section = 1; break;
                                case 2: section = 2; break;
                                case 3: section = 3; break;
                                case 4: game.Exit(); break;
                            }
                        }
                    } break;
                case 1:
                    {
                        if ((balanceBoard.WiimoteState.ButtonState.A && !prevButtonStatus) || (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                            section = 0;
                    } break;
                case 2:
                    {
                        MoveSensSelection(gameTime);

                        if ((balanceBoard.WiimoteState.ButtonState.A && !prevButtonStatus) || (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                        {
                            sensSelection = 0;
                            section = 0;
                        }
                    } break;
                case 3:
                    {
                        if ((balanceBoard.WiimoteState.ButtonState.A && !prevButtonStatus) || (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                            section = 0;
                    } break;
            }
            prevButtonStatus = balanceBoard.WiimoteState.ButtonState.A;
            prevKeyStatus = Keyboard.GetState().IsKeyDown(Keys.Enter);
            base.Update(gameTime);
        }

        protected override void LoadContent()
        {
            background = game.Content.Load<Texture2D>("Resources/Elements/menu");
            backgroundScore = game.Content.Load<Texture2D>("Resources/Elements/menuScores");
            bluetoothInstruction = game.Content.Load<Texture2D>("Resources/bluetoothInstruction");
            backgroundInstructions = game.Content.Load<Texture2D>("Resources/Elements/menuInstructions");
            scoreFont = game.Content.Load<SpriteFont>("menuFont");
            font = game.Content.Load<SpriteFont>("menuFont");
            font2 = game.Content.Load<SpriteFont>("menuFont");
            spriteBatch = new SpriteBatch(game.GraphicsDevice);


            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            switch (section)
            {
                case -1:
                    {
                        spriteBatch.Draw(bluetoothInstruction, new Vector2(0, 0), Color.White);
                    } break;
                case 0:
                    {
                        spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
                        for (int i = 0; i < option.Length; i++)
                        {
                            if (i != selection)
                                spriteBatch.DrawString(font, option[i], new Vector2(400, 350 + (i * 60)), Color.Black);
                            else
                                spriteBatch.DrawString(font, option[i], new Vector2(400, 350 + (i * 60)), Color.DarkRed);
                        }
                    } break;
                case 1:
                    {
                        spriteBatch.Draw(backgroundScore, new Vector2(0, 0), Color.White);
                        scores = ScoreData.LoadHighScores("highscore.xml");
                        for (int i = 0; i < 10; i++)
                        {
                            spriteBatch.DrawString(font2, i + 1 + "- " + scores.PlayerName[i] + " " + scores.Score[i], new Vector2(400, 210 + i * 43), Color.Black);
                        }
                    } break;
                case 2:
                    {

                        spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
                        spriteBatch.DrawString(scoreFont, "SENSIBILITY:", new Vector2(380, 330), Color.Black);
                        if (sensSelection == 0)
                        {
                            spriteBatch.DrawString(scoreFont, "Sensibility X ( 1-7 ) :", new Vector2(380, 400), Color.DarkRed);
                            spriteBatch.DrawString(scoreFont, ((float)Math.Round((double)(8 - game.sensibility.X * 10), 2)).ToString(), new Vector2(380, 435), Color.DarkRed);
                            spriteBatch.DrawString(scoreFont, "Sensibility Y ( 1-5 ) :", new Vector2(380, 470), Color.Black);
                            spriteBatch.DrawString(scoreFont, ((float)Math.Round((double)(6 - game.sensibility.Y * 10), 2)).ToString(), new Vector2(380, 505), Color.Black);
                        }
                        if (sensSelection == 1)
                        {
                            spriteBatch.DrawString(scoreFont, "Sensibility X ( 1-7 ) :", new Vector2(380, 400), Color.Black);
                            spriteBatch.DrawString(scoreFont, ((float)Math.Round((double)(8 - game.sensibility.X * 10), 2)).ToString(), new Vector2(380, 435), Color.Black);
                            spriteBatch.DrawString(scoreFont, "Sensibility Y ( 1-5 ) :", new Vector2(380, 470), Color.DarkRed);
                            spriteBatch.DrawString(scoreFont, ((float)Math.Round((double)(6 - game.sensibility.Y * 10), 2)).ToString(), new Vector2(380, 505), Color.DarkRed);
                        }
                    } break;
                case 3:
                    {
                        spriteBatch.Draw(backgroundInstructions, new Vector2(0, 0), Color.White);
                    } break;
            }


            spriteBatch.End();
            base.Draw(gameTime);
        }
        private void MoveSelection(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.Milliseconds;
            if (!Keyboard.GetState().IsKeyDown(Keys.Down) && !Keyboard.GetState().IsKeyDown(Keys.Up))
                timer = 120f;
            if (timer > delay)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    selection++;
                    timer = 0f;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    selection--;
                    timer = 0f;
                }
            }
            if (selection > 4)
                selection = 0;
            if (selection < 0)
                selection = 4;

        }
        public void MoveSensSelection(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.Milliseconds;
            if (!Keyboard.GetState().IsKeyDown(Keys.Down) && !Keyboard.GetState().IsKeyDown(Keys.Up) && !Keyboard.GetState().IsKeyDown(Keys.Right) && !Keyboard.GetState().IsKeyDown(Keys.Left))
                timer = 120f;
            if (timer > delay)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    sensSelection++;
                    timer = 0f;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    sensSelection--;
                    timer = 0f;
                }
                if (sensSelection > 1)
                    sensSelection = 0;
                if (sensSelection < 0)
                    sensSelection = 1;
                if (sensSelection == 0)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Left) && game.sensibility.X < 0.7f)
                    {
                        game.sensibility.X = (float)Math.Round((double)(game.sensibility.X + 0.01f), 2);
                        timer = 0f;
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Right) && game.sensibility.X > 0.1f)
                    {
                        game.sensibility.X = (float)Math.Round((double)(game.sensibility.X - 0.01f), 2);
                        timer = 0f;
                    }
                }
                else if (sensSelection == 1)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Left) && game.sensibility.Y < 0.5f)
                    {
                        game.sensibility.Y = (float)Math.Round((double)(game.sensibility.Y + 0.01f), 2);
                        timer = 0f;
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Right) && game.sensibility.Y > 0.1f)
                    {
                        game.sensibility.Y = (float)Math.Round((double)(game.sensibility.Y - 0.01f), 2);
                        timer = 0f;
                    }
                }
            }

        }

        public void Reset()
        {
            scores = ScoreData.LoadHighScores("highscore.xml");
            section = 0;
            timer = 0;
            delay = 100f;
            selection = 0;
        }
    }
}