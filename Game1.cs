using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Utility;
using System;
using Microsoft.Xna.Framework.Content;
using System.Collections;
using System.Collections.Generic;
using MonoGame.Extended.Timers;
#if DEBUG
using System.Runtime.InteropServices;
#endif

namespace SlayerKnight;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private TestLevelFeature testLevelFeature;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
#if DEBUG
        AllocConsole();
#endif
    }

    protected override void Initialize()
    {
        graphics.SynchronizeWithVerticalRetrace = true;
        graphics.GraphicsProfile = GraphicsProfile.HiDef;
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();
        spriteBatch = new SpriteBatch(GraphicsDevice);
        testLevelFeature = new TestLevelFeature(
            contentManager: Content,
            spriteBatch: spriteBatch,
            levelIdentifier: "testRoom",
            testComponentMaskAsset: "test/test_component_mask_asset",
            environmentVisualAsset: "test/test_environment_visual_asset",
            environmentMaskAsset: "test/test_environment_mask_asset",
            environmentGridSize: new MonoGame.Extended.Size(width: 100, height: 100),
            environmentStartColor: new Color(r: 255, g: 255, b: 0, alpha: 255),
            environmentIncludeColor: new Color(r: 255, g: 0, b: 0, alpha: 255),
            environmentExcludeColor: new Color(r: 0, g: 255, b: 0, alpha: 255));
        base.Initialize();
    }

    protected override void LoadContent()
    {
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var timeElapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        testLevelFeature.Update(timeElapsed);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        testLevelFeature.Draw();
        base.Draw(gameTime);
    }

#if DEBUG
    // https://gamedev.stackexchange.com/questions/45107/input-output-console-window-in-xna#:~:text=Right%20click%20your%20game%20in%20the%20solution%20explorer,tab.%20Change%20the%20Output%20Type%20to%20Console%20Application.
    // This opens a console window in the game.
    [DllImport("kernel32")]
    static extern bool AllocConsole();
#endif
}
