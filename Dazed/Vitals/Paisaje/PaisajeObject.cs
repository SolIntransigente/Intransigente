using System.Reflection;

namespace Dazed.Vitals.Paisaje
{
    /// <summary>
    /// The class which every element that constitutes a paisaje derives from.
    /// </summary>
    public abstract class PaisajeObject
    {
        internal uint ObjectUniqueIdentifier { get; private set; }
        public abstract bool IsDynamic { get; }

        internal static PaisajeObject ConstructObject(Type type, uint id, ElementData data)
        {
            var rawObj = Activator.CreateInstance(type)!;
            ((PaisajeObject)rawObj).ObjectUniqueIdentifier = id;
            if (!((PaisajeObject)rawObj).IsDynamic)
                return (PaisajeObject)rawObj;
            ((PaisajeObject)rawObj).InterpretData(data);
            ((PaisajeObject)rawObj).OnCreation();
            Phone.SubscribeToApplicationQuit(((IDynamic)rawObj).Destroy);
            return (PaisajeObject)rawObj;
        }

        protected virtual void OnCreation(){}

        private void InterpretData(ElementData data)
        {
            for (var i = 0; i < data.Fields.Length; i++)
            {
                if (data.Fields[i].FieldName.StartsWith("param_"))
                    InterpretAsParameter(data.Fields[i].FieldName, data.Data[data.Fields[i].ObjectFieldIndex]);
                else
                    InterpretAsField(data.Fields[i].FieldName, data.Data[data.Fields[i].ObjectFieldIndex]);
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
