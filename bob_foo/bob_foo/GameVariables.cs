using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace bob_foo
{
    public struct GameVariables
    {
        public static Matrix View;
        public static Matrix Projection;
        public static Vector3 position;
        public static float rotationZ;
    }
}