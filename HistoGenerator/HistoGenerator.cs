using System;
using System.Drawing;
using System.Linq;
using Glaukopis.SlideProcessing;
using Glaukopis.SharpAccessoryIntegration;
using SharpAccessory.Imaging.Filters;
using System.IO;
using VMscope.InteropCore.VirtualMicroscopy;
using System.Collections.Generic;
using SharpAccessory.Threading;

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
            //Eingabe Parameter
            String srcSlides = args[0];
            String outCsv = args[1];
            String outputPics = args[2];

            //Nötige Ordner ertsellen 
            if (!Directory.Exists(outputPics))
                Directory.CreateDirectory(outputPics);
            if (!Directory.Exists(outCsv))
                Directory.CreateDirectory(outCsv);

            //Annotaionen und Histowerte holen und in liste sammeln 
            TissueAnnotaionList annotationList;
            annotationList = getAnnotaions(srcSlides, outputPics);

            //Liste anpassen
            annotationList.sort();
            annotationList.cleanUp();

            //Ergebnisse in CSV schreiben
            Console.WriteLine("Results saved to:  {0} ", outCsv);
            annotationList.writeToCsv(outCsv + @"\data.csv");
            annotationList.groubByOnClassAndOthers("Fett").writeToCsv(outCsv + @"\dataFett.csv");
            annotationList.groubByOnClassAndOthers("Fettgewebe").writeToCsv(outCsv + @"\dataFettgeweebe.csv");
            annotationList.groubByOnClassAndOthers("Tumor").writeToCsv(outCsv + @"\dataTumor.csv");
            annotationList.groubByOnClassAndOthers("Entzündung").writeToCsv(outCsv + @"\dataEntzuendung.csv");
            annotationList.groubByOnClassAndOthers("Gefäß").writeToCsv(outCsv + @"\dataGefaess.csv");
            annotationList.groubByOnClassAndOthers("Mikrokalk").writeToCsv(outCsv + @"\dataMikrokalk.csv");
            annotationList.groubByOnClassAndOthers("Nerv").writeToCsv(outCsv + @"\dataNerv.csv");
            annotationList.groubByOnClassAndOthers("Stroma").writeToCsv(outCsv + @"\dataStroma.csv");
            annotationList.groubByOnClassAndOthers("DCIC").writeToCsv(outCsv + @"\dataDCIC.csv");
            annotationList.groubByOnClassAndOthers("Kalk").writeToCsv(outCsv + @"\dataKalk.csv");
            annotationList.groubByOnClassAndOthers("Normales Mammaepithel").writeToCsv(outCsv + @"\dataNormalesMammaepithel.csv");

            //FINISH
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
                    Parallel.ForEach(slideCache.Slide.GetAnnotations(), (annotation) =>
                    {
                        if (abort != 0)
                        {
                            if (i >= abort)
                                return;

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

                            TissueAnnotationClass tissueAnnotation = new TissueAnnotationClass(annotation.Id, annotation.Name, slideCache.SlideName);

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
                    });
                    /*foreach (var annotation in slideCache.Slide.GetAnnotations())
                    {
                       
                    }*/
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
