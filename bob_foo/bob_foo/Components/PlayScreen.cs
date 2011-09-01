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
        private Boolean prevStateEnterKey;

        //numero del livello
        public int currLevel;

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
        private Boolean gameOver;
        private Boolean successful;
        private Boolean reverse;

        //variabile che si occupa del timer
        private timeSprite countDown;

        //font per il timeSprite
        SpriteFont timeFont;

        //oggetto balance board
        Wiimote bb;

        //collezione di balanceBoards
        WiimoteCollection balanceBoards;

        //balance board sensibility

        //da settare a true se si usano più balanceboard
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
        Vector3 EndPoint;
        Plane finishPlane;
        Vector3[] bobVertices;
        int[] bobIndices;

        //punteggio di gioco, cioè tempo di percorrenza tracciato
        private timeSprite gameScore;
        //timer che conta il tempo in cui il bob è ribaltato
        private timeSprite reverseTimer;
        //Entity model del bob
        private EntityModel bobEntity;
        //timer per la pressione dei tasti
        private int keyTimer;
        //component per il menu di pausa e gameover
        private gameMenu gamemenu;

        
        #endregion

        //overloading, metodo da usare con più balanceboards
        public PlayScreen(Game game, WiimoteCollection balanceBoards)
            : base(game)
        {
            currLevel = 0;
            usingBalanceBoards = true;
            this.Enabled = false;
            this.Visible = false;
            stage = new Model[3];
            this.balanceBoards = balanceBoards;
        }

        public override void Initialize()
        {

            start = true;

            pause = false;

            gameOver = false;

            successful = false;

            reverse = false;

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

            //bob con bobbisti
            bobMod = Game.Content.Load<Model>("models/bob_con_bobmen_4.1");
            bobMod.Root.Transform = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, MathHelper.PiOver2, 0) * Matrix.CreateTranslation(0, -3.1f, 3f) * bobMod.Root.Transform;
            

            //calcolo vertici che mi serviranno per la bounding box
            TriangleMesh.GetVerticesAndIndicesFromModel(bobMod, out bobVertices, out bobIndices);

            //load background music
            song = Game.Content.Load<SoundEffect>("audio/SummerSong");
            
            //load sound effects
            engine = Game.Content.Load<SoundEffect>("audio/engine_2");
            
            hyperspace = Game.Content.Load<SoundEffect>("audio/hyperspace_activate");
            
            finalBoxMod = Game.Content.Load<Model>("models/cube");
            

            //for da mettere  posto in caso di più livelli
            for (int i = 0; i < stage.Length; i++)
            {
                //stage[i] = Game.Content.Load<Model>("models/pista07");
                stage[i] = Game.Content.Load<Model>("models/Pista0"+i);
                //stage[i] = Game.Content.Load<Model>("models/pista09.2");
                //la nuova pista va girata di 90 gradi per allinearla con il bob
                //stage[i].Root.Transform = Matrix.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0, 0) * stage[i].Root.Transform;
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
            EndPoint = stageMod.getBonePosition("Finish");
            
            Game.Components.Add(stageMod);
            stageMod.Visible = true;
            stageMod.Enabled = true;

            //stessa cosa per il bg e per il bob

            //background
            TriangleMesh.GetVerticesAndIndicesFromModel(BackGroundMod, out vertices, out indices);
            var bgMesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -100, 0)));
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
            bobBox.Position = new Vector3(0, -0.3f, 0);
            //per la pista09.2
            //bobBox.Position = new Vector3(0, 4f, 0);
            Matrix scaling = Matrix.CreateScale(0.03f, 0.03f, 0.03f);
            //scaling = scaling * Matrix.CreateRotationZ(MathHelper.Pi);
            //Matrix rotation = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2,0,0);
            bobEntity = new EntityModel(bobBox, bobMod, scaling, Game, this);
            Game.Components.Add(bobEntity);
            bobBox.Tag = bobEntity;
            space.Add(bobBox);
            bobEntity.Visible = true;
            bobEntity.Enabled = true;

            //space.Add(finalBox);

            //sposto la telecamera per avere effetto introduzione
            Camera.Position = new Vector3(400, 400, 400);

            songInstance = song.CreateInstance();
            hyperspaceInsance = hyperspace.CreateInstance();
            engineInstance = engine.CreateInstance();

            gamemenu = new gameMenu(Game, timeFont);
            Game.Components.Add(gamemenu);
        }

        void CollisionHandler(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {

        }

        public void Reset()
        {
            successful = false;
            reverse = false;
            start = true;
            pause = false;
            gameOver = false;
            Camera = null;
            Game.Components.Remove(gameScore);
            Game.Components.Remove(countDown);
            countDown = null;
            Game.Components.Remove(bobEntity);
            space.Remove(stageMesh);
            Game.Components.Remove(stageMod);
            space.Remove(bobBox);
            Game.Components.Remove(bgMod);
            Game.Components.Remove(gamemenu);
            songInstance.Stop();
            if (engineInstance.State == SoundState.Playing)
            {
                engineInstance.Stop();
            }
            if (hyperspaceInsance.State == SoundState.Playing)
                hyperspaceInsance.Stop();
            Initialize();
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
            
            if (!pause && !start && !gameOver)
            {
               
                //gestione pausa
                if (KeyboardState.IsKeyDown(Keys.P) && prevStatePauseKey == false)
                {
                    pause = true;
                    gamemenu.pause = true;
                    gamemenu.Visible = true;
                    if (reverseTimer != null)
                        reverseTimer.pause = true;
                    if (gameScore != null)
                        gameScore.pause = true;
                }
                //aggiorno la fisica del gioco
                space.Update();

                if (bobBox.OrientationMatrix.Down.Y > 0.70 && !reverse)
                {
                    reverse = true;
                    reverseTimer = new timeSprite(Game, 3000f, new Vector2(465, 220), timeFont, Color.Red, false, 0);
                    reverseTimer.stringToAppend = "reversed ";
                    Game.Components.Add(reverseTimer);
                }
                else if (bobBox.OrientationMatrix.Down.Y <=  0.70)
                {
                    reverse = false;
                    if (reverseTimer != null)
                    {
                        Game.Components.Remove(reverseTimer);
                    }
                }
                if (reverseTimer != null && reverseTimer.timeOver)
                {
                    Game.Components.Remove(reverseTimer);
                    reverseTimer = null;
                    gameOver = true;
                    gameScore.pause = true;
                    gamemenu.gameOver = true;
                    gamemenu.Visible = true;
                }

                if ((Game as Engine).usingBalanceBoard) //update with balance board
                {

                    float horizontaldelta = 0;
                    float verticaldelta = 0;
                    float total = 0;

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
                    
                    
                    //logica di spostamento
                    if (total > 30) //per evitare spostamenti non voluti, visto che il corpo non sarà mai perfettamente fermo
                    {
                        //la percentuale di peso che si può spostare è limitata alla metà del peso corporeo
                        float forwardThreshold = 0.5f;
                        float lateralThreshold = 0.5f;
                        float forwardMovement = (verticaldelta / total)/(Game as Engine).sensibility;
                        float lateralMovement = (0.05f*horizontaldelta / total)/(Game as Engine).sensibility; //0.2 è per limitare il movimento laterale

                        //gestione dei trashold per evitare che ci siano movimenti esagerati
                        if (verticaldelta / total > forwardThreshold)
                            forwardMovement = forwardThreshold / (Game as Engine).sensibility;
                        if (verticaldelta / total < -forwardThreshold)
                            forwardMovement = -forwardThreshold / (Game as Engine).sensibility;
                        if (horizontaldelta / total > lateralThreshold)
                            lateralMovement = 0.5f * lateralThreshold / (Game as Engine).sensibility;
                        if (horizontaldelta / total < -lateralThreshold)
                            lateralMovement = -0.5f * lateralThreshold / (Game as Engine).sensibility;
                        
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
                            bobBox.AngularVelocity += (0.5f * Vector3.One - bobBox.AngularVelocity / 60) / 7;
                        }

                        if (lateralMovement > 0.2)
                        {
                            Vector3 right = bobBox.OrientationMatrix.Right;
                            bobBox.LinearVelocity += (lateralMovement * right - bobBox.LinearVelocity / 60) / 7;
                            bobBox.AngularVelocity -= (0.5f * Vector3.One - bobBox.LinearVelocity / 60) / 7;
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
                        //old behaviour
                        //bobBox.LinearVelocity += (0.7f * left - bobBox.LinearVelocity / 60) / 7;
                        //new mod
                        bobBox.LinearVelocity += (0.7f * left - bobBox.LinearVelocity / 60) / 7;
                        bobBox.AngularVelocity += (0.1f * Vector3.One - bobBox.AngularVelocity / 60) / 7;
                    }
                    if (KeyboardState.IsKeyDown(Keys.Right))
                    {
                        Vector3 right = bobBox.OrientationMatrix.Right;
                        //old behaviour
                        //bobBox.LinearVelocity += (0.7f * right - bobBox.LinearVelocity / 60) / 7;
                        //new mod
                        bobBox.LinearVelocity += (0.7f * right - bobBox.LinearVelocity / 60) / 7;
                        bobBox.AngularVelocity -= (0.1f * Vector3.One - bobBox.LinearVelocity / 60) / 7;
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

                    if (Vector3.Distance(EndPoint, bobBox.Position) < 5)
                    {
                        if (reverseTimer != null)
                        {
                            reverseTimer.pause = true;
                        }
                        gameOver = true;
                        gameScore.pause = true;
                        gamemenu.gameOver = true;
                        successful = true;
                        gamemenu.Visible = true;
                    }

                    if(bobBox.Position.Y<-70)
                    {
                        if (reverseTimer != null)
                        {
                            reverseTimer.pause = true;
                        }
                        gameOver = true;
                        gameScore.pause = true;
                        gamemenu.gameOver = true;
                        gamemenu.Visible = true;
                    }
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
                    countDown = new timeSprite(Game, 3000f, new Vector2(465, 220), timeFont, Color.DarkRed, false, 0);
                    Game.Components.Add(countDown);
                }
                else if (countDown.timeOver)
                {
                    songInstance.Volume = 0.2f;
                    songInstance.IsLooped = true;
                    songInstance.Play();
                    countDown.Dispose();
                    start = false;
                    gameScore = new timeSprite(Game, 0f, new Vector2(20, 20), timeFont, Color.White, true, 3);
                    Game.Components.Add(gameScore);
                }
            }

            else if (pause)
            {
                if (KeyboardState.IsKeyDown(Keys.Enter) && !prevStateEnterKey)
                {
                    if (gamemenu.selection == 0)
                    {
                        pause = false;
                        gamemenu.pause = false;
                        gamemenu.Visible = false;
                    }
                    else if (gamemenu.selection == 1)
                    {
                        gamemenu.selection = 0;
                        gamemenu.Visible = false;
                        Reset();
                        nextLevel();
                    }
                    else if (gamemenu.selection == 2)
                    {
                        goToMenu();
                    }
                }
                if (KeyboardState.IsKeyDown(Keys.Down) && keyTimer > 500)
                {
                    if (gamemenu.selection < 2)
                        gamemenu.selection++;
                    else
                        gamemenu.selection = 0;
                    keyTimer = 0;
                }
                if (KeyboardState.IsKeyDown(Keys.Up) && keyTimer > 500)
                {
                    if (gamemenu.selection > 0)
                        gamemenu.selection--;
                    else
                        gamemenu.selection = 2;
                    keyTimer = 0;
                }
                keyTimer += gameTime.ElapsedGameTime.Milliseconds;
            }

            else if (gameOver)
            {
                space.Update();
                if(KeyboardState.IsKeyDown(Keys.Enter) && !prevStateEnterKey)
                {
                    if (gamemenu.selection == 0)
                    {
                        Reset();
                        nextLevel();
                        gamemenu.Visible = false;
                        gamemenu.selection = 0;
                    }
                    else
                    {
                        if (successful)
                            goToSaveScore();
                        else
                            goToMenu();
                    }
                }
                if ((KeyboardState.IsKeyDown(Keys.Up) || KeyboardState.IsKeyDown(Keys.Down)) && keyTimer > 500)
                {
                    if (gamemenu.selection == 1)
                        gamemenu.selection = 0;
                    else
                        gamemenu.selection = 1;
                    keyTimer = 0;
                }
                keyTimer += gameTime.ElapsedGameTime.Milliseconds;
            }

            prevStatePauseKey = KeyboardState.IsKeyDown(Keys.P);
            
            base.Update(gameTime);
        }

       
        public override void Draw(GameTime gameTime)
        {
            
            base.Draw(gameTime);
            
        }

        private void goToMenu()
        {
            Reset();
            (Game as Engine).SetStatus(0);
            this.Enabled = false;
            this.Visible = false;
            gamemenu.selection = 0;
        }

        private void goToSaveScore()
        {
            Reset();
            (Game as Engine).SetScore(-(int)gameScore.getTime());
            (Game as Engine).SetStatus(2);
            this.Enabled = false;
            this.Visible = false;
            gamemenu.selection = 0;
        }

    }
}