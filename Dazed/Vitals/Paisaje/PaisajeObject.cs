using System.Reflection;
using Dazed.Vitals.Rendering;

namespace Dazed.Vitals.Paisaje
{
    /// <summary>
    /// The class which every element that constitutes a paisaje derives from.
    /// </summary>
    public abstract class PaisajeObject
    {
        internal uint ObjectUniqueIdentifier { get; private set; }

        internal static PaisajeObject ConstructObject(Type type, uint id, SerializedElementData data)
        {
            var rawObj = Activator.CreateInstance(type)!;
            ((PaisajeObject)rawObj).ObjectUniqueIdentifier = id;
            if (data.Dynamic == null)
                return (PaisajeObject)rawObj;
            if (data.Geometry != null)
            {
                var value = data.Geometry.Value;
                ((IGeometry)(rawObj)).AssignData(value.MaterialDataPath, value.VertexData, value.TextureCoordinates, value.Indices);
            }
            ((PaisajeObject)rawObj).InterpretData(data.Dynamic.Value);
            ((PaisajeObject)rawObj).OnCreation();
            Phone.SubscribeToApplicationQuit(((IDynamic)rawObj).Destroy);
            return (PaisajeObject)rawObj;
        }

        protected virtual void OnCreation(){}
        public virtual void DrawUpdate(in RenderingData data){}

        private void InterpretData(SerializedDynamic data)
        {
            for (var i = 0; i < data.FieldsAndParameters.Length; i++)
            {
                if (data.FieldsAndParameters[i].StartsWith("param_"))
                    InterpretAsParameter(data.FieldsAndParameters[i], data.Values[i]);
                else
                    InterpretAsField(data.FieldsAndParameters[i], data.Values[i]);
            }
        }
        private void InterpretAsField(string dataField, object o)
        {
            FieldInfo? fieldInfo = GetType().GetField(dataField, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo == null)
            {
                if (DebugState.DebugMedium) Scriptorium.Message($"Field <{dataField}> not found, might have been deleted or renamed");
                return;
            }
            fieldInfo.SetValue(this, o);
        }

        private void InterpretAsParameter(string dataField, object o)
        {
            MethodInfo? methodInfo = GetType().GetMethod(dataField, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public);
            if (methodInfo == null)
            {
                if (DebugState.DebugMedium) Scriptorium.Warn($"There is no set method with name <{dataField}>, it might have been deleted or renamed.");
                return;
            }
            methodInfo.Invoke(this, [o]);
        }
    }
}
