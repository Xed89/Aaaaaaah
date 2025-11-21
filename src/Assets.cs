using Raylib_cs;

public class Assets
{
  public Texture2D textureNoise;
  public Texture2D textureSpearmanIdle;
  //public Sound soundGun;

  public Assets()
  {
    var assetsFolder = "./assets/";
    textureNoise = Raylib.LoadTexture(assetsFolder + "Noise.png");
    textureSpearmanIdle = Raylib.LoadTexture(assetsFolder + "Warrior_3/Idle.png");
    //soundGun = Raylib.LoadSound(assetsFolder + "Gun.wav");
  }
}