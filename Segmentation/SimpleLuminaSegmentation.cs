/*
 * @author Sebastian Lohmann
 */
namespace Glaukopis.SharpAccessoryIntegration.Segmentation {
	using System.Drawing;
	using SharpAccessory.Imaging.Processors;
	using SharpAccessory.Imaging.Segmentation;
	public class SimpleLuminaSegmentation{
		public ObjectLayer Execute(Bitmap image) {
			var m=new Map(image.Width,image.Height);
			using(var gp=new GrayscaleProcessor(image.Clone() as Bitmap,RgbToGrayscaleConversion.Mean))
				for(var x=0;x<image.Width;x++)
					for(var y=0;y<image.Height;y++)
						m[x,y]=gp.GetPixel(x,y)<200?0u:1u;
			var layer=new ConnectedComponentCollector().Execute(m);
			layer.Name="lumina";
			return layer;
		}
	}
}
