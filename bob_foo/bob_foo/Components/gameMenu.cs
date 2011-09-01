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


namespace bob_foo.Components
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class gameMenu : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public Boolean gameOver;
        public Boolean pause;
        public int selection;
        private SpriteFont timeFont;

        public gameMenu(Game game, SpriteFont timeFont)
            : base(game)
        {
            gameOver = false;
            pause = false;
            this.timeFont = timeFont;
            this.Visible = false;
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            (Game as Engine).spriteBatch.Begin();
            if (gameOver && selection == 0)
            {
                (Game as Engine).spriteBatch.DrawString(timeFont, "GAME OVER", new Vector2(400, 160), Color.White);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Play Again", new Vector2(400, 220), Color.DarkRed);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Menu", new Vector2(400, 260), Color.Red);
            }
            if (gameOver && selection == 1)
            {
                (Game as Engine).spriteBatch.DrawString(timeFont, "GAME OVER", new Vector2(400, 160), Color.White);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Play Again", new Vector2(400, 220), Color.Red);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Menu", new Vector2(400, 260), Color.DarkRed);
            }
            if (pause && selection == 0)
            {
                (Game as Engine).spriteBatch.DrawString(timeFont, "PAUSE", new Vector2(400, 160), Color.White);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Resume", new Vector2(400, 220), Color.DarkRed);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Play Again", new Vector2(400, 260), Color.Red);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Menu", new Vector2(400, 300), Color.Red);
            }
            if (pause && selection == 1)
            {
                (Game as Engine).spriteBatch.DrawString(timeFont, "PAUSE", new Vector2(400, 160), Color.White);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Resume", new Vector2(400, 220), Color.Red);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Play Again", new Vector2(400, 260), Color.DarkRed);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Menu", new Vector2(400, 300), Color.Red);
            }
            if (pause && selection == 2)
            {
                (Game as Engine).spriteBatch.DrawString(timeFont, "PAUSE", new Vector2(400, 160), Color.White);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Resume", new Vector2(400, 220), Color.Red);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Play Again", new Vector2(400, 260), Color.Red);
                (Game as Engine).spriteBatch.DrawString(timeFont, "Menu", new Vector2(400, 300), Color.DarkRed);
            }
            (Game as Engine).spriteBatch.End();
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            base.Draw(gameTime);
        }
    }
}
