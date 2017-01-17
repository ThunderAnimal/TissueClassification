/*
 * @author Sebastian Lohmann
 */

using System.Collections.Generic;
using Segmentation;

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
        /// <summary>
        /// Improve Calc-Function for Lumina
        /// </summary>
        /// <requred_features>
        ///  Area
        /// </requred_features>
        /// <param name="lumina"></param>
        /// <returns></returns>
        public ObjectLayer ImprObjectLayer(ObjectLayer objectLayerLumina)
        {
            const double minArea = 500;

            var listImageObjects = new List<ImageObject>();
            var mapObjectLayerMap = new Map(objectLayerLumina.Map.Width, objectLayerLumina.Map.Height);

            foreach (var imageObject in objectLayerLumina.Objects)
            {
                //Nur Werte Aufnehmen die auch die min. Bedingung erfüllen
                if (imageObject.Features.GetFeatureByName("area").Value >= minArea)
                {   
                    //In neue Liste eintragen
                    listImageObjects.Add(ObjectLayerrUtils.CopyImageObject(imageObject.Id, imageObject));

                    //In neuer Map eintragen
                    for (var x = 0; x < objectLayerLumina.Map.Width; x++)
                    {
                        for (var y = 0; y < objectLayerLumina.Map.Height; y++)
                        {
                            if (objectLayerLumina.Map[x, y] == imageObject.Id)
                            {
                                mapObjectLayerMap[x, y] = imageObject.Id;
                            }
                        }
                    }
                }
            }

            return new ObjectLayer(mapObjectLayerMap, listImageObjects.ToArray(), objectLayerLumina.Name);
        }
	}
}
