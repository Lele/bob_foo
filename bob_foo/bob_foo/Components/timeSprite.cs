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


namespace bob_foo.Components
{

    public class timeSprite : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private string text;
        private Texture2D texture;
        private float delay;
        private SpriteFont font;
        private Vector2 position;
        private SpriteBatch spriteBatch;
        public Boolean timeOver;
        private Color color;

        public timeSprite(Game game, String text, SpriteFont font, float delay, Vector2 position, Color color)
            : base(game)
        {
            this.text = text;
            this.font = font;
            this.delay = delay;
            this.position = position;
            this.color = color;
            this.timeOver = false;
        }

        public timeSprite(Game game, float delay, Vector2 position, SpriteFont font, Color color)
            : base(game)
        {
            this.font = font;
            this.delay = delay;
            this.color = color;
            this.position = position;
            this.timeOver = false;
        }

        public timeSprite(Game game, Texture2D texture, float delay, Vector2 position)
            : base(game)
        {
            this.color = Color.White;
            this.texture = texture;
            this.delay = delay;
            this.position = position;
            this.timeOver = false;
        }

        public override void Initialize()
        {
            this.spriteBatch = (Game as Engine).spriteBatch;
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            delay -= gameTime.ElapsedGameTime.Milliseconds;
            if (delay < 0)
            {
                this.timeOver = true;
            }
            base.Update(gameTime);
        }
           public override void Draw(GameTime gameTime)
        {
            if (!this.timeOver)
            {
                spriteBatch.Begin();
                if (text != null)
                    this.spriteBatch.DrawString(font, text, position, color);
                else if (texture != null)
                    this.spriteBatch.Draw(texture, position, null, color, 0f,new Vector2(texture.Width/2,texture.Height/2),Vector2.One,SpriteEffects.None,0);
                else
                    this.spriteBatch.DrawString(font, Convert.ToString(Math.Ceiling(delay / 1000)), position, color);

                spriteBatch.End();
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                base.Draw(gameTime);

            }
        }
    }
}