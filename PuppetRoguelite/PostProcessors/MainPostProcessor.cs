using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.PostProcessors
{
    public class MainPostProcessor : PostProcessor
    {
        RenderTexture _mainRenderTexture;
        RenderTexture _uiRenderTexture;

        public MainPostProcessor(RenderTexture mainRenderTexture, RenderTexture uiRenderTexture) : base(0)
        {
            _mainRenderTexture = mainRenderTexture;
            _uiRenderTexture = uiRenderTexture;
        }

        public override void Process(RenderTarget2D source, RenderTarget2D destination)
        {
            Game1.GraphicsDevice.SetRenderTarget(destination);

            Graphics.Instance.Batcher.Begin(BlendState.AlphaBlend);
            Graphics.Instance.Batcher.Draw(source, Vector2.Zero, Color.White);
            Graphics.Instance.Batcher.Draw(_mainRenderTexture, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
            Graphics.Instance.Batcher.Draw(_uiRenderTexture, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
            Graphics.Instance.Batcher.End();
        }
    }
}
