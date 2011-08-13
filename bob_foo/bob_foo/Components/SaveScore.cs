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

    public class SaveScore : DrawableGameComponent
    {


        Engine game;
        SpriteBatch spriteBatch;
        String name;
        SpriteFont font;
        Texture2D background;
        float timer = 0;
        float delay = 125f;
        float pulseTimer = 0;
        float pulseDelay = 200f;
        Vector2 namePosition = new Vector2(400, 480);
        Vector2 textPosition = new Vector2(360, 350);


        String[] allowed = {"A","B","C","D","E","F","G","H","I","J","K","L",
                               "M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","Back"};




        public SaveScore(Engine game)
            : base(game)
        {
            this.game = game;
            this.Visible = false;
            this.Enabled = false;
        }


        public override void Initialize()
        {


            base.Initialize();
        }


        public override void Update(GameTime gameTime)
        {
            // aggiungere pulsante balanceBoard

            if (name != null && Keyboard.GetState().IsKeyDown(Keys.Enter)) SendScore();
            else TypeName(gameTime);
                




            base.Update(gameTime);
        }


        private void SendScore()
        {

            ScoreData.SaveHighScore(name, game.GetScore());
            this.Enabled = false;
            this.Visible = false;
            name = null;
            game.SetStatus(0);
        }

        private void TypeName(GameTime gameTime)
        {
            
            timer += (float)gameTime.ElapsedGameTime.Milliseconds;

            Keys[] letters = Keyboard.GetState().GetPressedKeys();

            //if (letters.Length == 0) timer = delay;

            if (timer >= delay)
            {
                if (letters.Length > 0)
                {
                    if (allowed.Contains<String>(letters[0].ToString()))
                        switch (letters[0].ToString())
                        {


                            case "Back": if (name.Length > 0) name = name.Substring(0, name.Length - 1); break;

                            default: if (name == null || name.Length < 10) name += letters[0].ToString(); break;

                        }

                    timer = 0f;


                }
           
            }
        
        }





        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = game.Content.Load<SpriteFont>("menuFont");
            background = game.Content.Load<Texture2D>("Resources/Elements/menu");

            base.LoadContent();
        }




        public override void Draw(GameTime gameTime)
        {

            spriteBatch.Begin();

            spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, "Your score is: " + game.GetScore() + "\n type your name.", textPosition, Color.Black);

            {
                pulseTimer += (float)gameTime.ElapsedGameTime.Milliseconds;
                if (pulseTimer < pulseDelay)
                {
                    if (name != null) spriteBatch.DrawString(font, name + "_", namePosition, Color.Black);
                    else spriteBatch.DrawString(font, "_", namePosition, Color.Black);
                }

                else if (pulseTimer < 2 * pulseDelay && pulseTimer > pulseDelay)
                {
                    if (name != null) spriteBatch.DrawString(font, name, namePosition, Color.Black);

                }

                else
                {

                    pulseTimer = 0;
                    if (name != null) spriteBatch.DrawString(font, name, namePosition, Color.Black);
                }
            }


            spriteBatch.End();







            base.Draw(gameTime);
        }
    }
}