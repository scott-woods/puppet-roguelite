using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class InspectableModel
    {
        IEnumerator _handler;

        public InspectableModel() { }

        public InspectableModel(IEnumerator handler)
        {
            _handler = handler;
        }

        public virtual IEnumerator HandleInspection(int interactionCount)
        {
            yield return _handler;
        }
    }
}
