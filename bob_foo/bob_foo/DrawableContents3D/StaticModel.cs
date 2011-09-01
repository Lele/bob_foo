using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using bob_foo.Components;
using System;

namespace bob_foo.DrawableContents3D
{
    /// <summary>
    /// Component that draws a model.
    /// </summary>
    public class StaticModel : DrawableGameComponent
    {
        Model model;
        /// <summary>
        /// Base transformation to apply to the model.
        /// </summary>
        public Matrix Transform;
        Matrix[] boneTransforms;
        PlayScreen ps;
        /// <summary>
        /// Creates a new StaticModel.
        /// </summary>
        /// <param name="model">Graphical representation to use for the entity.</param>
        /// <param name="transform">Base transformation to apply to the model before moving to the entity.</param>
        /// <param name="game">Game to which this component will belong.</param>
        public StaticModel(Model model, Matrix transform, Game game,PlayScreen ps)
            : base(game)
        {
            this.Enabled = false;
            this.Visible = false;
            this.model = model;
            this.Transform = transform;
            this.ps = ps;

            //Collect any bone transformations in the model itself.
            //The default cube model doesn't have any, but this allows the StaticModel to work with more complicated shapes.
            boneTransforms = new Matrix[model.Bones.Count];
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * Transform;
                    effect.View = ps.Camera.ViewMatrix;
                    effect.Projection = ps.Camera.ProjectionMatrix;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }

        public Vector3 getBonePosition(string boneName, Vector3 origin)
        {
            //matrice di trasformazione che dall'origine mi porta al centro del modello
            //partiamo dalla trasformazione della bone che stiamo cercando, quindi le altre matrici andranno moltiplicate a destra
            Matrix fromOriginToBoneTransform = Matrix.Identity;

            //ora devo trovare le matrici di trasformazione che mi permettono di passare dal centro del modello
            //alla bone voluta

            ModelBone bone = model.Bones[boneName];
            fromOriginToBoneTransform = bone.Transform * Transform;
            //costruisco la matrice, moltiplicando a destra la matrice del genitore
            //nota che all'ultimo passo la matrice di destra sarà equivalente alla matrice di trasformazione
            //che dal centro mi porta al centro del modello
            while(bone.Parent!=null)
            {
                fromOriginToBoneTransform = fromOriginToBoneTransform * bone.Parent.Transform;
                bone = bone.Parent;
            }

            //prendo il punto è lo moltiplico per la matrice di trasformazione complessiva
            Vector3 bonePosition = Vector3.Transform(origin,fromOriginToBoneTransform);
            
            return bonePosition;

        }
    }
}
