using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI
{
    public class CustomCanvas : UICanvas
    {
        public CustomCanvas()
        {
            Stage = new CustomStage();
        }
    }
}
