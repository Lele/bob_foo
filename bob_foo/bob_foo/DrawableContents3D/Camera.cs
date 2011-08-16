﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using bob_foo.Components;

namespace bob_foo.DrawableContents3D
{
    /// <summary>
    /// Basic camera class supporting mouse/keyboard/gamepad-based movement.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Gets or sets the position of the camera.
        /// </summary>
        public Vector3 Position { get; set; }
        float yaw;
        float pitch;
        /// <summary>
        /// Gets or sets the yaw rotation of the camera.
        /// </summary>
        public float Yaw
        {
            get
            {
                return yaw;
            }
            set
            {
                yaw = MathHelper.WrapAngle(value);
            }
        }
        /// <summary>
        /// Gets or sets the pitch rotation of the camera.
        /// </summary>
        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                pitch = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2);
            }
        }

        /// <summary>
        /// Gets or sets the speed at which the camera moves.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets the view matrix of the camera.
        /// </summary>
        public Matrix ViewMatrix { get; private set; }
        /// <summary>
        /// Gets or sets the projection matrix of the camera.
        /// </summary>
        public Matrix ProjectionMatrix { get; set; }

        /// <summary>
        /// Gets the world transformation of the camera.
        /// </summary>
        public Matrix WorldMatrix { get; private set; }

        /// <summary>
        /// Gets the game owning the camera.
        /// </summary>
        public PlayScreen Ps { get; private set; }

        /// <summary>
        /// Constructs a new camera.
        /// </summary>
        /// <param name="game">Game that this camera belongs to.</param>
        /// <param name="position">Initial position of the camera.</param>
        /// <param name="speed">Initial movement speed of the camera.</param>
        public Camera(PlayScreen ps, Vector3 position,Vector3 target, float speed)
        {
            Ps = ps;
            Position = position; 
            Speed = speed;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4f / 3f, .1f, 10000.0f);
            Mouse.SetPosition(200, 200);
            WorldMatrix = Matrix.CreateWorld(Vector3.Zero, target - Position, Vector3.Up);
            ViewMatrix = Matrix.CreateLookAt(Position, target, Vector3.Up);
        }

          public void Move(Vector3 bobPos)
        {
            Position += Speed*(bobPos-Position-new Vector3(0,-0.5f,-1));
            WorldMatrix = Matrix.CreateWorld(Vector3.Zero, bobPos - Position, Vector3.Up);
            ViewMatrix = Matrix.CreateLookAt(Position, bobPos, Vector3.Up);
        }

        public void Update(float dt)
        {
            Move(Ps.bobBox.Position);
        }
    }
}
