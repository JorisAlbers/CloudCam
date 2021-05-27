using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light
{
	public static class LedEffects
	{
		public static int[] sinColorTable = new int[128];

		static LedEffects()
		{
			FillColorTable();
		}

		private static void FillColorTable()
		{
			var refValues = new byte[sinColorTable.Length];
			var phase = 120d;
			//The on time for a color is 180°, the full cycle is phase * 3
			var onTimeRatio = refValues.Length * 180d / (phase * 3);
			var radian = Math.PI / onTimeRatio;

			//compute the values to do a simple lookup
			for (int i = 0; i < refValues.Length; i++)
			{
				//Console.WriteLine(Math.Sin(radian * i) * 255);
				refValues[i] = (byte)(i < (int)onTimeRatio ? Math.Sin(radian * i) * 255 : 0);
			}
			//now asign each color with an offset of 120°
			for (int i = 0; i < sinColorTable.Length; i++)
			{
				var greenOffset = (int)(i + (onTimeRatio * phase * 2 / 180d)) % sinColorTable.Length;
				var blueOffset = (int)(i + (onTimeRatio * phase / 180d)) % sinColorTable.Length;
				sinColorTable[i] =
					(refValues[i] << 8) |               //red 
					(refValues[greenOffset] << 16) |    //green
					refValues[blueOffset];              //blue
			}
		}
	}
}
