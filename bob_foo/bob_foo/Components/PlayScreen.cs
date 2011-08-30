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
using System.Threading;

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
        public Model finalBoxMod;
        //modello fisico del bob
        public Box bobBox = null;
        //box posizionata alla fine della pista per decretare la fine del livello
        public Box finalBox = null;

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

        //collezione di balanceBoards
        WiimoteCollection balanceBoards;

        //balance board sensibility
        float sensibility = 0.4f;

        //booleano da settare a 1 se si vuole usare la balance board
        Boolean usingBalanceBoard = false;

        //da settare a true se si usano pi� balanceboard
        Boolean usingBalanceBoards = false;

        //sound variables
        SoundEffect song;
        SoundEffectInstance songInstance;
        SoundEffect engine;
        SoundEffectInstance engineInstance;
        SoundEffect hyperspace;
        SoundEffectInstance hyperspaceInsance;
        float enginePitch = 0.0f;
        float engineVolume = 0.0f;
        float actualForwardVelocity = 0.0f;
        float previousForwardVelocity = 0.0f;
        Boolean hyperspaceAlreadyPlayed = false;

        //variabili per calcolare il piano alla fine della pista
        Vector3 firstPlanePoint;
        Vector3 secondPlanePoint;
        Vector3 thirdPlanePoint;
        Plane finishPlane;
        Vector3[] bobVertices;
        int[] bobIndices;
        #endregion

        public PlayScreen(Game game, Wiimote bb)
            :base(game)
        {
            //se chiamo questo metodo vuol dire che uso la bb
            usingBalanceBoard = true;
            //disabilitiamo il componente per le altre fasi di gioco. saranno settate a true alla pressione di New Game
            this.Enabled = false;
            this.Visible = false;
            //inizializzo il vettore di modelli per i livelli
            stage = new Model[1];

            this.bb = bb;
        }

        //overloading, metodo da usare con pi� balanceboards
        public PlayScreen(Game game, WiimoteCollection balanceBoards)
            : base(game)
        {
            usingBalanceBoards = true;
            this.Enabled = false;
            this.Visible = false;
            stage = new Model[1];
            this.balanceBoards = balanceBoards;
        }

        public override void Initialize()
        {
            currLevel = 0;

            start = true;

            pause = false;

            Camera = new Camera(this, new Vector3(0, 0, -1),Vector3.Zero, 0.09f);

            //Camera = new Camera(this, new Vector3(bobBox.Position.X, bobBox.Position.Y, bobBox.Position.Z-1),bobBox.Position, 0.09f);

            prevStatePauseKey = false;

            base.Initialize();
        }

       
        protected override void LoadContent()
        {
            //carico tutto il materiale per il gioco
            timeFont = Game.Content.Load<SpriteFont>("menuFont");

            BackGroundMod = Game.Content.Load<Model>("models/playground");

            //old bob
            bobMod = Game.Content.Load<Model>("models/bob09");
            //bobMod.Root.Transform = Matrix.CreateFromYawPitchRoll(0, MathHelper.PiOver2, 0) * bobMod.Root.Transform;
            
            //bob con pupazzetti
            //bobMod = Game.Content.Load<Model>("models/bobbista_scheletro_collo_SKIFO");

            //calcolo vertici che mi serviranno per la bounding box
            TriangleMesh.GetVerticesAndIndicesFromModel(bobMod, out bobVertices, out bobIndices);

            //load background music
            song = Game.Content.Load<SoundEffect>("audio/SummerSong");
            songInstance = song.CreateInstance();

            //load sound effects
            engine = Game.Content.Load<SoundEffect>("audio/engine_2");
            engineInstance = engine.CreateInstance();
            hyperspace = Game.Content.Load<SoundEffect>("audio/hyperspace_activate");
            hyperspaceInsance = hyperspace.CreateInstance();

            finalBoxMod = Game.Content.Load<Model>("models/cube");
            

            //for da mettere  posto in caso di pi� livelli
            for (int i = 0; i < stage.Length; i++)
            {
                //stage[i] = Game.Content.Load<Model>("models/pista07");
                stage[i] = Game.Content.Load<Model>("models/pista09.1");
                //stage[i] = Game.Content.Load<Model>("models/pista09.2");
                //la nuova pista va girata di 90 gradi per allinearla con il bob
                stage[i].Root.Transform = Matrix.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0, 0) * stage[i].Root.Transform;
            }
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
            //per la vecchia pista
            //stageMesh = new StaticMesh(vertices, indices, new AffineTransform(Matrix3X3.CreateScale(6f), Vector3.Zero));
            //per la nuova pista
            stageMesh = new StaticMesh(vertices, indices, new AffineTransform(Matrix3X3.CreateScale(6f),Vector3.Zero));
            //lo aggiungo allo spazio fisico
            space.Add(stageMesh);
            //creo il modello 3d collegato al modello fisico
            stageMod = new StaticModel(stage[currLevel], stageMesh.WorldTransform.Matrix, Game, this);
            //calcolo i punti per la costruzione del piano che determina la fine del livello
            firstPlanePoint = stageMod.getBonePosition("Bone_0", Vector3.Zero);
            secondPlanePoint = stageMod.getBonePosition("Bone_2", Vector3.Zero);
            thirdPlanePoint = stageMod.getBonePosition("Bone_3", Vector3.Zero);
            //firstPlanePoint = stageMod.getBonePosition("BoneDown", Vector3.Zero);
            //secondPlanePoint = stageMod.getBonePosition("BoneLeft", Vector3.Zero);
            //thirdPlanePoint = stageMod.getBonePosition("BoneRight", Vector3.Zero);
            //finishPlane = new Plane(firstPlanePoint, secondPlanePoint, thirdPlanePoint);
            
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
            bobBox = new Box(Vector3.Zero, 0.20f, 0.13f, 0.5f, 15);
            bobBox.WorldTransform = Camera.WorldMatrix;
            //per la vecchia pista
            //bobBox.Position = new Vector3(0, 0.1f, 0);
            //per la nuova pista
            bobBox.Position = new Vector3(0, 0.25f, 0);
            //per la pista09.2
            //bobBox.Position = new Vector3(0, 4f, 0);
            Matrix scaling = Matrix.CreateScale(0.03f, 0.03f, 0.03f);
            //scaling = scaling * Matrix.CreateRotationZ(MathHelper.Pi);
            //Matrix rotation = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2,0,0);
            EntityModel model = new EntityModel(bobBox, bobMod, scaling, Game, this);
            Game.Components.Add(model);
            bobBox.Tag = model;
            space.Add(bobBox);
            model.Visible = true;
            model.Enabled = true;

            //creo una scatola e la posiziono in fondo alla pista
            finalBox = new Box(Vector3.Zero, 0.5f, 0.5f, 0.5f);
            finalBox.Position = thirdPlanePoint;
            finalBox.CollisionInformation.Events.InitialCollisionDetected += HandleStageEnd;
            EntityModel finalBoxModel = new EntityModel(finalBox, finalBoxMod, Matrix.CreateScale(0.5f,0.5f,0.5f), Game, this);
            //Game.Components.Add(finalBoxModel);
            finalBoxModel.Visible = true;
            finalBoxModel.Enabled = true;
            
            //space.Add(finalBox);

            //sposto la telecamera per avere effetto introduzione
            Camera.Position = new Vector3(500, 500, 500);
        }

        void HandleStageEnd(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
                Console.WriteLine("sei arrivato alla fine!");
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
            //100% cpu utilization with a i7-2820m, and it still not perform well
            //Thread trd = new Thread(new ThreadStart(this.boundingBoxTasks));
            //trd.IsBackground = true;
            //trd.Start();
            //boundingBoxTasks();

            //alternative solution: check if the bob collide with a fake/invisible box at the and of the track
            
            

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
            bobBox.LinearVelocity += (0.1f * dw - bobBox.LinearVelocity / 60) / 7;

            if (!pause && !start)
            {
                //gestione pausa
                if (KeyboardState.IsKeyDown(Keys.P) && prevStatePauseKey == false)
                    pause = true;

                //aggiorno la fisica del gioco
                space.Update();

                if (balanceBoards != null && balanceBoards.Count >= 1) //update with balance board
                {

                    float horizontaldelta = 0;
                    float verticaldelta = 0;
                    float total = 0;

                    if (balanceBoards.Count == 1) //one bb
                    {
                        BalanceBoardSensorsF balance = bb.WiimoteState.BalanceBoardState.SensorValuesKg;
                        horizontaldelta = (balance.BottomRight + balance.TopRight) - (balance.BottomLeft + balance.TopLeft);
                        verticaldelta = (balance.TopLeft + balance.TopRight) - (balance.BottomLeft + balance.BottomRight);
                        total = (balance.TopLeft + balance.TopRight) + (balance.BottomLeft + balance.BottomRight);
                    }

                    if (balanceBoards.Count > 1) //more bb
                    {
                        for (int i = 0; i < balanceBoards.Count; i++)
                        {
                            BalanceBoardSensorsF balance = balanceBoards[i].WiimoteState.BalanceBoardState.SensorValuesKg;
                            //sommo tutti i contributi di tutte le balanceboards
                            horizontaldelta += (balance.BottomRight + balance.TopRight) - (balance.BottomLeft + balance.TopLeft);
                            verticaldelta += (balance.TopLeft + balance.TopRight) - (balance.BottomLeft + balance.BottomRight);
                            total += (balance.TopLeft + balance.TopRight) + (balance.BottomLeft + balance.BottomRight);

                        }
                        //faccio la media
                        horizontaldelta = horizontaldelta / balanceBoards.Count;
                        verticaldelta = verticaldelta / balanceBoards.Count;
                        total = total / balanceBoards.Count;
                    }
                    
                    //logica di spostamento
                    if (total > 30) //per evitare spostamenti non voluti, visto che il corpo non sar� mai perfettamente fermo
                    {
                        //la percentuale di peso che si pu� spostare � limitata alla met� del peso corporeo
                        float forwardThreshold = 0.5f;
                        float lateralThreshold = 0.5f;
                        float forwardMovement = (verticaldelta / total)/sensibility;
                        float lateralMovement = (0.2f*horizontaldelta / total)/sensibility; //0.2 � per limitare il movimento laterale

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
                    //la stessa di prima la faccio se non sono in pausa e se non c'� il countdown alla pressione delle frecce
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
                        //old behaviour
                        //bobBox.LinearVelocity += (0.7f * left - bobBox.LinearVelocity / 60) / 7;
                        //new mod
                        bobBox.LinearVelocity += (0.7f * left - bobBox.LinearVelocity / 60) / 7;
                        //bobBox.AngularVelocity += (0.1f * Vector3.One - bobBox.AngularVelocity / 60) / 7;
                    }
                    if (KeyboardState.IsKeyDown(Keys.Right))
                    {
                        Vector3 right = bobBox.OrientationMatrix.Right;
                        //old behaviour
                        //bobBox.LinearVelocity += (0.7f * right - bobBox.LinearVelocity / 60) / 7;
                        //new mod
                        bobBox.LinearVelocity += (0.7f * right - bobBox.LinearVelocity / 60) / 7;
                        //bobBox.AngularVelocity -= (0.1f * Vector3.One - bobBox.LinearVelocity / 60) / 7;
                    }
                }

                //engineInstance.Play();
                if (engineInstance.State != SoundState.Playing)
                {
                    engineInstance.IsLooped = true;
                    engineInstance.Play();
                }

                //update delle caratteristiche sonore
                actualForwardVelocity = Vector3.Dot(bobBox.LinearVelocity, bobBox.OrientationMatrix.Forward);
                
                //enginePitch += Vector3.Dot(bobBox.LinearVelocity, bobBox.OrientationMatrix.Forward) / 1000;
                enginePitch += (actualForwardVelocity - previousForwardVelocity)/10;
                engineVolume += (actualForwardVelocity - previousForwardVelocity)/10;

                previousForwardVelocity = actualForwardVelocity;
                
                //engineVolume = Vector3.Dot(bobBox.LinearVelocity, bobBox.OrientationMatrix.Forward) / 30;
                //engine pitch must be bounded from -1.0f to 1.0f and volume from 0.0f to 1.0f;
                if (engineInstance.State == SoundState.Playing)
                {
                    
                    if (enginePitch >= 1.0f)
                    {
                        enginePitch = 1.0f;
                        if (hyperspaceAlreadyPlayed == false)
                        {
                            hyperspaceInsance.Play();
                            hyperspaceAlreadyPlayed = true;
                        }
                    }
                    else
                    {
                        hyperspaceAlreadyPlayed = false;
                        if (enginePitch <= -1.0f)
                            enginePitch = -1.0f;
                    }
                    if (engineVolume >= 1.0f)
                    {
                        engineVolume = 1.0f;
                    }
                    else
                    {
                        if (engineVolume <= 0.0f)
                            engineVolume = 0.0f;
                    }
                    
                    engineInstance.Pitch = enginePitch;
                    engineInstance.Volume = engineVolume;
                }
                //modify engine pitch, from -1.0f to 1.0f

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
                    countDown = new timeSprite(Game, 3000f, new Vector2(465, 220), timeFont, Color.AliceBlue);
                    Game.Components.Add(countDown);
                }
                else if (countDown.timeOver)
                {
                    songInstance.Volume = 0.2f;
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

        private void boundingBoxTasks()
        {
            //controllo se sono arrivato alla fine
            //quindi calcolo la boundingbox
            //BoundingSphere bobBoundingSphere = BoundingSphere.CreateFromPoints(bobVertices);
            //ricalcolo la bounding box trasformando i vertici nella posizione attuale
            Matrix trasformazioneBob = bobMod.Root.Transform;
            /*int i;
            for (i=0; i < bobVertices.Length; i++)
            {
                bobVertices[i] = Vector3.Transform(bobVertices[i], trasformazioneBob);
            }*/
            //Vector3[] newBobVertices;
            Vector3.Transform(bobVertices, ref trasformazioneBob, bobVertices);


            BoundingBox bobBoundingBox = BoundingBox.CreateFromPoints(bobVertices);

            //la testo contro il piano
            PlaneIntersectionType intersectionType = finishPlane.Intersects(bobBoundingBox);
            if (intersectionType.GetType().Equals(PlaneIntersectionType.Intersecting))
            {
                Console.WriteLine("Sei arrivato alla fine");
            }

        }

    }
}