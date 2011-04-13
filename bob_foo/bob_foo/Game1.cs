using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace bob_foo
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Vector3 punto_inquadrato;
        Stampa_Modelli modello2;
        Input prova_input; //<-- La classe che implementa i movimenti
        Pg personaggio_giocante; //<-- Il nuovo componente Pg


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        protected override void Initialize()
        {

            punto_inquadrato = new Vector3(0, 0, 0);
            Mie_variabili.View = Matrix.CreateLookAt(new Vector3(0, 50, 0.1f), punto_inquadrato, Vector3.Up);
            Mie_variabili.Projection = Matrix.CreatePerspectiveFieldOfView(3.14f/2, 1.33f, 10f, 10000);


          //  modello2 = new Stampa_Modelli(this, "models/undead", new Vector3(-10, 0, 0));
          //  Components.Add(modello2);

            personaggio_giocante = new Pg(this, "models/bob01"); //<-- Il personaggio che muoveremo
            Components.Add(personaggio_giocante);


            prova_input = new Input(this);

            base.Initialize();
        }



        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            prova_input.movimenti(); //<-- il metodo che muove il personaggio

            base.Draw(gameTime);
        }


    }
}

