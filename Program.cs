using MathNet.Numerics.Interpolation;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

using System.Threading;
using System.Threading.Tasks;

namespace TestMySpline
{
	class Program
	{
		public static string NameMethod;
		public class Point
		{
			public double x;
			public double y;

			public Point(double x, double y)
			{
				this.x = x;
				this.y = y;
			}

			public override string ToString()
			{
				return string.Format("{0}, {1}", Math.Round(x, 2), (Math.Round(y, 2)).ToString().Replace(',', '.'));
			}
		}

		private static bool WriteTxtLog(string outputWriter, string pathLogDirectory, string name = "Script")
		{
			if (!Directory.Exists(pathLogDirectory))
				Directory.CreateDirectory(pathLogDirectory);

			if (!File.Exists(pathLogDirectory))
			{
				File.WriteAllText(pathLogDirectory + @"\Log_" + name + @".txt", outputWriter);
			}
			return true;
		}
		private static bool WriteCsvLog(string outputWriter, string pathLogDirectory, string name = "Script")
		{
			if (!Directory.Exists(pathLogDirectory))
				Directory.CreateDirectory(pathLogDirectory);

			if (!File.Exists(pathLogDirectory))
			{
				File.WriteAllText(pathLogDirectory + @"\Log_" + name + @".Csv", outputWriter);
			}
			return true;
		}

		static void Main(string[] args)
		{
			NameMethod = "CapexTec";
			var watch = new Stopwatch();
			watch.Start();

			int interation = 1;
			int MaxInter = 1;
 
			//var result = Parallel.For(1, MaxInter, (i, state) =>
			//{
			string[] output = TesteMathNetInterpolation();
			//Console.WriteLine("i: {0} | state: {1}",i,state);
			WriteTxtLog(output[0], @".\LOG\", NameMethod + "Akima");
			WriteTxtLog(output[1], @".\LOG\", NameMethod + "Linear");

			//});

			watch.Stop();
			WriteTxtLog("Elapsed Time: " + watch.Elapsed, @".\LOG\", "TIMER");
		}

		public static string[] TesteMathNetInterpolation()
		{
			double DataPoints = 1000;

			// Create the data to be fitted
			//var x = new List<double> { 1, 8, 16, 25, 30 }; // 5 pontos

			var x = new List<double> { 1, 2, 3, 4, 16, 30 }; // 5 pontos
			//var x = new List<double> { 1, 2, 3, 4, 5, 10, 15, 20, 25, 30 }; // 10 pontos

			var y = new List<double> { 8.33, 9.25, 9.16, 8.43, 9.5, 9.14};		
			//var y = new List<double> { 8.33, 9.37, 8.47, 9.32, 9.11, 9.04, 8.93, 9.49, 8.29, 8.88			
			//var y = new List<double> { 10.255, 10.064, 9.961, 9.945, 9.930 , 9.37, 9.65, 9.95, 10.40, 10.88};
			//var y = new List<double> { 3, 3, 3, 3, 15, 40, 50, 60, 70, 80 };

			//Lembre-se sempre de modificar as variacoes de X e Y da plotagem

			var xAkima = new List<double>();
			var yAkima = new List<double>();
			var xLinear = new List<double>();
			var yLinear = new List<double>();

			/// Interpolação Linear
			var linearInterpolation = Interpolate.Linear(x.ToArray(), y.ToArray());

			/// Interpolação Polinomial
			//var PolinomialInterpolation = new NevillePolynomialInterpolation(x.ToArray(), y.ToArray());

			/// Interpolação Akima Spline
			var akimaInterpolation = CubicSpline.InterpolateAkima(x.ToArray(), y.ToArray());


			var ep = 0;
			var a = x.Min();
			var b = x.Max();

			#region Akima Interpolation
			for (int i = 0; i <= DataPoints; i++)
			{
				/// nomalizedForm = (b−a) (x−min / max − min) + a
				/// b = valor maximo do intervalo que os numeros devem ficar
				/// a = valor minimo do intervalo que os numeros devem ficar
				/// max = valor maximo do intervalo atual
				/// min = valor minimo do intervalo atual
				double normalized = ((b + ep) - (a - ep)) * (i / DataPoints) + (a - ep);
				var yInterpoled = akimaInterpolation.Interpolate(normalized);
				xAkima.Add(normalized);
				yAkima.Add(yInterpoled);

			}
			#endregion

			#region Linear Interpolation
			for (int i = 0; i <= DataPoints; i++)
			{
				double normalized = ((b + ep) - (a - ep)) * (i / DataPoints) + (a - ep);
				var yInterpoled = linearInterpolation.Interpolate(normalized);
				xLinear.Add(normalized);
				yLinear.Add(yInterpoled);
			}
			#endregion

			var pointsX = new List<double>();
			var pointSY = new List<double>();

			List<Point> logLinear = new List<Point>();
			List<Point> logAkima = new List<Point>();

			#region Normalizar pontos dos periodos
			for (int i = 1; i <= 30; i++)
			{
				var cY = akimaInterpolation.Interpolate(i);

				pointsX.Add(i);
				pointSY.Add(cY);
				logAkima.Add(new Point(i, cY));
			}
			for (int i = 1; i <= 30; i++)
			{
				var cY = linearInterpolation.Interpolate(i);

				pointsX.Add(i);
				pointSY.Add(cY);
				logLinear.Add(new Point(i, cY));
			}
			#endregion

			//---plotar solução-- -
			PlotSolution(NameMethod,
					  x.ToArray(), y.ToArray(),
					  xAkima.ToArray(), yAkima.ToArray(),
					  xLinear.ToArray(), yLinear.ToArray(),
					  @"..\..\" + NameMethod + ".png");

			string[] output = new string[] { "", "" };
			foreach (Point p in logAkima)
				output[0] += p.ToString() + "\n";
			foreach (Point p in logLinear)
				output[1] += p.ToString() + "\n";

			return output;
		}

		#region PlotSolution

		private static void PlotSolution(string title, double[] x, double[] y, double[] xAkima, double[] yAkima,
										 double[] xLinear, double[] yLinear, string path)
		{

			var chart = new Chart();
			chart.Size = new Size(1200, 800);
			chart.Titles.Add(title);
			chart.Legends.Add(new Legend("Legend"));

			ChartArea ca = new ChartArea("DefaultChartArea");
			ca.AxisX.Title = "X";
			ca.AxisY.Title = "Y";
			chart.ChartAreas.Add(ca);

			Series s1 = CreateSeries(chart, "Akima", CreateDataPoints(xAkima, yAkima), Color.Green, MarkerStyle.None);
			Series s2 = CreateSeries(chart, "Linear", CreateDataPoints(xLinear, yLinear), Color.Blue, MarkerStyle.None);
			Series s3 = CreateSeries(chart, "Pontos", CreateDataPoints(x, y), Color.Red, MarkerStyle.Diamond);

			chart.Series.Add(s3);
			chart.Series.Add(s2);
			chart.Series.Add(s1);
			// Modifica cor e opacidade do grid eixo X e eixo Y
			chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.FromArgb(80, Color.Gray);
			chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(200, Color.Gray);


			var maxY = y.Max();
			if (yAkima.Max() < maxY)
				maxY = yAkima.Max();
			if (yLinear.Max() < maxY)
				maxY = yLinear.Max();

			//var minY = y.Min();
			//if (yAkima.Min() < minY)
			//	minY = yAkima.Min();
			//if (yLinear.Min() < minY)
			//	minY = yLinear.Min();

			// Configura os Eixos
			ca.RecalculateAxesScale();
			ca.AxisX.Minimum = Math.Floor(ca.AxisX.Minimum);
			ca.AxisX.Maximum = Math.Ceiling(ca.AxisX.Maximum);
			ca.AxisY.Maximum = Math.Ceiling(maxY + 1);
			ca.AxisY.Minimum = 3; //Math.Floor(minY - 1);
			ca.AxisY.Interval = 0.5;
			ca.AxisX.Interval = 1;

			// Save
			if (File.Exists(path))
			{
				File.Delete(path);
			}

			using (FileStream fs = new FileStream(path, FileMode.CreateNew))
			{
				chart.SaveImage(fs, ChartImageFormat.Png);
			}
		}

		public static void randomizeValues(List<double> x, out List<double> y, double min, double max)
		{
			var numberOfPoint = x.Count();
			Random rand = new Random();
			var yAux = new List<double>();
			for (int i = 1; i < numberOfPoint - 1; i++)
			{
				var yValue = rand.NextDouble() * (max - min) + min;
				yAux.Add(yValue);
			}

			y = yAux;
		}

		private static List<DataPoint> CreateDataPoints(double[] x, double[] y)
		{
			Debug.Assert(x.Length == y.Length);
			List<DataPoint> points = new List<DataPoint>();

			for (int i = 0; i < x.Length; i++)
			{
				points.Add(new DataPoint(x[i], y[i]));
			}

			return points;
		}

		private static Series CreateSeries(Chart chart, string seriesName, IEnumerable<DataPoint> points, Color color, MarkerStyle markerStyle = MarkerStyle.None)
		{
			var s = new Series()
				{
					XValueType = ChartValueType.Double,
					YValueType = ChartValueType.Double,
					Legend = chart.Legends[0].Name,
					IsVisibleInLegend = true,
					ChartType = SeriesChartType.Line,
					Name = seriesName,
					ChartArea = chart.ChartAreas[0].Name,
					MarkerStyle = markerStyle,
					Color = color,
					MarkerSize = 8
				};

			foreach (var p in points)
			{
				s.Points.Add(p);
			}

			return s;
		}

		#endregion
	}
}
