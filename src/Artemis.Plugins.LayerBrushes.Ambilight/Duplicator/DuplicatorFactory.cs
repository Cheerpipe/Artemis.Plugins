using System;
using System.Collections.Generic;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Usage = Vortice.Direct3D11.Usage;

namespace Artemis.Plugins.LayerBrushes.Ambilight
{
    public static class DuplicatorFactory
    {
        private const int DEFAULT_ADAPTER_ID = 0; //Should be always 0
        private static Dictionary<int, Duplicator> duplications;

        static DuplicatorFactory()
        {
            PopulateDuplicators();
        }

        public static readonly FeatureLevel[] s_featureLevels = new[]
        {
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
            FeatureLevel.Level_9_3,
            FeatureLevel.Level_9_2,
            FeatureLevel.Level_9_1,
        };

        public static List<IDXGIOutput> GetDeviceOutputs()
        {
            /*
            IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            ID3D11Device device;
            IDXGIAdapter adapter = factory.GetAdapter1(DEFAULT_ADAPTER_ID);
            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.Debug, s_featureLevels, out device);
            int i = 0;
            IDXGIOutput output;
            List<IDXGIOutput> outputs = new();
            while (adapter.EnumOutputs(i, out output) != Vortice.DXGI.ResultCode.NotFound)
            {
                outputs.Add(output);
                ++i;
            }
            factory.Dispose();
            device.Dispose
            return outputs;
            */
            return null;
        }
        public static Duplicator GetDuplicator(int outputId)
        {
            if (outputId > duplications.Count)
                return null;
            return duplications[outputId];
        }

        public static bool RePopulateDuplicator(int outputId)
        {
            try
            {
                duplications[outputId] = new Duplicator(0, outputId);
                return true;
            }
            catch (Exception e)
            {
                return false;
                //TODO: LOG
            }
        }

        public static int PopulateDuplicators()
        {
           // var outputs = GetDeviceOutputs();
            int populatedDuplicatorsCount = 0;

            duplications = new();
            //TODO: Handle existing and working duplicators. Populate only those that are invalid
            for (int i = 0; i <  1/*outputs.Count*/; i++)
            {
                try
                {
                    duplications.Add(
                        i,
                        new Duplicator(0, i)
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
