using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Scenes
{
    public class BaseScene : Scene, IFinalRenderDelegate
    {
        ScreenSpaceRenderer _uiRenderer;
        ScreenSpaceRenderer _cursorRenderer;
        RenderLayerExcludeRenderer _gameRenderer;

        public override void Initialize()
        {
            base.Initialize();

            ClearColor = Color.Blue;

            //var diff = Game1.ResolutionScale - 1;
            //Camera.SetMinimumZoom(1f);
            //Camera.SetMaximumZoom(Game1.ResolutionScale + diff);
            //Camera.Zoom = 0.5f;

            _gameRenderer = new RenderLayerExcludeRenderer(0, (int)RenderLayers.ScreenSpaceRenderLayer, (int)RenderLayers.Cursor);
            var mainRenderTarget = new RenderTexture(Game1.DesignResolution.X, Game1.DesignResolution.Y);
            _gameRenderer.RenderTexture = mainRenderTarget;
            AddRenderer(_gameRenderer);

            _uiRenderer = new ScreenSpaceRenderer(1, (int)RenderLayers.ScreenSpaceRenderLayer);
            var uiRenderTarget = new RenderTexture(Game1.UIResolution.X, Game1.UIResolution.Y);
            uiRenderTarget.ResizeBehavior = RenderTexture.RenderTextureResizeBehavior.None;
            _uiRenderer.RenderTexture = uiRenderTarget;
            AddRenderer(_uiRenderer);

            _cursorRenderer = new ScreenSpaceRenderer(2, (int)RenderLayers.Cursor);
            //var cursorRenderTarget = new RenderTexture(Game1.DesignResolution.X, Game1.DesignResolution.Y);
            var cursorRenderTarget = new RenderTexture(Game1.UIResolution.X, Game1.UIResolution.Y);
            cursorRenderTarget.ResizeBehavior = RenderTexture.RenderTextureResizeBehavior.None;
            _cursorRenderer.RenderTexture = cursorRenderTarget;
            AddRenderer(_cursorRenderer);

            //_uiRenderer = new ScreenSpaceRenderer(100, (int)RenderLayers.ScreenSpaceRenderLayer);
            //_cursorRenderer = new RenderLayerRenderer(100, (int)RenderLayers.Cursor);
            //AddRenderer(_cursorRenderer);

            //AddRenderer(new RenderLayerExcludeRenderer(0, (int)RenderLayers.ScreenSpaceRenderLayer, (int)RenderLayers.Cursor));

            FinalRenderDelegate = this;
        }

        private Scene _scene;

        public void OnAddedToScene(Scene scene) => _scene = scene;

        public void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
        {
            _gameRenderer.OnSceneBackBufferSizeChanged(newWidth, newHeight);
            _uiRenderer.OnSceneBackBufferSizeChanged(newWidth, newHeight);
            _cursorRenderer.OnSceneBackBufferSizeChanged(newWidth, newHeight);
            _cursorRenderer.RenderTexture.Resize(Screen.Width, Screen.Height);
        }

        public void HandleFinalRender(RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source,
                                      Rectangle finalRenderDestinationRect, SamplerState samplerState)
        {
            Core.GraphicsDevice.SetRenderTarget(null);
            Core.GraphicsDevice.Clear(letterboxColor);

            Graphics.Instance.Batcher.Begin(BlendState.AlphaBlend, samplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            Graphics.Instance.Batcher.Draw(source, finalRenderDestinationRect, Color.White);

            //draw game
            Graphics.Instance.Batcher.Draw(_gameRenderer.RenderTexture, finalRenderDestinationRect, Color.White);

            //render ui
            //Graphics.Instance.Batcher.Draw(_uiRenderer.RenderTexture, finalRenderDestinationRect, Color.White);
            var uiRect = new Rectangle(0, 0, finalRenderDestinationRect.Width, finalRenderDestinationRect.Height);
            //var uiRect = finalRenderDestinationRect;
            Graphics.Instance.Batcher.Draw(_uiRenderer.RenderTexture, uiRect, Color.White);

            //render cursor
            Graphics.Instance.Batcher.Draw(_cursorRenderer.RenderTexture, uiRect, Color.White);

            Graphics.Instance.Batcher.End();

            //_uiRenderer.Render(_scene);
            //_cursorRenderer.Render(_scene);
        }
    }
    //public class BaseScene : Scene, IFinalRenderDelegate
    //{
    //    RenderLayerExcludeRenderer _gameRenderer;
    //    ScreenSpaceRenderer _uiRenderer;

    //    public BaseScene()
    //    {
    //        FinalRenderDelegate = this;

    //        _gameRenderer = new RenderLayerExcludeRenderer(0, (int)RenderLayers.ScreenSpaceRenderLayer);
    //        var mainRenderTarget = new RenderTexture(480, 270);
    //        _gameRenderer.RenderTexture = mainRenderTarget;
    //        AddRenderer(_gameRenderer);

    //        _uiRenderer = new ScreenSpaceRenderer(1, (int)RenderLayers.ScreenSpaceRenderLayer);
    //        var uiRenderTarget = new RenderTexture(960, 540);
    //        uiRenderTarget.ResizeBehavior = RenderTexture.RenderTextureResizeBehavior.None;
    //        _uiRenderer.RenderTexture = uiRenderTarget;
    //        AddRenderer(_uiRenderer);

    //        AddRenderer(new RenderLayerExcludeRenderer(0, (int)RenderLayers.ScreenSpaceRenderLayer));

    //        CreateEntity("test")
    //            .SetPosition(0, 0)
    //            .AddComponent(new SpriteRenderer(_uiRenderer.RenderTexture))
    //            .SetRenderLayer(int.MaxValue);

    //        var scaleX = (float)uiRenderTarget.RenderTarget.Width / mainRenderTarget.RenderTarget.Width;
    //        var scaleY = (float)uiRenderTarget.RenderTarget.Height / mainRenderTarget.RenderTarget.Height;

    //        ResolutionHelper.SetScaleFactor(new Vector2(scaleX, scaleY));
    //    }

    //    #region IFinalRenderDelegate

    //    private Scene _scene;

    //    public void OnAddedToScene(Scene scene) => _scene = scene;

    //    public void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
    //    {
    //        var screenSize = new Point(Screen.Width, Screen.Height);
    //        var uiDesignResolution = new Point(960, 540);
    //        var uiFactor = screenSize / uiDesignResolution;
    //        _uiRenderer.RenderTexture.Resize(uiDesignResolution.X * uiFactor.X, uiDesignResolution.Y * uiFactor.Y);
    //        //_gameRenderer.OnSceneBackBufferSizeChanged(newWidth, newHeight);
    //        //_uiRenderer.OnSceneBackBufferSizeChanged(newWidth, newHeight);
    //    }

    //    public void HandleFinalRender(RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source,
    //        Rectangle finalRenderDestinationRect, SamplerState samplerState)
    //    {
    //        Core.GraphicsDevice.SetRenderTarget(null);
    //        Core.GraphicsDevice.Clear(letterboxColor);

    //        Graphics.Instance.Batcher.Begin(BlendState.AlphaBlend, samplerState, DepthStencilState.None, RasterizerState.CullNone, null);

    //        //render source
    //        //Graphics.Instance.Batcher.Draw(source, finalRenderDestinationRect, Color.White);

    //        //draw game
    //        Graphics.Instance.Batcher.Draw(_gameRenderer.RenderTexture, finalRenderDestinationRect, Color.White);

    //        //calculate scaling
    //        var gameDesignResolution = new Point(480, 270);
    //        var uiDesignResolution = new Point(960, 540);
    //        Vector2 scale = (uiDesignResolution / gameDesignResolution).ToVector2();

    //        //render ui
    //        Graphics.Instance.Batcher.Draw(_uiRenderer.RenderTexture, finalRenderDestinationRect.Location.ToVector2(), null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

    //        ////draw ui
    //        //Graphics.Instance.Batcher.Draw(_uiRenderer.RenderTexture, finalRenderDestinationRect, Color.White);

    //        Graphics.Instance.Batcher.End();
    //    }

    //    #endregion
    //}
}
