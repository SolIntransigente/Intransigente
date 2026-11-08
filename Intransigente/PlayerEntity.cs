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

        public override AssetPath PathToMesh => m_mesh;
        public override AssetPath PathToMaterialData => m_materialData;
        [ExposedField("Player material data")]
        private static AssetPath m_materialData;
        [ExposedField("Player mesh")]
        private static AssetPath m_mesh;
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
