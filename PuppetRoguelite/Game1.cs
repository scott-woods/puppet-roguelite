using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using PuppetRoguelite.Scenes;

namespace PuppetRoguelite
{
    public class Game1 : Core
    {
        protected override void Initialize()
        {
            base.Initialize();

            DebugRenderEnabled = true;
            Window.AllowUserResizing = false;

            Scene = new TestScene();
        }
    }
}