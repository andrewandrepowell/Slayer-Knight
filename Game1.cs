using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Utility;
using System;
#if DEBUG
using System.Runtime.InteropServices;
#endif

namespace SlayerKnight;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private KeyboardManager keyboardManager;
    private KeyboardFeature keyboardFeature;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
#if DEBUG
        AllocConsole();
#endif
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        keyboardManager = new KeyboardManager();
        keyboardFeature = new KeyboardFeature() { Activated=true };
        keyboardManager.Features.Add(keyboardFeature);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        keyboardManager.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        if (keyboardFeature.InfoQueue.Count > 0)
        {
            var keyInfo = keyboardFeature.InfoQueue.Dequeue();
            Console.WriteLine($"DEBUG: {keyInfo.Key} {keyInfo.State}");
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }

#if DEBUG
    // https://gamedev.stackexchange.com/questions/45107/input-output-console-window-in-xna#:~:text=Right%20click%20your%20game%20in%20the%20solution%20explorer,tab.%20Change%20the%20Output%20Type%20to%20Console%20Application.
    // This opens a console window in the game.
    [DllImport("kernel32")]
    static extern bool AllocConsole();
#endif
}
