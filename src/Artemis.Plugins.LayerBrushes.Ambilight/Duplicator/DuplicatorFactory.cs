using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Usage = Vortice.Direct3D11.Usage;

namespace Artemis.Plugins.LayerBrushes.Ambilight
{
    public static class DuplicatorFactory
    {
        private const int DEFAULT_ADAPTER_ID = 0; //Should be always 0
        private static Dictionary<int, Duplicator> _duplications = new();
        private static IDXGIFactory1 _factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();

        static DuplicatorFactory()
        {
            PopulateDuplicators();
        }

        public static readonly FeatureLevel[] s_featureLevels = new[]
        {
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
        };

        public static int GetOutputsCount()
        {
            var  factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            var adapter = factory.GetAdapter1(DEFAULT_ADAPTER_ID); //Adapter should be always 0
            int i = 0;
            IDXGIOutput output;
            while (adapter.EnumOutputs(i, out output) != Vortice.DXGI.ResultCode.NotFound)
            {
                output.Release();
                ++i;
            }
            adapter.Release();
            factory.Release();
            return i;
        }
        public static Duplicator GetDuplicator(int outputId)
        {
            if (!_duplications.ContainsKey(outputId))
                return null;
            return _duplications.GetValueOrDefault(outputId);
        }

        public static bool DuplicationExists(int outputId)
        {

            if (!_duplications.ContainsKey(outputId))
                return false;

            if (_duplications[outputId].Duplication.NativePointer == IntPtr.Zero)
                return false;

            if (_duplications[outputId].Duplication.IsDisposed)
                return false;
            return true;
        }

        public static int PopulateDuplicators()
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(500)); //Wait until outputs refreshs
            var outputsCount = GetOutputsCount();
            var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            var adapter = factory.GetAdapter1(DEFAULT_ADAPTER_ID); //Adapter should be always 0
            ID3D11Device device;
            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.Debug, s_featureLevels, out device);
            //TODO: First, clear existing duplicators to free up API Calls (4 max windows globally)
            for (int i = 0; i < _duplications.Count; i++)
            {
                _duplications[i].Close();
                _duplications[i].Dispose();
            }
            
            _duplications.Clear();

            int populatedDuplicatorsCount = 0;
            Debug.WriteLine(outputsCount);
            for (int i = 0; i < outputsCount; i++)
            {
                try
                {
                    //Try create duplicator. It will fail if a display mode change takes too much time and outputs are not ready to be duplicated so we will have to retry some time but not forever
                    //In my case, when go from Secondary to Extended, my GSync monitor take sarround 5/8 seconds to be ready to init a capture
                    var output = adapter.GetOutput(i);
                    var output1 = output.QueryInterface<IDXGIOutput1>();
                    _duplications.Add(
                        i,
                        new Duplicator(0, i, device, output1)
                        );
                    populatedDuplicatorsCount++;
                }
                catch
                {
                    //LOG
                }
            }
            return populatedDuplicatorsCount;
        }
    }
}
