using Artemis.Core;
using System.ComponentModel;

namespace Artemis.Plugins.LayerBrushes.Ambilight.PropertyGroups
{
    public class AmbilightPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription(MinInputValue = 0, InputStepSize = 1, MaxInputValue = 3, DisableKeyframes = true)]
        public IntLayerProperty Output { get; set; }
        protected override void PopulateDefaults()
        {
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }
}