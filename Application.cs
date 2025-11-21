using System.Runtime.InteropServices.JavaScript;

namespace RaylibWasm;

public partial class Application
{
  private static Game? game;

  public static void Main()
  {
  }

  [JSExport]
  public static void UpdateFrame(int width, int height)
  {
    if (game == null)
    {
      game = new Game();
      game.Create(width, height);
    } 
    game.Update();
  }
}
