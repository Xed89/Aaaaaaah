using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace RaylibWasm;

class Game
{
  int windowWidth = 800;
  int windowHeight = 800;
  Assets assets = null!;
  List<Entity> entities = new();
  Entity player = null!;
  int punti = 0;

  public void Create(int width, int height)
  {
    windowWidth = width;
    windowHeight = height;
    Raylib.InitWindow(windowWidth, windowHeight, "Aaaaaaah");
    Raylib.SetTargetFPS(60);
    Raylib.InitAudioDevice();

    assets = new Assets();

    CreateScene();
  }

  public void Update()
  {
    // Leggi la durata del frame, in secondi
    // Normalmente questo dato, facendo 60 frame al secondo, è 1/60 di secondo, cioè 0.016666... secondi.
    var deltaTime = Raylib.GetFrameTime();

    // Aggiorna lo stato del gioco
    Update(deltaTime);

    // Disegna
    Raylib.BeginDrawing();
    Raylib.ClearBackground(new Color(18, 19, 25));
    Draw();
    // Disegna gli FPS
    float fps = 1 / Raylib.GetFrameTime();
    Raylib.DrawText("FPS: " + fps.ToString("00.0"), 5, 5, 20, Color.Black);
    Raylib.DrawText("Punti: " + punti.ToString(), windowWidth / 2 - 100, 5, 60, Color.Black);
    Raylib.EndDrawing();
  }

  void CreateScene()
  {
    player = new Entity();
    player.Type = EntityType.Player;
    player.Pos = new Vector3(GeneraNumeroCasuale(10, windowWidth - 20),
                             GeneraNumeroCasuale(10, windowWidth - 20),
                             z: 0);
    entities.Add(player);
  }

  void Update(float deltaTime)
  {
    for (var i = 0; i < entities.Count; i++)
    {
      var entity = entities[i];
      entity.SpriteIdx += deltaTime * 5;
    }
  }

  void Draw()
  {
    {
      var tex = assets.textureNoise;
      var rectSrc = new Rectangle(0, 0, tex.Width, tex.Height);
      var rectDst = new Rectangle(0, 0, windowWidth, windowHeight);
      var color = new Color(255, 255, 255, 10);
      Raylib.DrawTexturePro(tex, rectSrc, rectDst, Vector2.Zero, 0 / MathF.PI * 180, color);
    }

    entities.Sort((a, b) => -a.Pos.Z.CompareTo(b.Pos.Z));
    for (var i = 0; i < entities.Count; i++)
    {
      var entity = entities[i];

      // Determina il colore in base al tipo
      Color color;
      Texture2D tex;
      Rectangle texRect;
      float Size = 1f;
      bool fitX = true;
      float rot = 0;

      switch (entity.Type)
      {
        case EntityType.Player:
          {
            color = new Color(255, 255, 255, 255);
            tex = assets.textureSpearmanIdle;
            Size = tex.Width / 5;
            var spriteIdx = ((int)entity.SpriteIdx) % 5;
            var spriteW01 = 1f / 5f;
            texRect = new Rectangle(spriteIdx * spriteW01, 0, spriteW01, 1);
          }
          break;
        default: continue;
      }

      var rectSrc = new Rectangle(tex.Width * texRect.X, tex.Height * texRect.Y,
                                  tex.Width * texRect.Width, tex.Height * texRect.Height);

      float SizeX, SizeY;
      if (fitX)
      {
        SizeX = Size;
        SizeY = SizeX / rectSrc.Width * rectSrc.Height;
      }
      else
      {
        SizeY = Size;
        SizeX = SizeY / rectSrc.Height * rectSrc.Width;
      }

      var rectDst = new Rectangle(entity.Pos.X, entity.Pos.Y, SizeX, SizeY);
      Raylib.DrawTexturePro(tex, rectSrc, rectDst, rectDst.Size / 2, rot / MathF.PI * 180, color);
    }
  }

  static float GeneraNumeroCasuale(float min, float max)
  {
    return Lerp(Random.Shared.NextSingle(), min, max);
  }

  static float Lerp(float val, float min, float max)
  {
    return min + (max - min) * val;
  }

  static float Distance(float x1, float y1, float x2, float y2)
  {
    var dx = x1 - x2;
    var dy = y1 - y2;
    return MathF.Sqrt(dx * dx + dy * dy);
  }
}