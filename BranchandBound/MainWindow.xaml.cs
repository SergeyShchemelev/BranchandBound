using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BranchandBound.src;

using System.Diagnostics;
using System.Threading;

namespace BranchandBound
{
    
    public partial class MainWindow : Window
    {
        Grid graphs;
        Cities cities;
        Stack<TextBox> errorTB;
        CancellationTokenSource[] cts;
        Queue<Task> tasks;
        public MainWindow()
        {
            InitializeComponent();
            graphs = new Grid();
            graphs = graph5;
            errorTB = new Stack<TextBox>();
            tasks = new Queue<Task>();
            cts = new CancellationTokenSource[1];
            cts[0] = new CancellationTokenSource();

        }
        private void CancelCalculation()
        {
            foreach (var c in cts)
            {
                c.Cancel();
            }
            tasks.Clear();
        }
        public void DrawPoints(object sender, RoutedEventArgs e)
        {
            CancelCalculation();
            ClearTextBox();
            int nCities;
            if (!Int32.TryParse(textB_countCities.Text, out nCities))
            {
                textB_countCities.Background = Brushes.Coral;
                errorTB.Push(textB_countCities);
                return;
            }
            cities = new Cities(nCities);
            cities.Generate((int)graph5.Width);
            GeometryGroup cityGroup = new GeometryGroup();
            for (int i = 0; i < cities.NumCities; i++)
            {
                Location l = cities.GetLocation(i);
                // формирование точек на карте
                EllipseGeometry city = new EllipseGeometry();
                city.Center = new Point(l.X, l.Y);
                city.RadiusX = 4;
                city.RadiusY = 4;
                cityGroup.Children.Add(city);
            }
            Path myPath = new Path();
            myPath.Fill = Brushes.Plum;
            myPath.Stroke = Brushes.Black;
            myPath.Data = cityGroup;
            graphs.Children.Clear();
            graphs.Children.Add(myPath);

            button_CalcBB.IsEnabled = true;
   

        } // DrawPoints

        public void DrawLines(int[] trail, Grid graph)
        {
            GeometryGroup linesGroup = new GeometryGroup();
            LineGeometry line = new LineGeometry();
            int city = 1;
            for (; city < trail.Length; city++)
            {
                line = new LineGeometry();
                line.StartPoint = new Point(cities.GetLocation(trail[city - 1]).X, cities.GetLocation(trail[city - 1]).Y);
                line.EndPoint = new Point(cities.GetLocation(trail[city]).X, cities.GetLocation(trail[city]).Y);
                linesGroup.Children.Add(line);
            }
            line = new LineGeometry();
            line.StartPoint = new Point(cities.GetLocation(trail[city - 1]).X, cities.GetLocation(trail[city - 1]).Y);
            line.EndPoint = new Point(cities.GetLocation(trail[0]).X, cities.GetLocation(trail[0]).Y);
            linesGroup.Children.Add(line);

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.Data = linesGroup;
            if (graph.Children.Count > 1)
                graph.Children.Remove(graph.Children[1]);
            graph.Children.Add(myPath);
        }
        public void DrawLines(Location[] trail, Grid graph)
        {
            GeometryGroup linesGroup = new GeometryGroup();
            LineGeometry line = new LineGeometry();
            int city = 1;
            for (; city < trail.Length; city++)
            {
                line = new LineGeometry();
                line.StartPoint = new Point(trail[city - 1].X, trail[city - 1].Y);
                line.EndPoint = new Point(trail[city].X, trail[city].Y);
                linesGroup.Children.Add(line);
            }
            line = new LineGeometry();
            line.StartPoint = new Point(trail[city - 1].X, trail[city - 1].Y);
            line.EndPoint = new Point(trail[0].X, trail[0].Y);
            linesGroup.Children.Add(line);

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.Data = linesGroup;
            if (graph.Children.Count > 1)
                graph.Children.Remove(graph.Children[1]);
            graph.Children.Add(myPath);
        }

        public void ClearTextBox()
        {
            while (errorTB.Count > 0)
            {
                errorTB.Pop().Background = Brushes.White;
            }
        }


        public async Task CalculateBB()
        {
            ClearTextBox();
            cts[0].Cancel();
            cts[0] = new CancellationTokenSource();
            int maxTime;
            if (!Int32.TryParse(textB_timeBB.Text, out maxTime))
            {
                textB_timeBB.Background = Brushes.Coral;
                errorTB.Push(textB_timeBB);
                return;
            }
            BB algorithm = null;
            Stopwatch time = null;
            Location[] solve = null;
            Task thisTask = (Task.Run(() =>
            {
                algorithm = new BB(maxTime);
                time = new Stopwatch();
                time.Start();
                solve = algorithm.Solution(cities);
                time.Stop();
            }));
            tasks.Enqueue(thisTask);
            await Task.Run(() =>
            {
                while (true)
                {

                    if (cts[0].Token.IsCancellationRequested || thisTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
            if (solve != null)
            {

                DrawLines(solve, graphs);
                timeBB.Content = time.ElapsedMilliseconds;
                lengthBB.Content = Math.Round(algorithm.TotalDistance, 2);
            }
        }


        private void button_CalcBB_Click(object sender, RoutedEventArgs e)
        {
            CalculateBB();
        }

     
    }
}
