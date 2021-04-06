using System;
using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Usage = Vortice.Direct3D11.Usage;

namespace Artemis.Plugins.LayerBrushes.Ambilight
{
    //adapted from https://stackoverflow.com/questions/24064837/resizing-a-dxgi-resource-or-texture2d-in-sharpdx
    public class Duplicator : IDisposable
    {
        private IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();

        public IDXGIOutputDuplication Duplication { get => _duplication; }
        private IDXGIOutputDuplication _duplication;

        public ID3D11Texture2D StagingTexture { get => _stagingTexture; }
        private ID3D11Texture2D _stagingTexture;

        public ID3D11Device Device { get => _device; }
        private ID3D11Device _device;

        public ID3D11Texture2D SmallerTexture { get => _smallerTexture; }
        private ID3D11Texture2D _smallerTexture;

        public ID3D11ShaderResourceView SmallerTextureView { get => _smallerTextureView; }
        private ID3D11ShaderResourceView _smallerTextureView;

        public Duplicator(int adapterId, int outputId, ID3D11Device device, IDXGIOutput1 output1)
        {
            _device = device;
            var bounds = output1.Description.DesktopCoordinates;
            var width = bounds.Right - bounds.Left;
            var height = bounds.Bottom - bounds.Top;

            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width / 2,
                Height = height / 2,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = Usage.Staging
            };
            _stagingTexture = _device.CreateTexture2D(textureDesc);

            var smallerTextureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.GenerateMips,
                MipLevels = 4,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = Usage.Default
            };
            _smallerTexture = _device.CreateTexture2D(smallerTextureDesc);
            _smallerTextureView = _device.CreateShaderResourceView(_smallerTexture);

            _duplication = output1.DuplicateOutput(_device);
            Thread.Sleep(500);
        }

        public void Close()
        {
            _duplication?.Release();
            Thread.Sleep(500);
        }

        public void Dispose()
        {
            _stagingTexture?.Dispose();
            _smallerTexture?.Dispose();
            _smallerTextureView?.Dispose();
            //_duplication.Dispose(); //Check if it can be disposable before dispose
        }
    }
}
