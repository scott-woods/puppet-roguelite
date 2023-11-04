using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions
{
    /// <summary>
    /// wrapper class to save player actions as Types
    /// </summary>
    public class PlayerActionType
    {
        public string TypeName;

        public static PlayerActionType FromType(Type type)
        {
            return new PlayerActionType() { TypeName = type.AssemblyQualifiedName };
        }

        public Type ToType()
        {
            return Type.GetType(TypeName);
        }
    }
}
