namespace Dazed.Vitals.Paisaje
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PaisajeElementAttribute(string elementIdentifier) : Attribute
    {
        public readonly string ElementIdentifier = elementIdentifier;
    }
}
