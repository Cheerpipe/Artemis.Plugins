﻿using Artemis.Core.DataModelExpansions;
using SkiaSharp;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.DataModelExpansion
{
    public class ChromaDataModel : DataModel
    {
        public string CurrentApplication { get; internal set; }
        public List<string> ApplicationList { get; internal set; }
        public List<int> PidList { get; internal set; }
        public SKColor[] Mousepad { get; set; }
        public SKColor[] Mouse { get; set; }
        public SKColor[] Keypad { get; set; }
        public SKColor[] Keyboard { get; set; }
        public SKColor[] Headset { get; set; }
        public SKColor[] ChromaLink { get; set; }
    }
}