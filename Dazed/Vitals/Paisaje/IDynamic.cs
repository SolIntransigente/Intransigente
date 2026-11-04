namespace Dazed.Vitals.Paisaje;

public interface IDynamic
{
    public Transform Transformation {get;}
    public bool Exists {get;}
    //? Could also add reference to obj's collision later when implemented

    public void Destroy();
}