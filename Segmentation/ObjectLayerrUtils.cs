using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpAccessory.Imaging.Classification;
using SharpAccessory.Imaging.Classification.Features.Shape;
using SharpAccessory.Imaging.Segmentation;
using SharpAccessory.Threading;

namespace Segmentation
{
    public class ObjectLayerrUtils
    {
        /// <summary>
        ///  new Feature:
        /// centerX
        /// centerY
        /// </summary>
        /// <param name="objectLayer"></param>
        /// <returns></returns>
        public static ObjectLayer AddFeatureCenterPoint(ObjectLayer objectLayer)
        {
            var listImageObjects = new List<ImageObject>();
            var dicPoints = new Dictionary<uint, List<Point>>();

            for (var x = 0; x < objectLayer.Map.Width; x++)
            {
                for (var y = 0; y < objectLayer.Map.Height; y++)
                {
                    if (objectLayer.Map[x, y] == 0) continue;

                    if (!dicPoints.ContainsKey(objectLayer.Map[x, y]))
                    {
                        dicPoints.Add(objectLayer.Map[x, y], new List<Point>());
                    }
                    dicPoints[objectLayer.Map[x, y]].Add(new Point(x, y));
                }
            }
            foreach (var imageObject in objectLayer.Objects)
            {
                var sumX = 0;
                var sumY = 0;
                foreach (var point in dicPoints[imageObject.Id])
                {
                    sumX += point.X;
                    sumY += point.Y;
                }
                var item = CopyImageObject(imageObject.Id, imageObject);
                item.Features.Add(new Feature("centerX", sumX / dicPoints[imageObject.Id].Count));
                item.Features.Add(new Feature("centerY", sumY / dicPoints[imageObject.Id].Count));

                listImageObjects.Add(item);
            }

            return new ObjectLayer(objectLayer.Map, listImageObjects.ToArray(), objectLayer.Name);
        }

        /// <summary>
        ///  new Feature:
        /// range
        /// </summary>
        /// <param name="objectLayer"></param>
        /// <returns></returns>
        public static ObjectLayer AddFeatureRange(ObjectLayer objectLayer)
        {
            var listImageObjects = new List<ImageObject>();

            foreach (var imageObject in objectLayer.Objects)
            {
                var item = CopyImageObject(imageObject.Id, imageObject);
                item.Features.Add(new Feature("range", imageObject.Contour.Length));

                listImageObjects.Add(item);
            }       

            return new ObjectLayer(objectLayer.Map, listImageObjects.ToArray(), objectLayer.Name);
        }

        /// <summary>
        ///  new Feature:
        /// area
        /// </summary>
        /// <param name="objectLayer"></param>
        /// <returns></returns>
        public static ObjectLayer AddFeatureArea(ObjectLayer objectLayer)
        {
            var listImageObjects = new List<ImageObject>();
            var dicCountPixel = objectLayer.Objects.ToDictionary(imageObject => imageObject.Id, imageObject => 0);

            for (var x = 0; x < objectLayer.Map.Width; x++)
            {
                for (var y = 0; y < objectLayer.Map.Height; y++)
                {
                    if (objectLayer.Map[x, y] != 0)
                    {
                        dicCountPixel[objectLayer.Map[x, y]]++;
                    }
                }
            }

            foreach (var imageObject in objectLayer.Objects)
            {
                var item = CopyImageObject(imageObject.Id, imageObject);
                item.Features.Add(new Feature("area", dicCountPixel[imageObject.Id]));

                listImageObjects.Add(item);
            }
            return new ObjectLayer(objectLayer.Map, listImageObjects.ToArray(), objectLayer.Name);
        }

        /// <summary>
        /// new Feature:
        /// coresInNear
        /// <required_feature> CenterPoint of Cores! </required_feature>
        /// </summary>
        /// <param name="objectLayer"></param>
        /// <param name="cores"></param>
        /// <returns></returns>
        public static ObjectLayer AddFeatureCoresInNear(ObjectLayer objectLayer, ObjectLayer cores)
        {
            const int inNear = 75;
            const int calcXPoint = 30;

            var listImageObjects = new List<ImageObject>();
            var dicCoresInNear = new Dictionary<uint, HashSet<uint>>();


            //Zellkerne in der Nähe ermitteln
            foreach (var imageObject in objectLayer.Objects)
            {
                var counter = 0;
                dicCoresInNear.Add(imageObject.Id, new HashSet<uint>());

                //Berechnung aller Zellkerne in der Nähe
                foreach (var contourPoint in imageObject.Contour.GetPoints())
                {
                    counter++;

                    //nur jeden x. Punkt berechen aus performance gründen
                    if (counter % calcXPoint != 0) continue;

                    counter = 0;
                    foreach (var coreImageObject in cores.Objects)
                    {
                        if (
                            Math.Sqrt(
                                Math.Pow(contourPoint.X - coreImageObject.Features.GetFeatureByName("centerX").Value, 2) +
                                Math.Pow(contourPoint.Y - coreImageObject.Features.GetFeatureByName("centerY").Value, 2)) <
                            inNear)
                        {
                            dicCoresInNear[imageObject.Id].Add(coreImageObject.Id);
                        }
                    }
                }

                var item = CopyImageObject(imageObject.Id, imageObject);
                item.Features.Add(new Feature("coresInNear", dicCoresInNear[imageObject.Id].Count));
                item.Class = 0 != dicCoresInNear[imageObject.Id].Count
                    ? new Class("coresInNear", Color.Green)
                    : new Class("noCoresinNear", Color.Black);
                listImageObjects.Add(item);
            }

            return new ObjectLayer(objectLayer.Map, listImageObjects.ToArray(), objectLayer.Name);
        }

        /// <summary>
        /// new Feature:
        /// FormFactorOfContour
        /// </summary>
        /// <param name="objectLayer"></param>
        /// <returns></returns>
        public static ObjectLayer AddFeatureFormFactor(ObjectLayer objectLayer)
        {
            var listImageObjects = new List<ImageObject>();

            foreach (var imageObject in objectLayer.Objects)
            {
                var item = CopyImageObject(imageObject.Id, imageObject);
                item.Features.Add(new FormFactorOfContour(ContourProperties.FromContour(imageObject.Contour)));

                listImageObjects.Add(item);
            }

            return new ObjectLayer(objectLayer.Map, listImageObjects.ToArray(), objectLayer.Name);
        }

        /// <summary>
        ///  objectLayer2 is in front of objectlayer 1
        /// </summary>
        /// <param name="objectLayer1"></param>
        /// <param name="objectLayer2"></param>
        /// <returns></returns>
        public static ObjectLayer MergeObjectLayers(ObjectLayer objectLayer1, ObjectLayer objectLayer2)
        {
            var maxId = 0u;

            var listImageObjects = new List<ImageObject>();
            var mapObjectLayerMap = new Map(objectLayer1.Map.Width, objectLayer1.Map.Height);

            //Copy Map from Objectlayer 1
            for (var x = 0; x < objectLayer1.Map.Width; x++)
            {
                for (var y = 0; y < objectLayer1.Map.Height; y++)
                {
                    mapObjectLayerMap[x, y] = objectLayer1.Map[x, y];
                }
            }

            foreach (var imageObject in objectLayer1.Objects)
            {
                maxId = Math.Max(maxId, imageObject.Id);
                listImageObjects.Add(CopyImageObject(imageObject.Id, imageObject));
            }
            foreach (var imageObject in objectLayer2.Objects)
            {
                maxId++;
                //Copy Map from ObjectLayer 2 mit umschlüsseln der IDs
                for (var x = 0; x < objectLayer2.Map.Width; x++)
                {
                    for (var y = 0; y < objectLayer2.Map.Height; y++)
                    {
                        if (objectLayer2.Map[x, y] == imageObject.Id)
                        {
                            mapObjectLayerMap[x, y] = maxId;
                        }
                    }
                }
                listImageObjects.Add(CopyImageObject(maxId, imageObject));
            }
            return new ObjectLayer(mapObjectLayerMap, listImageObjects.ToArray(), objectLayer1.Name + "-" + objectLayer2.Name);
        }

        /// <summary>
        ///  Gibt neuen Objetlayerr zurück mit entsprechendder Klasse
        /// </summary>
        /// <param name="objectLayer"></param>
        /// <param name="clClass"></param>
        /// <returns></returns>
        public static  ObjectLayer SetClass(ObjectLayer objectLayer, Class clClass)
        {
            var listImageObjects = new List<ImageObject>();

            foreach (var imageObject in objectLayer.Objects)
            {
                var item = CopyImageObject(imageObject.Id, imageObject);
                item.Class = clClass;
                listImageObjects.Add(item);
            }

            return new ObjectLayer(objectLayer.Map, listImageObjects.ToArray(), objectLayer.Name);
        }

        /// <summary>
        ///  Copy a Image Object with Features
        /// </summary>
        /// <param name="newId"></param>
        /// <param name="copyImageObject"></param>
        /// <returns></returns>
        public static ImageObject CopyImageObject(uint newId, ImageObject copyImageObject)
        {
            var imageObject = new ImageObject(newId, copyImageObject.Contour, copyImageObject.Class);

            //Copy Features
            foreach (var feature in copyImageObject.Features)
            {
                imageObject.Features.Add(feature);
            }

            return imageObject;
        }

        public static List<uint> GetFeatureValueList(ObjectLayer objectLayer, string featureName)
        {
            var featureValueList = new List<uint>();

            foreach (var imageObject in objectLayer.Objects)
            {
                featureValueList.Add((uint) imageObject.Features.GetFeatureByName(featureName).Value);
            }
            featureValueList.Sort();
            return featureValueList;
        }
    }
}