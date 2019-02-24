public abstract class BComponent
{
    public BComponent() { }

    public virtual bool OnPlace(int x, int y, int z, int rot) => true;
    public virtual bool OnBreak(int x, int y, int z) => true;
}