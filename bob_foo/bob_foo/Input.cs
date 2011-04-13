using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace bob_foo
{
    class Input
    {

        KeyboardState stato_tastiera;

        public Input(Game game)
        {

        }


        public void movimenti()
        {
            stato_tastiera = Keyboard.GetState();

            //Spostamento a destra
            if (stato_tastiera.IsKeyDown(Keys.Right))
            {
                Mie_variabili.posizione_pg.X++;
            }

            //Spostamento a sinistra
            if (stato_tastiera.IsKeyDown(Keys.Left))
            {
                Mie_variabili.posizione_pg.X--;
            }

            //Spostamento in avanti
            if (stato_tastiera.IsKeyDown(Keys.Up))
            {
                Mie_variabili.posizione_pg.Z--;
            }

            //Spostamento indietro
            if (stato_tastiera.IsKeyDown(Keys.Down))
            {
                Mie_variabili.posizione_pg.Z++;
            }




        }

    }

}

