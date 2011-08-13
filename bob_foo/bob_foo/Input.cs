using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace bob_foo

{

    class Input
    {

        public float accelerationX;
        public float accelerationZ;
        static float MAX_ROT = 0.5f;
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
                if (accelerationX < 0)
                    accelerationX = 1;
                GameVariables.position.X += accelerationX;
                if (GameVariables.rotationZ > -MAX_ROT)
                    GameVariables.rotationZ -= 0.005f*accelerationX;
                if(accelerationX < 20)
                {
                    accelerationX += 20f/(20f-(float)Math.Abs(accelerationX));
                }
            }

            //Spostamento a sinistra
            if (stato_tastiera.IsKeyDown(Keys.Left))
            {
                if (accelerationX > 0)
                    accelerationX = -1;
                GameVariables.position.X += accelerationX;
                if (GameVariables.rotationZ < MAX_ROT)
                    GameVariables.rotationZ -= 0.005f * accelerationX;
                if(accelerationX > -20)
                {
                    accelerationX -= 20f / (20f - (float)Math.Abs(accelerationX));
                }
            }

            //Spostamento in avanti
            if (stato_tastiera.IsKeyDown(Keys.Up))
            {
                if (accelerationZ > 0)
                    accelerationZ = -1;
                GameVariables.position.Z += accelerationZ;
                if (accelerationZ > -20)
                {
                    accelerationZ -= 20f / (20f - (float)Math.Abs(accelerationZ));
                }
            }

            //Spostamento indietro
            if (stato_tastiera.IsKeyDown(Keys.Down))
            {
                if (accelerationZ < 0)
                    accelerationZ = 1;
                GameVariables.position.Z += accelerationZ;
                if (accelerationZ < 20)
                {
                    accelerationZ += 20f / (20f - (float)Math.Abs(accelerationZ));
                }
            }




        }

    }
}
