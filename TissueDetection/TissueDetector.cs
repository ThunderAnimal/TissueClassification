/*
 * @author Sebastian Lohmann
 */
namespace TissueDetection{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Collections;
    using Glaukopis.SlideProcessing;
    using SharpAccessory.Imaging.Processors;
    using System.Threading.Tasks;
    using System.IO;

    internal class TissueDetector{
		private static void Main(string[] args){
            const double GRENZ_ENTROPIE = 0.1;

            foreach (var slideName in Util.GetSlideFilenames(new string[] { args[0] })){
				using(var slideCache=new SlideCache(slideName)){
                    // scale=1 -> baselayer , sollte so klein wie möglich sein um die Rechenzeit zu minimieren
                    // targetSize Größe der zu prozessierenden Bilder, hängt von der hardware ab und bestimmt die Auflösung der Heatmap, für niedrige scale-Werte sollte auch die Größe reduziert werden
                    // Auflösung: Breite=tissueSlidePartitioner.Columns Höhe=tissueSlidePartitioner.Rows
                    // var: implizit typisiert, tatsächlich stark typisiert da der Compiler den Typ kennt; RMT->Goto To Definition
                    // Empfehlung: http://shop.oreilly.com/product/0636920040323.do die 5.0 gibt es auch als pdf im Internet
                    var tissueSlidePartitioner=new SlidePartitioner<bool>(slideCache.Slide,0.2f,new Size(500,500)); //Nicht unbedingt auf dem Baselayerr arbeiten zB. 1f --> 0.1
					using(var overViewImage=slideCache.Slide.GetImagePart(0,0,slideCache.Slide.Size.Width,slideCache.Slide.Size.Height,tissueSlidePartitioner.Columns,tissueSlidePartitioner.Rows)){
						//TODO falls die Gewebeerkennung auf dem Übersichtsbild stattfinden soll, dann hier
						slideCache.SetImage("overview",overViewImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\...
					}

                    //Multithreading 
                    Parallel.ForEach(tissueSlidePartitioner.Values, (tile) =>
                    {
                        using (var tileImage = slideCache.Slide.GetImagePart(tile))
                        {
                            var containsTissue = false;
                            double entropie = 0.0;
                            #region hier sollte containsTissue richtig bestimmt werden
                            var bitmapProcessor = new BitmapProcessor(tileImage);//liefert schnelleren Zugriff auf die Pixel-Werte, alternativ auch SharpAccessory.Imaging.Processors.GrayscaleProcessor

                            int[] greyArray = new int[256];

                            for (var y = 0; y < bitmapProcessor.Height; y++)
                            {
                                for (var x = 0; x < bitmapProcessor.Width; x++)
                                {
                                    var r = bitmapProcessor.GetRed(x, y);
                                    var g = bitmapProcessor.GetGreen(x, y);
                                    var b = bitmapProcessor.GetBlue(x, y);

                                    var grauwert = (int)(r + g + b) / 3;
                                    greyArray[grauwert]++;
                                }
                            }
                            bitmapProcessor.Dispose();

                            //Calculate Shannon Entropie
                            entropie = calcEntropie(greyArray);

                            //Contains Tissue ermitteln
                            if (0 == tile.Index.Y)
                            {
                                containsTissue = false;//oberste Reihe sollte kein Gewebe enthalten
                            }
                            else
                            {
                                //if (slideCache.Slide.GetAnnotationsInArea(tile.SourceArea).Any()) containsTissue = true;//Kacheln mit Annotationen enthalten Gewebe
                                if (entropie > GRENZ_ENTROPIE) containsTissue = true;
                            }
                            #endregion

                            //Wert zur Kachel speichern
                            tile.Data = containsTissue;

                            //Only for Debug
                            var saveImage = false;
                            if (saveImage)
                            {
                                string path = args[1] + @"\" + slideCache.SlideName + @"\";
                                if (containsTissue)
                                    path += @"isTissue\";
                                else
                                    path += @"noTissue\";

                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                tileImage.Save(path + tile.Index.X + "-" + tile.Index.Y + ".png");
                                //var tileCache = slideCache.GetTileCache(tile.Index);
                                //tileCache.SetImage("rgb",tileImage);//speichert unter C:\ProgramData\processingRepository\[slideCache.SlideName]\[Index]\... ansonsten tileImage.Save("[uri].png")
                            }
                            Console.WriteLine(slideCache.SlideName + "-" + tile.Index + " done - containsTissue: " + containsTissue.ToString() + " - entropie: " + entropie.ToString());
                        }
                    });
                    //true wird zu grün, false zu rot; syntax ist lambda (=>) mit einem conditional operator (?)
                    Func<bool, Color> f = b =>
                    {
                        if (b) return Color.Red;
                        else return Color.Green;
                    };
                    using (var heatMap = tissueSlidePartitioner.GenerateHeatMap(f))
                    {
                        slideCache.SetImage("tissueHeatMap", heatMap);
                    }
				}
			}
		}

        //Berechnung der Entropie Einehit bits/pixel
        private static double calcEntropie(int[] absoluteHistogramm)
        {
            double[] normiertesHistogramm = new double[absoluteHistogramm.Length];

            double entropie = 0;
            int countPixel = 0;

            //Normierte Histogramm ermitteln
            foreach (int count in absoluteHistogramm)
            {
                countPixel += count;
            }
            for(int i = 0; i < absoluteHistogramm.Length; i++)
            {
                normiertesHistogramm[i] = (double)absoluteHistogramm[i] / countPixel;
            }

            //Entropie ermitteln
            for(int i = 0; i < normiertesHistogramm.Length; i++)
            {
                if(normiertesHistogramm[i] > 0)
                {
                    entropie += normiertesHistogramm[i] * Math.Log(normiertesHistogramm[i], 2);
                }
            }
            entropie = entropie * -1;

            return entropie;
        }
	}
}