using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
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
  Entity player2 = null!;
  int punti = 0;
  bool isAskingConnectionName;
  string myName;
  string connectTo;
  PeerJs peerJs;
  float posUpdateTime;

  public void Create(int width, int height)
  {
    windowWidth = width;
    windowHeight = height;
    Raylib.InitWindow(windowWidth, windowHeight, "Aaaaaaah");
    Raylib.SetTargetFPS(60);
    Raylib.InitAudioDevice();

    myName = PeerJsInterop.Prompt("Come ti chiami?", "abc");
    peerJs = PeerJsInterop.Create(CreateGuidStringFromText(myName));

    assets = new Assets();

    CreateScene();
  }

  private string CreateGuidStringFromText(string text)
  {
    var textBytes = UTF8Encoding.UTF8.GetBytes(text);
    var textHash = SHA256.HashData(textBytes);
    var guid = new Guid(textHash.AsSpan().Slice(0, 16));
    return guid.ToString();
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
    Raylib.DrawText("FPS: " + fps.ToString("00.0"), 5, 5, 20, Color.White);
    Raylib.DrawText("Nome: " + myName, 5, 30, 20, Color.White);
    Raylib.DrawText("Connetti a: " + connectTo, 5, 55, 20, Color.White);
    Raylib.DrawText("Punti: " + punti.ToString(), windowWidth / 2 - 100, 5, 60, Color.White);
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

    player2 = new Entity();
    player2.Type = EntityType.Player;
    player2.Pos = new Vector3(-100, 0, z: 0);
    entities.Add(player2);
  }

  void Update(float deltaTime)
  {
    var AskConnectPressed = false;
    var MoveToRandomPoint = false;
    if (Raylib.GetTouchPointCount() > 0)
    {
      if (Raylib.GetTouchPosition(0).Length() < 100)
      {
        AskConnectPressed = true;
      }
      else
      {
        MoveToRandomPoint = true;
      }
    }

    if (Raylib.IsKeyDown(KeyboardKey.C))
    {
      AskConnectPressed = true;
    }
    if (Raylib.IsKeyDown(KeyboardKey.Space))
    {
      MoveToRandomPoint = true;
    }

    if (AskConnectPressed)
    {
      if (!isAskingConnectionName)
      {
        isAskingConnectionName = true;
        connectTo = PeerJsInterop.Prompt("A chi vuoi connetterti?", "abc");
        peerJs.ConnectTo(CreateGuidStringFromText(connectTo));
      }
    }

    if (MoveToRandomPoint)
    {
      player.Pos = new Vector3(GeneraNumeroCasuale(10, windowWidth - 20),
                         GeneraNumeroCasuale(10, windowWidth - 20),
                         z: 0);
    }

    if (peerJs.DataQueue.Count > 0)
    {
      if (peerJs.DataQueue[0].Length == 2)
      {
        player2.Pos.X = peerJs.DataQueue[0][0] * 10;
        player2.Pos.Y = peerJs.DataQueue[0][1] * 10;
      }
      peerJs.DataQueue.Clear();
    }

    posUpdateTime += deltaTime;
    if (posUpdateTime > 3)
    {
      posUpdateTime = 0;
      peerJs.SendData([(byte)(player.Pos.X / 10), (byte)(player.Pos.Y / 10)]);
    }

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