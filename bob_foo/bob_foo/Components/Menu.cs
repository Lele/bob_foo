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
        //Wiimote balanceBoard; //sufficiente quando se ne usa una sola
        WiimoteCollection balanceBoards; //collezione di balance boards
        Texture2D background;
        Texture2D backgroundScore;
        Texture2D backgroundPiste;
        Texture2D backgroundInstructions;
        Texture2D bluetoothInstruction;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        int section;
        int selection;
        int keyDelay;
        int levelScoreDisplayed;
        float delay;
        float timer;

        String[] option = { "NEW GAME", "SCORES", "SENSIBILITY", "CONTROLLERS", "EXIT" };
        String[] piste = { "EASY", "MEDIUM", "HARD" };
        SpriteFont scoreFont;
        Boolean prevButtonStatus = true;
        Boolean prevKeyStatus = true;
        Boolean choosenLevel = false;
        ScoreData.HighScoreData scores;

        //costruttore quando si usa una sola balanceboard
        //public Menu(Engine game, Wiimote balanceBoard)
        //    : base(game)
        //{
        //    this.Enabled = false;
        //    this.Visible = false;
        //    this.balanceBoard = balanceBoard;
        //    this.game = game;
        //}

        //costrutture da usare quando ci sono più balanceboards
        public Menu(Engine game, WiimoteCollection balanceBoards):base(game)
        {
            this.Enabled = false;
            this.Visible = false;
            this.balanceBoards = balanceBoards;
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
                        if (/*(balanceBoards[0].WiimoteState.ButtonState.A && !prevButtonStatus) ||*/ (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                        {
                            //x Lele
                            //quando si usano più balanceboards bisogna usare l'oggetto balanceBoards
                            //ne prendi una e ci fai quello che prima facevi con l'oggetto balanceBoard
                            
                            //new code
                            //deve essere "scommentato" per abilitare le balanceboards
                            section = 0;
                            //balanceBoards.FindAllWiimotes();
                            //for (int i=0; i < balanceBoards.Count; i++ )
                            //    balanceBoards[i].Connect();
                            
                            //old code
                            //balanceBoard.Connect();
                        }
                    } break;
                case 0:
                    {
                        MoveSelection(gameTime);
                        if (/*(balanceBoards[0].WiimoteState.ButtonState.A && !prevButtonStatus) ||*/ (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                        {
                            switch (selection)
                            {
                                case 0: section = 4; break;
                                case 1: section = 1; break;
                                case 2: section = 2; break;
                                case 3: section = 3; break;
                                case 4: game.Exit(); break;
                            }
                        }
                    } break;
                case 1:
                    {
                        if ((Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                            section = 0;
                        if ((Keyboard.GetState().IsKeyDown(Keys.Right) && !prevKeyStatus && keyDelay > 500))
                        {
                            levelScoreDisplayed++;
                            if (levelScoreDisplayed > 2)
                                levelScoreDisplayed = 0;
                            keyDelay = 0;
                        }
                        else if ((Keyboard.GetState().IsKeyDown(Keys.Left) && !prevKeyStatus && keyDelay > 500))
                        {
                            levelScoreDisplayed--;
                            if (levelScoreDisplayed < 0)
                                levelScoreDisplayed = 2;
                            keyDelay = 0;
                        }
                        else
                            keyDelay += gameTime.ElapsedGameTime.Milliseconds;
                    } break;
                case 2:
                    {
                        MoveSensSelection(gameTime);

                        if (/*(balanceBoards[0].WiimoteState.ButtonState.A && !prevButtonStatus) || */(Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                        {
                            section = 0;
                        }
                    } break;
                case 3:
                    {
                        if (/*(balanceBoards[0].WiimoteState.ButtonState.A && !prevButtonStatus) ||*/ (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus))
                        {
                            section = 0;
                            if (game.usingBalanceBoard)
                            {
                                balanceBoards.FindAllWiimotes();
                                for (int i=0; i < balanceBoards.Count; i++ )
                                    balanceBoards[i].Connect();
                            }
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Down) && keyDelay > 500)
                        {
                            if (game.usingBalanceBoard)
                                game.usingBalanceBoard = false;
                            else
                                game.usingBalanceBoard = true;
                            keyDelay = 0;
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Up) && keyDelay > 500)
                        {
                            if (game.usingBalanceBoard)
                                game.usingBalanceBoard = false;
                            else
                                game.usingBalanceBoard = true;
                            keyDelay = 0;
                        }
                        keyDelay += gameTime.ElapsedGameTime.Milliseconds;
                    } break;
                case 4:
                       if(!choosenLevel)
                               {
                                        if (Keyboard.GetState().IsKeyDown(Keys.Down) && keyDelay > 500)
                                        {
                                            if (game.level.currLevel < 2)
                                                game.level.currLevel++;
                                            else
                                                game.level.currLevel = 0;
                                            keyDelay = 0;
                                        }
                                        else if (Keyboard.GetState().IsKeyDown(Keys.Up) && keyDelay > 500)
                                        {
                                            if (game.level.currLevel > 0)
                                                game.level.currLevel--;
                                            else
                                                game.level.currLevel = 2;
                                            keyDelay = 0;
                                        }
                                        else if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                                        {
                                            game.level.currLevel = 0;
                                            section = 0;
                                        }
                                        else
                                            keyDelay += gameTime.ElapsedGameTime.Milliseconds;
                                        if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyStatus)
                                            choosenLevel= true;
                                    }
                                    else
                                    {
                                        section = 0;
                                        game.SetStatus(1);
                                        game.level.nextLevel();
                                        this.Visible = false;
                                        this.Enabled = false;
                                        choosenLevel = false;
                                    }break;
            }
            //prevButtonStatus = balanceBoards[0].WiimoteState.ButtonState.A;
            prevKeyStatus = Keyboard.GetState().IsKeyDown(Keys.Enter);
            base.Update(gameTime);
        }

        protected override void LoadContent()
        {
            background = game.Content.Load<Texture2D>("Resources/Elements/menu");
            backgroundScore = game.Content.Load<Texture2D>("Resources/Elements/menuScores");
            bluetoothInstruction = game.Content.Load<Texture2D>("Resources/bluetoothInstruction");
            backgroundInstructions = game.Content.Load<Texture2D>("Resources/Elements/menuInstructions");
            backgroundPiste = game.Content.Load<Texture2D>("Resources/Elements/menu_piste");
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
                        spriteBatch.DrawString(font, "MENU:", new Vector2(390, 250), Color.Red);
                        for (int i = 0; i < option.Length; i++)
                        {
                            if (i != selection)
                                spriteBatch.DrawString(font, option[i], new Vector2(350, 350 + (i * 60)), Color.White);
                            else
                                spriteBatch.DrawString(font, option[i], new Vector2(350, 350 + (i * 60)), Color.DarkRed);
                        }
                    } break;
                case 1:
                    {
                        spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
                        scores = ScoreData.LoadHighScores("highscore.xml");
                        spriteBatch.DrawString(font, "HIGHSCORES", new Vector2(80, 280), Color.White); 
                        spriteBatch.DrawString(font, "LEVEL:" , new Vector2(80, 330), Color.DarkRed); 
                        for (int i = 0; i < 3; i++)
                        {
                            if (i == levelScoreDisplayed)
                                spriteBatch.DrawString(font, (i + 1).ToString(), new Vector2(230 + i * 80, 330), Color.DarkRed);
                            else
                                spriteBatch.DrawString(font, (i + 1).ToString(), new Vector2(230 + i * 80, 330), Color.White);
                        }
                        for (int i = 0; i < 10; i++)
                        {
                            spriteBatch.DrawString(font2, i + 1 + "- " + scores.level[levelScoreDisplayed].PlayerName[i] + " " + Math.Floor(-scores.level[levelScoreDisplayed].Score[i] / 1000f / 60f) + "m" + Math.Floor(-scores.level[levelScoreDisplayed].Score[i] / 1000f % 60f) + "s", new Vector2(500, 240 + i * 43), Color.White);
                        }
                    } break;
                case 2:
                    {

                        spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
                        spriteBatch.DrawString(scoreFont, "SENSIBILITY:", new Vector2(360, 330), Color.White);
                       
                        spriteBatch.DrawString(scoreFont, "Sensibility ( 0.01 - 1 ) :", new Vector2(300, 400), Color.White);
                        spriteBatch.DrawString(scoreFont, ((float)Math.Round((double)(game.sensibility), 2)).ToString(), new Vector2(450, 460), Color.DarkRed);
        
                    } break;
                case 3:
                    {
                        spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
                        if (!(Game as Engine).usingBalanceBoard)
                        {
                            spriteBatch.DrawString(scoreFont, "KEYBOARD", new Vector2(320, 300), Color.DarkRed);
                            spriteBatch.DrawString(scoreFont, "BALANCE-BOARD", new Vector2(320, 360), Color.White);
                        }
                        else
                        {
                            spriteBatch.DrawString(scoreFont, "KEYBOARD", new Vector2(320, 300), Color.White);
                            spriteBatch.DrawString(scoreFont, "BALANCE-BOARD", new Vector2(320, 360), Color.DarkRed);
                        }
                    } break;
                case 4:
                    {
                        spriteBatch.Draw(backgroundPiste, new Vector2(0, 0), Color.White);
                        for (int i = 0; i < piste.Length; i++)
                        {
                            if (i != game.level.currLevel)
                                spriteBatch.DrawString(font, piste[i], new Vector2(400, 300 + (i * 100)), Color.White);
                            else
                                spriteBatch.DrawString(font, piste[i], new Vector2(400, 300 + (i * 100)), Color.DarkRed);
                        }
                    } break;
            }


            spriteBatch.End();
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
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
                if (Keyboard.GetState().IsKeyDown(Keys.Left) && game.sensibility< 1f)
                {
                    game.sensibility = (float)Math.Round((double)(game.sensibility + 0.01f), 2);
                    timer = 0f;
                }

                else if (Keyboard.GetState().IsKeyDown(Keys.Right) && game.sensibility > 0.01f)
                {
                    game.sensibility = (float)Math.Round((double)(game.sensibility - 0.01f), 2);
                    timer = 0f;
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