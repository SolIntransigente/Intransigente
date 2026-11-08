namespace Intransigente;

/// <summary>
/// When JIT is enabled, CurrentDomain.GetAssemblies() will only return active or already used assemblies
///To force it to load this one, here there's a workaround
/// </summary>
public static class AssemblyRelationCreate
{
    public static void CreateRelation(){}
}