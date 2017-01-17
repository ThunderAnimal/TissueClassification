using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glaukopis.SharpAccessoryIntegration;
using Segmentation;
using SharpAccessory.Imaging.Filters;

namespace ClassificatioDataGenerator
{
    class ComputeImageFeatures
    {
        public static TissueAnnotationClass ComputeFeatures(TissueAnnotationClass tissueAnnotationOld, Bitmap image)
        {
            using (Bitmap hImage = image.Clone() as Bitmap, eImage = image.Clone() as Bitmap)
            {
                TissueAnnotationClass tissueAnnotation = tissueAnnotationOld.Clone();
                int imageSize = 0;

                //H und E Werte aus Bild ermitteln
                var gpH = new ColorDeconvolution().Get1stStain(hImage, ColorDeconvolution.KnownStain.HaematoxylinEosin);
                var gpE = new ColorDeconvolution().Get2ndStain(eImage, ColorDeconvolution.KnownStain.HaematoxylinEosin);

                uint[] hHistogram = new uint[256];
                uint[] eHistogram = new uint[256];


                foreach (var grayscalePixel in gpH.Pixels())
                {
                    imageSize++;
                    hHistogram[grayscalePixel.V]++;
                    eHistogram[gpE.GetPixel(grayscalePixel.X, grayscalePixel.Y)]++;
                }


                //Segmentation aus dem Bild ermitteln
                //Layers erstellen
                var luminaLayer = ObjectLayerCreate.CreateLuminaLayer(image);
                var coresLayer = ObjectLayerCreate.CreateCoresLayer(image);

                //Features dem layer hinzufügen
                coresLayer =
                    ObjectLayerrUtils.AddFeatureFormFactor(
                        ObjectLayerrUtils.AddFeatureCenterPoint(coresLayer));
                luminaLayer =
                    ObjectLayerrUtils.AddFeatureCoresInNear(
                        ObjectLayerrUtils.AddFeatureFormFactor(luminaLayer), coresLayer);


                //Alle Features ermitteln und in Listen schreiben --> sortiert um anschließend Mid und Avverage auszurechen
                List<uint> hHistogramList = histogramToList(hHistogram);
                List<uint> eHistogramList = histogramToList(eHistogram);

                List<uint> coresSizeList = ObjectLayerrUtils.GetFeatureValueList(coresLayer, "area");
                List<uint> luminaSizeList = ObjectLayerrUtils.GetFeatureValueList(luminaLayer, "area");
                List<uint> coresFormFactorList = ObjectLayerrUtils.GetFeatureValueList(coresLayer,
                    "FormFactorOfContour");

                List<uint> luminaFormFactorWithSize = new List<uint>();
                List<uint> luminaCoresInNear = new List<uint>();
                List<uint> luminaCoresInNearWithForm = new List<uint>();
                foreach (var imageObject in luminaLayer.Objects)
                {
                    var coresInNear = imageObject.Features.GetFeatureByName("coresInNear").Value;
                    var formFactor = imageObject.Features.GetFeatureByName("FormFactorOfContour").Value;
                    var area = imageObject.Features.GetFeatureByName("area").Value;

                    if (Math.Abs(coresInNear) != 0)
                    {
                        luminaCoresInNear.Add((uint)(area / coresInNear));
                        luminaCoresInNearWithForm.Add((uint)((area * formFactor) / coresInNear));
                    }
                    if (Math.Abs(formFactor) != 0)
                    {
                        luminaFormFactorWithSize.Add((uint)(area / (2 * formFactor)));
                    }

                }
                luminaCoresInNear.Sort();
                luminaCoresInNearWithForm.Sort();
                luminaFormFactorWithSize.Sort();

                //alle Features dem Object hinzufügen
                tissueAnnotation.Q25H = calcQuantil(hHistogramList, 0.25);
                tissueAnnotation.MeanH = calcQuantil(hHistogramList, 0.5);
                tissueAnnotation.Q75H = calcQuantil(hHistogramList, 0.75);

                tissueAnnotation.Q25E = calcQuantil(eHistogramList, 0.25);
                tissueAnnotation.MeanE = calcQuantil(eHistogramList, 0.5);
                tissueAnnotation.Q75E = calcQuantil(eHistogramList, 0.75);

                if (coresSizeList.Count == 0)
                    tissueAnnotation.CountCores = 0u;
                else
                    tissueAnnotation.CountCores = (uint)(imageSize / coresSizeList.Count);

                if (luminaSizeList.Count == 0)
                    tissueAnnotation.CountLumina = 0u;
                else
                    tissueAnnotation.CountLumina = (uint)(imageSize / luminaSizeList.Count);

                tissueAnnotation.MidCoresSize = calcMid(coresSizeList);
                tissueAnnotation.MeanCoresSize = calcQuantil(coresSizeList, 0.5);
                tissueAnnotation.Q25CoresSize = calcQuantil(coresSizeList, 0.25);
                tissueAnnotation.Q75CoresSize = calcQuantil(coresSizeList, 0.75);

                tissueAnnotation.MidLuminaSize = calcMid(luminaSizeList);
                tissueAnnotation.MeanLuminaSize = calcQuantil(luminaSizeList, 0.5);
                tissueAnnotation.Q25LuminaSize = calcQuantil(luminaSizeList, 0.25);
                tissueAnnotation.Q75LuminaSize = calcQuantil(luminaSizeList, 0.75);


                if (coresSizeList.Count == 0)
                    tissueAnnotation.DensityCores = 0;
                else
                    tissueAnnotation.DensityCores = (uint)(imageSize /
                                                            coresSizeList.Count);

                tissueAnnotation.MidFormFactorCores = calcMid(coresFormFactorList);
                tissueAnnotation.MeanFormFactorCores = calcQuantil(coresFormFactorList, 0.5);
                tissueAnnotation.Q25FormFactorCores = calcQuantil(coresFormFactorList, 0.25);
                tissueAnnotation.Q75FormFactorCores = calcQuantil(coresFormFactorList, 0.75);

                tissueAnnotation.MidFormFactorLuminaWithSize = calcMid(luminaFormFactorWithSize);
                tissueAnnotation.MeanFormFactorLuminaWithSize = calcQuantil(luminaFormFactorWithSize, 0.5);
                tissueAnnotation.Q25FormFactorLuminaWithSize = calcQuantil(luminaFormFactorWithSize, 0.25);
                tissueAnnotation.Q75FormFactorLuminaWithSize = calcQuantil(luminaFormFactorWithSize, 0.75);

                tissueAnnotation.MidDensityLuminaCoresInNear = calcMid(luminaCoresInNear);
                tissueAnnotation.MeanDensityLuminaCoresInNear = calcQuantil(luminaCoresInNear, 0.5);
                tissueAnnotation.Q25DensityLuminaCoresInNear = calcQuantil(luminaCoresInNear, 0.25);
                tissueAnnotation.Q75DensityLuminaCoresInNear = calcQuantil(luminaCoresInNear, 0.75);

                tissueAnnotation.MidDensityFormFactorLuminaCoresInNear = calcMid(luminaCoresInNearWithForm);
                tissueAnnotation.MeanDensityFormFactorLuminaCoresInNear =
                    calcQuantil(luminaCoresInNearWithForm, 0.5);
                tissueAnnotation.Q25DensityFormFactorLuminaCoresInNear =
                    calcQuantil(luminaCoresInNearWithForm, 0.25);
                tissueAnnotation.Q75DensityFormFactorLuminaCoresInNear =
                    calcQuantil(luminaCoresInNearWithForm, 0.75);

                return tissueAnnotation;
            }
        }

        private static uint calcQuantil(List<uint> list, double percentage)
        {
            if (list.Count == 0)
                return 0;

            return list.ElementAt((int)(list.Count * percentage));
        }

        private static uint calcMid(List<uint> list)
        {
            if (list.Count == 0)
                return 0;
            var sum = 0d;
            foreach (var item in list)
            {
                sum += item;
            }
            return (uint)(sum / list.Count);
        }

        private static List<uint> histogramToList(uint[] histogram)
        {
            List<uint> list = new List<uint>();
            for (uint i = 0; i < histogram.Length; i++)
            {
                for (uint k = 0; k < histogram[i]; k++)
                {
                    list.Add(i);
                }
            }
            return list;
        }
    }
}
