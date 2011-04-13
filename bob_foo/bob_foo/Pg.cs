using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace bob_foo
{
    public class Pg : DrawableGameComponent //<-- cambiamo il nome della classe
    {

        Game game;
        string modello_pg;
        Model mio_modello;
        //non serve più la variabile posizione modello perchè sarà nella struttura

        public Pg(Game par_game, string par_modello_pg) //<-- tolti i prametri non necessari
            : base(par_game)
        {

            game = par_game;
            modello_pg = par_modello_pg;

        }



        protected override void LoadContent()
        {
            mio_modello = game.Content.Load<Model>(modello_pg); // <--- usiamo la variabile modello_pg

            base.LoadContent();
        }



        public override void Draw(GameTime gameTime)
        {

            foreach (ModelMesh mesh in mio_modello.Meshes)
            {

                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.CreateTranslation(Mie_variabili.posizione_pg);//<--cambiamo il parametro
                    effect.View = Mie_variabili.View;
                    effect.Projection = Mie_variabili.Projection;

                }
                mesh.Draw();

            }

            base.Draw(gameTime);
        }

    }
}

