using System.Drawing;
using System.Linq;
using Glaukopis.SharpAccessoryIntegration.Segmentation;
using SharpAccessory.Imaging.Automation;
using SharpAccessory.Imaging.Segmentation;

namespace Segmentation
{
    public class ObjectLayerCreate
    {
        public static ObjectLayer CreateLuminaLayer(Bitmap bitmap)
        {
            var segmentation = new SimpleLuminaSegmentation();
            var layer = segmentation.Execute(bitmap);
            layer = ObjectLayerrUtils.AddFeatureRange(ObjectLayerrUtils.AddFeatureArea(layer));
            layer = segmentation.ImprObjectLayer(layer);
            layer.Name = "lumina";
            return layer;
        }

        public static ObjectLayer CreateCoresLayer(Bitmap bitmap)
        {
            var process = new CellCoresHE();
            var processResult = process.Execute(new ProcessExecutionParams(bitmap));
            var layer = processResult.Layers.Last();
            layer = ObjectLayerrUtils.AddFeatureRange(ObjectLayerrUtils.AddFeatureArea(layer));
            layer.Name = "cores";
            return layer;
        }
    }
}
