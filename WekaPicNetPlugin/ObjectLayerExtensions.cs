/*
 * @author Sebastian Lohmann
 */
namespace Glaukopis.Adapters.PicNetML {
	using System.Collections.Generic;
	using System.Linq;
	using SharpAccessory.Imaging.Segmentation;
	using weka.core;
	/// <summary>
	/// Erweiterungen des <see cref="ObjectLayer"/> für die Verwendung mit WEKA
	/// </summary>
	public static class ObjectLayerExtensions {
		/// <summary>
		/// erzeugt einen WEKA-kompatiblen Datensatz aus einem <see cref="ObjectLayer"/>
		/// </summary>
		/// <param name="objectLayer"><see cref="ObjectLayer"/></param>
		/// <param name="classNames">Aufzählung der Klassennamen die im Datensatz verwendet werden sollen</param>
		/// <param name="classAttributeName">>Name für das Klassenattribut</param>
		/// <returns>Datensatz</returns>
		public static Instances CreateWekaData(this ObjectLayer objectLayer,IEnumerable<string> classNames,string classAttributeName="class"){
			var instances=Util.CreateInstances(classNames,objectLayer.FindContainedFeatures(),objectLayer.Name,classAttributeName);
			instances.AddImageObjects(objectLayer);
			return instances;
		}
		/// <summary>
		/// erzeugt einen WEKA-kompatiblen Datensatz aus einem <see cref="ObjectLayer"/>
		/// </summary>
		/// <param name="objectLayer"><see cref="ObjectLayer"/></param>
		/// <param name="classAttributeName">Name für das Klassenattribut</param>
		/// <returns>Datensatz</returns>
		public static Instances CreateWekaData(this ObjectLayer objectLayer,string classAttributeName="class"){
			return CreateWekaData(objectLayer,objectLayer.FindContainedClasses().Where(c=>null!=c).Select(c=>c.Name).Distinct().ToList(),classAttributeName);
		}
	}
}
