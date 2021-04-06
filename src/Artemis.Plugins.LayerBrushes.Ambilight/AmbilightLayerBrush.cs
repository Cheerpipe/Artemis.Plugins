using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Ambilight.PropertyGroups;
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Artemis.Plugins.LayerBrushes.Ambilight
{
    public class AmbilightLayerBrush : PerLedLayerBrush<AmbilightPropertyGroup>
    {

        private readonly object pixmapLock = new object();
        private static readonly object AcquireNextFrameLock = new object();
        private SKPixmap pixmap;
        private Duplicator duplicator;

        public override void EnableLayerBrush()
        {

        }

        public override void DisableLayerBrush()
        {
            //StopDesktopDuplicator();
            //TODO: Dispose all duplicators
        }

        public override void Update(double deltaTime)
        {
            GetNextFrame();
        }

        public async Task GetNextFrame()
        {
            duplicator = DuplicatorFactory.GetDuplicator(Properties.Output.BaseValue);

            if (duplicator == null)
                return;

            IDXGIResource screenResource;
            OutduplFrameInfo frameInfo;

            try
            {
                duplicator.Duplication.AcquireNextFrame(1, out frameInfo, out screenResource);
                using (var tempTexture = screenResource.QueryInterface<ID3D11Texture2D>())
                    duplicator.Device.ImmediateContext.CopySubresourceRegion(duplicator.SmallerTexture, 0, 0, 0, 0, tempTexture, 0);
                duplicator.Device.ImmediateContext.GenerateMips(duplicator.SmallerTextureView);
                duplicator.Device.ImmediateContext.CopySubresourceRegion(duplicator.StagingTexture, 0, 0, 0, 0, duplicator.SmallerTexture, 1);
                var dataBox = duplicator.Device.ImmediateContext.Map(duplicator.StagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
                ProcessDataIntoSKPixmap(dataBox);
                duplicator.Device.ImmediateContext.Unmap(duplicator.StagingTexture, 0);
                screenResource.Dispose();
                duplicator.Duplication.ReleaseFrame();
            }
            catch (SharpGen.Runtime.SharpGenException e)
            {
                if (e.ResultCode == Vortice.DXGI.ResultCode.AccessLost)
                {
                    DuplicatorFactory.PopulateDuplicators();
                }
                else if (e.ResultCode == Vortice.DXGI.ResultCode.WaitTimeout)
                {
                    //ignore
                }
                else if (e.ResultCode == Vortice.DXGI.ResultCode.InvalidCall)
                {
                    throw;
                }
                else
                {
                    throw;
                }
            }
            finally
            {

            }
        }

        private void ProcessDataIntoSKPixmap(MappedSubresource dataBox)
        {
            int width = dataBox.RowPitch / 4;
            int height = dataBox.DepthPitch / width / 4;

            var skInfo = new SKImageInfo
            {
                ColorType = SKColorType.Bgra8888,
                AlphaType = SKAlphaType.Premul,
                Height = height,
                Width = width
            };

            lock (pixmapLock)
            {
                pixmap = new SKPixmap(skInfo, dataBox.DataPointer);
            }
        }

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            if (duplicator == null)
                return SKColors.Transparent;

            const int sampleSize = 9;
            const int sampleDepth = 3;

            lock (pixmapLock)
            {
                var renderBounds = Layer.Bounds;
                var widthScale = pixmap.Width / renderBounds.Width;
                var heightScale = pixmap.Height / renderBounds.Height;
                int x = (int)Math.Max((renderPoint.X * widthScale), 0);
                int y = (int)Math.Max((renderPoint.Y * heightScale), 0);
                int width = (int)(led.Rectangle.Width * widthScale);
                int height = (int)(led.Rectangle.Height * heightScale);

                int verticalSteps = height / (sampleDepth - 1);
                int horizontalSteps = width / (sampleDepth - 1);

                int a = 0, r = 0, g = 0, b = 0;
                for (int horizontalStep = 0; horizontalStep < sampleDepth; horizontalStep++)
                {
                    for (int verticalStep = 0; verticalStep < sampleDepth; verticalStep++)
                    {
                        var bruhX = x + horizontalSteps * horizontalStep;
                        var bruhY = y + verticalSteps * verticalStep;
                        SKColor color = pixmap.GetPixelColor(
                            Math.Min(bruhX, pixmap.Width - 1),
                            Math.Min(bruhY, pixmap.Height - 1)
                            );
                        r += color.Red;
                        g += color.Green;
                        b += color.Blue;
                        a += color.Alpha;
                    }
                }
                return new SKColor((byte)(r / sampleSize), (byte)(g / sampleSize), (byte)(b / sampleSize));
            }
        }
    }
}