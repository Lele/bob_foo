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
        #region Variables
        //graphics manager ereditato da Engine..inutile sapere cos e' :-D
        GraphicsDeviceManager graphics;
        //Spazio fisico
        Space space;
        //telecamera del gioco
        public Camera Camera;
        //modelli 3d
        public Model BackGroundMod;
        public Model[] stage;
        public Model bobMod;
        //modello fisico del bob
        public Box bobBox = null;

        //stato della tastiera e del mouse
        public KeyboardState KeyboardState;
        public MouseState MouseState;
        
        //numero del livello
        private int currLevel;

        //modelli 3d del livello e del background
        private StaticModel stageMod;
        private StaticModel bgMod;

        //modelli fisici del livello e del background
        private StaticMesh stageMesh;
        private StaticMesh bgMesh;
        
        //variabili di stato del gioco
        private Boolean pause;
        private Boolean prevStatePauseKey;
        private Boolean start;

        //variabile che si occupa del timer
        private timeSprite countDown;

        //font per il timeSprite
        SpriteFont timeFont;

        //oggetto balance board
        Wiimote bb;
        //balance board sensibility
        float sensibility = 0.4f;

        //booleano da settare a 1 se si vuole usare la balance board
        Boolean usingBalanceBoard = true;

        //sound variables
        SoundEffect song;
        SoundEffectInstance songInstance;
        #endregion

        public PlayScreen(Game game, Wiimote bb)
            :base(game)
        {
            //disabilitiamo il componente per le altre fasi di gioco. saranno settate a true alla pressione di New Game
            this.Enabled = false;
            this.Visible = false;
            //inizializzo il vettore di modelli per i livelli
            stage = new Model[1];

            this.bb = bb;
        }

    
        public override void Initialize()
        {
            currLevel = 0;

            start = true;

            pause = false;

            Camera = new Camera(this, new Vector3(0, 0, -1),Vector3.Zero, 0.09f);

            prevStatePauseKey = false;

            base.Initialize();
        }

       
        protected override void LoadContent()
        {
            //carico tutto il materiale per il gioco
            timeFont = Game.Content.Load<SpriteFont>("menuFont");

            BackGroundMod = Game.Content.Load<Model>("models/playground");

            bobMod = Game.Content.Load<Model>("models/bob09");

            //load background music
            song = Game.Content.Load<SoundEffect>("audio/SummerSong");
            songInstance = song.CreateInstance();

            //for da mettere  posto in caso di più livelli
            for (int i = 0; i < stage.Length;i++ )
                stage[i] = Game.Content.Load<Model>("models/pista07");
         
            //inizializzo lo spazio fisico
            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f,0);
            space.ForceUpdater.AllowMultithreading = true;
            
            //codice per eventHandler
            //deleterBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollisio;

        }

        //metodo che inizializza il livello
        public void nextLevel()
        {
            Vector3[] vertices;
            int[] indices;

            //pista
            //Ricevo i vertici e gli indici del modello 3d
            TriangleMesh.GetVerticesAndIndicesFromModel(stage[currLevel], out vertices, out indices);
            //creo il modello fisico
            stageMesh = new StaticMesh(vertices, indices, new AffineTransform(Matrix3X3.CreateScale(2f),Vector3.Zero));
            //lo aggiungo allo spazio fisico
            space.Add(stageMesh);
            //creo il modello 3d collegato al modello fisico
            stageMod = new StaticModel(stage[currLevel], stageMesh.WorldTransform.Matrix, Game, this);
            Game.Components.Add(stageMod);
            stageMod.Visible = true;
            stageMod.Enabled = true;

            //stessa cosa per il bg e per il bob

            //background
            TriangleMesh.GetVerticesAndIndicesFromModel(BackGroundMod, out vertices, out indices);
            var bgMesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -60, 0)));
            space.Add(bgMesh);
            bgMod = new StaticModel(BackGroundMod, bgMesh.WorldTransform.Matrix, Game, this);
            Game.Components.Add(bgMod);
            bgMod.Enabled = true;
            bgMod.Visible = true;

            //Bob
            bobBox = new Box(Vector3.Zero, 0.30f, 0.12f, 0.5f, 15);
            bobBox.WorldTransform = Camera.WorldMatrix;
            bobBox.Position = new Vector3(0, 0.1f, 0);
            Matrix scaling = Matrix.CreateScale(0.03f, 0.03f, 0.03f);
            //scaling = scaling * Matrix.CreateRotationZ(MathHelper.Pi);
            EntityModel model = new EntityModel(bobBox, bobMod, scaling, Game, this);
            Game.Components.Add(model);
            bobBox.Tag = model;
            space.Add(bobBox);
            model.Visible = true;
            model.Enabled = true;

            //sposto la telecamera per avere effetto introduzione
            Camera.Position = new Vector3(500, 500, 500);
        }
        
        //metodo non utilizzato per la gestione di un evento collisione
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
            //codice per uscire dal gioco
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed
                || KeyboardState.IsKeyDown(Keys.Escape))
                Game.Exit();
            //aggiorno la telecamera
            Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            //ricavo la direzione top-down del bob e applico un'accellerazione in questa direzione per tenerlo il piu' possibile incollato alla  pista
            Vector3 dw = bobBox.OrientationMatrix.Down;
            bobBox.LinearVelocity += (0.01f * dw - bobBox.LinearVelocity / 60) / 7;

            if (!pause && !start)
            {
                //gestione pausa
                if (KeyboardState.IsKeyDown(Keys.P) && prevStatePauseKey == false)
                    pause = true;

                //aggiorno la fisica del gioco
                space.Update();

                if (usingBalanceBoard) //update with balance board
                {
                    BalanceBoardSensorsF balance = bb.WiimoteState.BalanceBoardState.SensorValuesKg;
                    float horizontaldelta = (balance.BottomRight + balance.TopRight) - (balance.BottomLeft + balance.TopLeft);
                    float verticaldelta = (balance.TopLeft + balance.TopRight) - (balance.BottomLeft + balance.BottomRight);
                    float total = (balance.TopLeft + balance.TopRight) + (balance.BottomLeft + balance.BottomRight);
                    if (total > 30) //per evitare spostamenti non voluti, visto che il corpo non sarà mai perfettamente fermo
                    {
                        //la percentuale di peso che si può spostare è limitata alla metà del peso corporeo
                        float forwardThreshold = 0.5f;
                        float lateralThreshold = 0.5f;
                        float forwardMovement = (verticaldelta / total)/sensibility;
                        float lateralMovement = (0.2f*horizontaldelta / total)/sensibility; //0.2 è per limitare il movimento laterale

                        //gestione dei trashold per evitare che ci siano movimenti esagerati
                        if (verticaldelta / total > forwardThreshold)
                            forwardMovement = forwardThreshold / sensibility;
                        if (verticaldelta / total < -forwardThreshold)
                            forwardMovement = -forwardThreshold / sensibility;
                        if (horizontaldelta / total > lateralThreshold)
                            lateralMovement = 0.5f*lateralThreshold / sensibility;
                        if (horizontaldelta / total < -lateralThreshold)
                            lateralMovement = -0.5f*lateralThreshold / sensibility;
                        
                        //effettuo lo spostamento del bob, proporzionale ai valori della bb
                        if ( forwardMovement>= 0.2)
                        {
                            Vector3 fw = bobBox.OrientationMatrix.Forward;
                            bobBox.LinearVelocity += (forwardMovement * fw + - bobBox.LinearVelocity / 60) / 7;
                        }

                        if (forwardMovement < -0.2)
                        {
                            Vector3 fw = bobBox.OrientationMatrix.Forward;
                            bobBox.LinearVelocity -= (-forwardMovement * fw - bobBox.LinearVelocity / 60) / 7;
                        }

                        if (lateralMovement < -0.2)
                        {
                            Vector3 left = bobBox.OrientationMatrix.Left;
                            bobBox.LinearVelocity += (-lateralMovement * left - bobBox.LinearVelocity / 60) / 7;
                        }

                        if (lateralMovement > 0.2)
                        {
                            Vector3 right = bobBox.OrientationMatrix.Right;
                            bobBox.LinearVelocity += (lateralMovement * right - bobBox.LinearVelocity / 60) / 7;
                        }

                    }

                }
                else //update with keyboard
                {   
                    //la stessa di prima la faccio se non sono in pausa e se non c'è il countdown alla pressione delle frecce
                    if (KeyboardState.IsKeyDown(Keys.Up))
                    {
                        Vector3 fw = bobBox.OrientationMatrix.Forward;
                        bobBox.LinearVelocity += (1.50f * fw - bobBox.LinearVelocity / 60) / 7;
                    }
                    if (KeyboardState.IsKeyDown(Keys.Down))
                    {
                        Vector3 fw = bobBox.OrientationMatrix.Forward;
                        bobBox.LinearVelocity -= (1.18f * fw - bobBox.LinearVelocity / 60) / 7;
                    }
                    if (KeyboardState.IsKeyDown(Keys.Left))
                    {
                        Vector3 left = bobBox.OrientationMatrix.Left;
                        bobBox.LinearVelocity += (0.7f * left - bobBox.LinearVelocity / 60) / 7;
                    }
                    if (KeyboardState.IsKeyDown(Keys.Right))
                    {
                        Vector3 right = bobBox.OrientationMatrix.Right;
                        bobBox.LinearVelocity += (0.7f * right - bobBox.LinearVelocity / 60) / 7;
                    }
                }
            }
            //codice per la gestione del countdown
            else if (start)
            {
                //faccio partire il brano di sottofondo
                //songInstance.Volume = 0.75f;
                //songInstance.IsLooped = false;
                //songInstance.Play();

                if (countDown == null)
                {
                    countDown = new timeSprite(Game, 10000f, new Vector2(465, 220), timeFont, Color.AliceBlue);
                    Game.Components.Add(countDown);
                }
                else if (countDown.timeOver)
                {
                    songInstance.Play();
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