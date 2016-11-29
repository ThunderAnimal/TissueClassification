using System;
using System.Drawing;
using System.Linq;
using Glaukopis.SlideProcessing;
using Glaukopis.SharpAccessoryIntegration;
using SharpAccessory.Imaging.Filters;
using System.IO;
using VMscope.InteropCore.VirtualMicroscopy;
using System.Collections.Generic;

namespace HistoGenerator
{
    internal static class HistoGenerator
    {
        /// <summary>
        /// Config Variablen
        /// </summary>
        private static bool isSave = false; //Save Pictures
        private static int abort = 0; //Abort after x Anotions for each Dataset 0- disable

        private static void Main(string[] args)
        {
            String srcSlides = args[0];
            String outCsv = args[1];
            String outputPics = args[2];

            if (!Directory.Exists(outputPics))
            {
                Directory.CreateDirectory(outputPics);
            }

            TissueAnnotaionList annotationList;
            annotationList = getAnnotaions(srcSlides, outputPics);
            annotationList.sort();

            annotationList.writeToCsv(outCsv + @"\data.csv");
            annotationList.groubByOnClassAndOthers("Fett").writeToCsv(outCsv + @"\dataFett.csv");
            annotationList.groubByOnClassAndOthers("Tumor").writeToCsv(outCsv + @"\dataTumor.csv"); 

            Console.WriteLine("\n############ FINISHED ###############");
            Console.ReadKey();
        }
        
        private static TissueAnnotaionList getAnnotaions(String srcSlides, String outputPics)
        {
            TissueAnnotaionList annotationList = new TissueAnnotaionList();

            //Get Anotaions from silde
            foreach (var slideName in Util.GetSlideFilenames(new String[] { srcSlides }))
            {
                using (var slideCache = new SlideCache(slideName))
                {
                    int i = 0;
                    foreach (var annotation in slideCache.Slide.GetAnnotations())
                    {
                        if(abort != 0)
                        {
                            if (i >= abort)
                                break;

                            i++;
                        }
                       
                        //Annotaions Bitmap extrahieren
                        var contained = new List<IAnnotation>();
                        foreach (var candidate in slideCache.Slide.GetAnnotations())
                        {
                            if (candidate == annotation) continue;
                            var rectangle = annotation.BoundingBox;
                            rectangle.Intersect(candidate.BoundingBox);
                            if (rectangle.IsEmpty) continue;
                            if (annotation.BoundingBox.Contains(candidate.BoundingBox)) contained.Add(candidate);
                        }

                        using (Bitmap annotationBitmap = annotation.Extract(1, contained), hImage = annotationBitmap.Clone() as Bitmap, eImage = annotationBitmap.Clone() as Bitmap)
                        {

                            TissueClass tissueAnnotation = new TissueClass(annotation.Id, annotation.Name, slideCache.SlideName);

                            //H und E Werte ermitteln
                            var gpH = new ColorDeconvolution().Get1stStain(hImage, ColorDeconvolution.KnownStain.HaematoxylinEosin);
                            var gpE = new ColorDeconvolution().Get2ndStain(eImage, ColorDeconvolution.KnownStain.HaematoxylinEosin);

                            uint[] hHistogram = new uint[256];
                            uint[] eHistogram = new uint[256];

                            foreach (var grayscalePixel in gpH.Pixels())
                            {
                                hHistogram[grayscalePixel.V]++;
                                eHistogram[gpE.GetPixel(grayscalePixel.X, grayscalePixel.Y)]++;
                            }

                            List<uint> hHistogramList = histogramToList(hHistogram);
                            List<uint> eHistogramList = histogramToList(eHistogram);

                            tissueAnnotation.Q25H = calcQuantil(hHistogramList, 0.25);
                            tissueAnnotation.MeanH = calcQuantil(hHistogramList, 0.5);
                            tissueAnnotation.Q75H = calcQuantil(hHistogramList, 0.75);
                            tissueAnnotation.Q25E = calcQuantil(eHistogramList, 0.25);
                            tissueAnnotation.MeanE = calcQuantil(eHistogramList, 0.5);
                            tissueAnnotation.Q75E = calcQuantil(eHistogramList, 0.75);

                            //Zur Liste hinzufügren
                            annotationList.add(tissueAnnotation);

                            //FOR DEBUG Save Image
                            if (isSave)
                            {
                                annotationBitmap.Save(outputPics + "\\" + tissueAnnotation + ".png");
                            }

                            Console.WriteLine(tissueAnnotation + " exc:" + contained.Count);
                        }
                    }
                }
            }
            return annotationList;
        }

        private static uint calcQuantil(List<uint> list, double percentage)
        {
            return list.ElementAt((int)(list.Count * percentage));
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
