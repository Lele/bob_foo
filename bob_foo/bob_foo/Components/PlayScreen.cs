using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
using bob_foo.Components;
using bob_foo.DrawableContents3D;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.DataStructures;
using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;
using BEPUphysics.Entities;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;

namespace bob_foo.Components
{
    
    public class PlayScreen : Microsoft.Xna.Framework.DrawableGameComponent
    {
        GraphicsDeviceManager graphics;

        Space space;

        public Camera Camera;

        public Model BackGroundMod;
        public Model[] stage;
        public Model Marble;

        public Box bobBox = null;

        public KeyboardState KeyboardState;
        public MouseState MouseState;
        
        private int currLevel;

        private StaticModel stageMod;
        private StaticModel bgMod;

        private StaticMesh stageMesh;
        private StaticMesh bgMesh;

        private Boolean pause;
        private Boolean prevStatePauseKey;
        private Boolean start;

        private timeSprite countDown;

        SpriteFont timeFont;

        public PlayScreen(Game game)
            :base(game)
        {
            this.Enabled = false;
            this.Visible = false;
            stage = new Model[1];
        }

    
        public override void Initialize()
        {
            currLevel = 0;

            start = true;

            pause = false;

            Camera = new Camera(this, new Vector3(0, 0, -1),Vector3.Zero, 0.05f);

            prevStatePauseKey = false;

            base.Initialize();
        }

       
        protected override void LoadContent()
        {
            timeFont = Game.Content.Load<SpriteFont>("menuFont");

            BackGroundMod = Game.Content.Load<Model>("models/playground");

            Marble = Game.Content.Load<Model>("models/bob09");

            for (int i = 0; i < stage.Length;i++ )
                stage[i] = Game.Content.Load<Model>("models/pista07");
         
            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f,0);
            space.ForceUpdater.AllowMultithreading = true;
            
            //codice per eventHandler
            //deleterBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollisio;

        }

        public void nextLevel()
        {
            Vector3[] vertices;
            int[] indices;

            //pista
            TriangleMesh.GetVerticesAndIndicesFromModel(stage[currLevel], out vertices, out indices);
            stageMesh = new StaticMesh(vertices, indices, new AffineTransform(Matrix3X3.CreateScale(2f),Vector3.Zero));
            space.Add(stageMesh);
            stageMod = new StaticModel(stage[currLevel], stageMesh.WorldTransform.Matrix, Game, this,true);
            Game.Components.Add(stageMod);
            stageMod.Visible = true;
            stageMod.Enabled = true;

            //background
            TriangleMesh.GetVerticesAndIndicesFromModel(BackGroundMod, out vertices, out indices);
            var bgMesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -60, 0)));
            space.Add(bgMesh);
            bgMod = new StaticModel(BackGroundMod, bgMesh.WorldTransform.Matrix, Game, this,false);
            Game.Components.Add(bgMod);
            bgMod.Enabled = true;
            bgMod.Visible = true;

            //Bob
            bobBox = new Box(Vector3.Zero, 0.30f, 0.12f, 0.5f, 40);
            bobBox.WorldTransform = Camera.WorldMatrix;
            bobBox.Position = new Vector3(0, 0.1f, 0);
            Matrix scaling = Matrix.CreateScale(0.03f, 0.03f, 0.03f);
            //scaling = scaling * Matrix.CreateRotationZ(MathHelper.Pi);
            EntityModel model = new EntityModel(bobBox, Marble, scaling, Game, this);
            Game.Components.Add(model);
            bobBox.Tag = model;
            space.Add(bobBox);
            model.Visible = true;
            model.Enabled = true;

            Camera.Position = new Vector3(200, 200, 200);
        }
        

        void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                space.Remove(otherEntityInformation.Entity); 
                Game.Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
            }
        }

        protected override void UnloadContent()
        {
           
        }

     
        public override void Update(GameTime gameTime)
        {

            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed
                || KeyboardState.IsKeyDown(Keys.Escape))
                Game.Exit();
            Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
           
            if (!pause && !start)
            {
                space.Update();
                if (KeyboardState.IsKeyDown(Keys.P) && prevStatePauseKey == false)
                    pause = true;
                if (KeyboardState.IsKeyDown(Keys.Up))
                {
                    Vector3 fw = bobBox.OrientationMatrix.Forward;
                    bobBox.LinearVelocity += (1.16f * fw - bobBox.LinearVelocity / 60) / 7;
                }
                if (KeyboardState.IsKeyDown(Keys.Down))
                {
                    Vector3 fw = bobBox.OrientationMatrix.Forward;
                    bobBox.LinearVelocity -= (1.16f * fw - bobBox.LinearVelocity / 60) / 7;
                }
                if (KeyboardState.IsKeyDown(Keys.Left))
                {
                    Vector3 left = bobBox.OrientationMatrix.Left;
                    bobBox.LinearVelocity += (0.7f*left - bobBox.LinearVelocity / 60) / 7;
                }
                if (KeyboardState.IsKeyDown(Keys.Right))
                {
                    Vector3 right = bobBox.OrientationMatrix.Right;
                    bobBox.LinearVelocity += (0.7f*right - bobBox.LinearVelocity / 60) / 7;
                }
            }

            else if (start)
            {
                if (countDown == null)
                {
                    countDown = new timeSprite(Game, 10000f, new Vector2(465, 220), timeFont, Color.AliceBlue);
                    Game.Components.Add(countDown);
                }
                else if (countDown.timeOver)
                {
                    countDown.Dispose();
                    start = false;
                    //computeStartingSpeed() <-- da fare
                }
            }

            else if (pause)
            {
                if (KeyboardState.IsKeyDown(Keys.P) && prevStatePauseKey == false)
                    pause = false;
            }

            prevStatePauseKey = KeyboardState.IsKeyDown(Keys.P);
            
            base.Update(gameTime);
        }

       
        public override void Draw(GameTime gameTime)
        {

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

    }
}