using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.PostProcessors
{
    public class SimplePostProcessor : PostProcessor
    {
        RenderTexture _renderTexture;

        public SimplePostProcessor(int executionOrder, RenderTexture renderTexture) : base(executionOrder)
        {
            _renderTexture = renderTexture;
        }

        public override void Process(RenderTarget2D source, RenderTarget2D destination)
        {
            Game1.GraphicsDevice.SetRenderTarget(destination);

            Graphics.Instance.Batcher.Begin();
            Graphics.Instance.Batcher.Draw(source, Vector2.Zero, Color.White);
            Graphics.Instance.Batcher.Draw(_renderTexture, Vector2.Zero, Color.White);
            Graphics.Instance.Batcher.End();
        }
    }
}
