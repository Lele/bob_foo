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
    public class Stampa_Modelli : DrawableGameComponent
    {

        Game game;
        string cartella_nome;
        Vector3 posizione_modello;
        Model mio_modello;

        public Stampa_Modelli(Game par_game, string par_cartella_nome, Vector3 par_posizione_modello)
            : base(par_game)
        {

            game = par_game;
            cartella_nome = par_cartella_nome;
            posizione_modello = par_posizione_modello;

        }



        protected override void LoadContent()
        {
            mio_modello = game.Content.Load<Model>(cartella_nome);

            base.LoadContent();
        }



        public override void Draw(GameTime gameTime)
        {

            foreach (ModelMesh mesh in mio_modello.Meshes)
            {

                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.CreateTranslation(posizione_modello);
                    effect.View = Mie_variabili.View;
                    effect.Projection = Mie_variabili.Projection;

                }
                mesh.Draw();

            }

            base.Draw(gameTime);
        }

    }
}
