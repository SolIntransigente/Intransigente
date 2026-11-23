using Dazed.Vitals;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Paisaje;
using Dazed.Vitals.Paisaje.Entities;

namespace Intransigente
{
    [PaisajeElement(PlayerEntityName)]
    public class PlayerEntity : Entity
    {
        public const string PlayerEntityName = "player";

        public override AssetPath PathToMesh => Mesh;
        public override AssetPath PathToMaterialData => Material;
        [PaisajeExpose]
        private AssetPath Material;
        [PaisajeExpose]
        private AssetPath Mesh;
        public override string ObjectSharedName => PlayerEntityName;

        protected override void OnCreation()
        {
            ExposedData.RetrieveAll(GetType(), this);            
        }

        protected override void DestroyCall()
        {
            //Dispose
        }
    }
}
