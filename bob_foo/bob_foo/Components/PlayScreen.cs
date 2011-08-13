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
        /// <summary>
        /// World in which the simulation runs.
        /// </summary>
        Space space;
        /// <summary>
        /// Controls the viewpoint and how the user can see the world.
        /// </summary>
        public Camera Camera;
        /// <summary>
        /// Graphical model to use for the boxes in the scene.
        /// </summary>
        public Model CubeModel;
        /// <summary>
        /// Graphical model to use for the environment.
        /// </summary>
        public Model PlaygroundModel;

        public Model stage;


        public Model Marble;

        private Box toAdd = null;

#if XBOX360
        /// <summary>
        /// Contains the latest snapshot of the gamepad's input state.
        /// </summary>
        public GamePadState GamePadState;
#else
        /// <summary>
        /// Contains the latest snapshot of the keyboard's input state.
        /// </summary>
        public KeyboardState KeyboardState;
        /// <summary>
        /// Contains the latest snapshot of the mouse's input state.
        /// </summary>
        public MouseState MouseState;
#endif



        public PlayScreen(Game game)
            :base(game)
        {
           
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            //Setup the camera.
            Camera = new Camera(this, new Vector3(0, 3, 10), 5);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //This 1x1x1 cube model will represent the box entities in the space.
            CubeModel = Game.Content.Load<Model>("models/cube");

            PlaygroundModel = Game.Content.Load<Model>("models/playground");

            Marble = Game.Content.Load<Model>("models/bob02");

            stage = Game.Content.Load<Model>("models/pista03");

            //Construct a new space for the physics simulation to occur within.
            space = new Space();

            //Set the gravity of the simulation by accessing the simulation settings of the space.
            //It defaults to (0,0,0); this changes it to an 'earth like' gravity.
            //Try looking around in the space's simulationSettings to familiarize yourself with the various options.
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f,0);
            space.ForceUpdater.AllowMultithreading = true;
            //Make a box representing the ground and add it to the space.
            //The Box is an "Entity," the main simulation object type of BEPUphysics.
            //Examples of other entities include cones, spheres, cylinders, and a bunch more (a full listing is in the BEPUphysics.Entities namespace).

            //Every entity has a set of constructors.  Some half a parameter for mass, others don't.
            //Constructors that allow the user to specify a mass create 'dynamic' entiites which fall, bounce around, and generally work like expected.
            //Constructors that have no mass parameter create a create 'kinematic' entities.  These can be thought of as having infinite mass.
            //This box being added is representing the ground, so the width and length are extended and it is kinematic.
            //Box ground = new Box(Vector3.Zero, 30, 1, 30);
            //space.Add(ground);


            //Now that we have something to fall on, make a few more boxes.
            //These need to be dynamic, so give them a mass- in this case, 1 will be fine.
            space.Add(new Box(new Vector3(0, 4, 0), 1, 1, 1, 1));
            space.Add(new Box(new Vector3(0, 8, 0), 1, 1, 1, 1));
            space.Add(new Box(new Vector3(0, 12, 0), 1, 1, 1, 1));

            //Create a physical environment from a triangle mesh.
            //First, collect the the mesh data from the model using a helper function.
            //This special kind of vertex inherits from the TriangleMeshVertex and optionally includes
            //friction/bounciness data.
            //The StaticTriangleGroup requires that this special vertex type is used in lieu of a normal TriangleMeshVertex array.
            Vector3[] vertices;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(PlaygroundModel, out vertices, out indices);
            //Give the mesh information to a new StaticMesh.  
            //Give it a transformation which scoots it down below the kinematic box entity we created earlier.
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -30, 0)));

            //Add it to the space!
            space.Add(mesh);
            //Make it visible too.
            Game.Components.Add(new StaticModel(PlaygroundModel, mesh.WorldTransform.Matrix, Game, this));
            TriangleMesh.GetVerticesAndIndicesFromModel(stage, out vertices, out indices);

            mesh = new StaticMesh(vertices, indices, new AffineTransform(Vector3.Zero));
            space.Add(mesh);

            Game.Components.Add(new StaticModel(stage, mesh.WorldTransform.Matrix, Game, this));
            
            
            //Hook an event handler to an entity to handle some game logic.
            //Refer to the Entity Events documentation for more information.
            Box deleterBox = new Box(new Vector3(8, 2, 0), 3, 3, 3);
            space.Add(deleterBox);
            deleterBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
            //Go through the list of entities in the space and create a graphical representation for them.
            foreach (Entity e in space.Entities)
            {
                Box box = e as Box;
                if (box != null) //This won't create any graphics for an entity that isn't a box since the model being used is a box.
                {
                    
                    Matrix scaling = Matrix.CreateScale(box.Width, box.Height, box.Length); //Since the cube model is 1x1x1, it needs to be scaled to match the size of each individual box.
                    EntityModel model = new EntityModel(e, CubeModel, scaling, Game, this);
                    //Add the drawable game component for this entity to the game.
                    Game.Components.Add(model);
                    e.Tag = model; //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
                }
            }

        }

        /// <summary>
        /// Used to handle a collision event triggered by an entity specified above.
        /// </summary>
        /// <param name="sender">Entity that had an event hooked.</param>
        /// <param name="other">Entity causing the event to be triggered.</param>
        /// <param name="pair">Collision pair between the two objects in the event.</param>
        void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            //This type of event can occur when an entity hits any other object which can be collided with.
            //They aren't always entities; for example, hitting a StaticMesh would trigger this.
            //Entities use EntityCollidables as collision proxies; see if the thing we hit is one.
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                //We hit an entity! remove it.
                space.Remove(otherEntityInformation.Entity); 
                //Remove the graphics too.
                Game.Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
            }
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
#if XBOX360
            GamePadState = GamePad.GetState(0);
#else
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
#endif
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed
#if XBOX360
                )
#else
                || KeyboardState.IsKeyDown(Keys.Escape))
#endif
                Game.Exit();

            //Update the camera.
            Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            #region Block shooting
#if XBOX360
            if (GamePadState.Triggers.Right > 0)
            {
                //If the user is holding down the trigger, start firing some boxes.
                //First, create a new dynamic box at the camera's location.
                Box toAdd = new Box(Camera.Position, 1, 1, 1, 1);
                //Set the velocity of the new box to fly in the direction the camera is pointing.
                //Entities have a whole bunch of properties that can be read from and written to.
                //Try looking around in the entity's available properties to get an idea of what is available.
                toAdd.LinearVelocity = Camera.WorldMatrix.Forward * 10;
                //Add the new box to the simulation.
                space.Add(toAdd);
            
                //Add a graphical representation of the box to the drawable game components.
                EntityModel model = new EntityModel(toAdd, CubeModel, Matrix.Identity, this);
                Components.Add(model);
                toAdd.Tag = model;  //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
            }

#else

            //if (MouseState.LeftButton == ButtonState.Pressed)
            //{
            //    //If the user is clicking, start firing some boxes.
            //    //First, create a new dynamic box at the camera's location.
            //    Box toAdd = new Box(Camera.Position, 1, 1, 1, 1);
            //    //Set the velocity of the new box to fly in the direction the camera is pointing.
            //    //Entities have a whole bunch of properties that can be read from and written to.
            //    //Try looking around in the entity's available properties to get an idea of what is available.
            //    toAdd.LinearVelocity = Camera.WorldMatrix.Forward * 10;
            //    //Add the new box to the simulation.
            //    space.Add(toAdd);

            //    //Add a graphical representation of the box to the drawable game components.
            //    EntityModel model = new EntityModel(toAdd, CubeModel, Matrix.Identity, this);
            //    Components.Add(model);
            //    toAdd.Tag = model;  //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
            //}

            if (MouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && toAdd == null)
            {
                toAdd = new Box(Camera.Position, 0.20f, 0.12f, 0.5f, 40);
                toAdd.WorldTransform = Camera.WorldMatrix;
                space.Add(toAdd);
                Matrix scaling = Matrix.CreateScale(0.04f,0.04f,0.04f);
                scaling = scaling * Matrix.CreateRotationZ(MathHelper.Pi);
                EntityModel model = new EntityModel(toAdd, Marble, scaling, Game, this);
                Game.Components.Add(model);
                toAdd.Tag = model;
            }


            if (KeyboardState.IsKeyDown(Keys.Up))
            {
                Vector3 fw = Camera.WorldMatrix.Forward;
                fw.Normalize();
                Camera.Position += fw * 2;
            }

            else if (KeyboardState.IsKeyDown(Keys.Down))
            {
                Vector3 fw = Camera.WorldMatrix.Forward;
                fw.Normalize();
                Camera.Position -= fw * 2;
            }

            if (toAdd != null)
            {
                Vector3 fw = toAdd.OrientationMatrix.Forward;
                toAdd.LinearVelocity += (fw - toAdd.LinearVelocity/10) / 7;
            }
#endif
            #endregion

            //Steps the simulation forward one time step.
            space.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}