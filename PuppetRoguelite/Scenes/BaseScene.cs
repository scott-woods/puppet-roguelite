using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Scenes
{
    public class BaseScene : Scene, IFinalRenderDelegate
    {
        RenderLayerExcludeRenderer _gameRenderer;
        ScreenSpaceRenderer _uiRenderer;

        public BaseScene()
        {
            FinalRenderDelegate = this;

            _gameRenderer = new RenderLayerExcludeRenderer(0, 999);
            var mainRenderTarget = new RenderTexture(480, 270);
            _gameRenderer.RenderTexture = mainRenderTarget;
            AddRenderer(_gameRenderer);

            _uiRenderer = new ScreenSpaceRenderer(100, 999);
            var uiRenderTarget = new RenderTexture(960, 540);
            uiRenderTarget.ResizeBehavior = RenderTexture.RenderTextureResizeBehavior.None;
            _uiRenderer.RenderTexture = uiRenderTarget;
            AddRenderer(_uiRenderer);
        }

        #region IFinalRenderDelegate

        private Scene _scene;

        public void OnAddedToScene(Scene scene) => _scene = scene;

        public void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
        {
            _gameRenderer.OnSceneBackBufferSizeChanged(newWidth, newHeight);
            _uiRenderer.OnSceneBackBufferSizeChanged(newWidth, newHeight);
        }

        public void HandleFinalRender(RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source,
            Rectangle finalRenderDestinationRect, SamplerState samplerState)
        {
            Core.GraphicsDevice.SetRenderTarget(null);
            Core.GraphicsDevice.Clear(letterboxColor);

            Graphics.Instance.Batcher.Begin(BlendState.AlphaBlend, samplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            //draw game
            Graphics.Instance.Batcher.Draw(_gameRenderer.RenderTexture, finalRenderDestinationRect, Color.White);
            //var gameScale = new Vector2(Screen.Width / _gameRenderer.RenderTexture.RenderTarget.Width, Screen.Height / _gameRenderer.RenderTexture.RenderTarget.Height);
            //Graphics.Instance.Batcher.Draw(_gameRenderer.RenderTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, gameScale, SpriteEffects.None, 0f);

            //draw ui
            Graphics.Instance.Batcher.Draw(_uiRenderer.RenderTexture, finalRenderDestinationRect, Color.White);
            //var uiScale = new Vector2(Screen.Width / _uiRenderer.RenderTexture.RenderTarget.Width, Screen.Height / _uiRenderer.RenderTexture.RenderTarget.Height);
            //Graphics.Instance.Batcher.Draw(_uiRenderer.RenderTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);

            Graphics.Instance.Batcher.End();
        }

        #endregion
    }
}
