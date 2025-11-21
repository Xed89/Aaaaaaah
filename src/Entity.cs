using System.Numerics;

public enum EntityType
{
  Player,
}

public class Entity
{
  public EntityType Type;
  public Vector3 Pos;
  public Vector3 Vel;
  public float Life;
  public float SpriteIdx;
}