using Artemis.Core;
using System.ComponentModel;

namespace Artemis.Plugins.LayerBrushes.Ambilight.PropertyGroups
{
    public class AmbilightPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription(DisableKeyframes = true)]
        public EnumLayerProperty<OutputEnum> Output { get; set; }
        protected override void PopulateDefaults()
        {
            Output.DefaultValue = OutputEnum.Output0;
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }

    public enum OutputEnum
    {
        Output0 = 0,
        Output1 = 1,
        Output2 = 2,
        Output3 = 3,
    }
}