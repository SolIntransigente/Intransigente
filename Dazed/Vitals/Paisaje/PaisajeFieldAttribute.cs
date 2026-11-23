// namespace Dazed.Vitals.Paisaje;

// /// <summary>
// /// Similar to the editor's exposed fields, but targeting only objects present inside a paisaje (Entities and dynamic level geometry)
// /// </summary>
// [AttributeUsage(AttributeTargets.Field)]
// public class PaisajeFieldAttribute : Attribute
// {
//     public bool HasCustomName => CustomFieldName != null;
//     public readonly string? CustomFieldName;
//     public PaisajeFieldAttribute(){}

//     public PaisajeFieldAttribute(string name)
//     {
//         CustomFieldName = name;
//     }
// }