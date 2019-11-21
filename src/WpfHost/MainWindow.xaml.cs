using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CatchOctocat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Game Game { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Game = new Game(boardSize: 32, numOfCats: 8, lowerBoundBlockingRate: 0.25, upperBoundBlockingRate: 0.33, RefreshBoardView);
            InitializeBoardView();
        }

        private void InitializeBoardView()
        {
            int gap = 10;
            int nodeWidth = (int)((canvas.RenderSize.Width - gap * Game.BoardSize) / Game.BoardSize);
            int nodeHeight = nodeWidth ;
            for (int i = 0; i < Game.BoardSize; i++)
            {
                for (int j = 0; j < Game.BoardSize; j++)
                {
                    string id = string.Format("node_{0}_{1}", i, j);
                    var node = new Ellipse();
                    var brush = GetNodeBrush(i, j);
                    node.Name = id;
                    node.Fill = brush;
                    node.Stroke = brush;
                    node.Height = nodeHeight;
                    node.Width = nodeWidth;
                    node.MouseDown += node_Tapped;
                    int leftPad = (int)Margin.Left + ((i & 1) == 0 ? 0 : (nodeWidth + gap) / 2);
                    node.Margin = new Thickness(leftPad + j * (nodeWidth + gap), i * nodeHeight, 0, 0);
                    canvas.Children.Add(node);
                    RegisterName(node.Name, node);
                }
            }
            int n = 0;
            foreach (Octocat cat in Game.Cats)
            {
                var catNode = FindName(string.Format("node_{0}_{1}", cat.RowIndex, cat.ColumnIndex)) as Ellipse;
                var catview = new Rectangle();
                catview.Name = "cat" + n++.ToString();
                var brushCat = GetCatBrush(cat);
                catview.Stroke = brushCat;
                catview.Fill = brushCat;
                catview.Margin = new Thickness(catNode.Margin.Left + nodeWidth / 4, catNode.Margin.Top + nodeHeight / 2 - nodeHeight, 0, 0);
                catview.Height = nodeHeight * 1.2;
                catview.Width = nodeWidth / 2;
                canvas.Children.Add(catview);
                RegisterName(catview.Name, catview);
            }
            ApplyTemplate();
        }

        private Brush GetNodeTapedBrush()
        {
            return new SolidColorBrush(Colors.Black);
        }

        private Brush GetNodeBrush(int i, int j)
        {
            return Game.Board[i, j] ?
                        GetNodeTapedBrush() :
                        Game.Cats.Count(c => c.EscapePath.Contains(new Tuple<int, int>(i, j))) != 0 ? new SolidColorBrush(Colors.LemonChiffon) : new SolidColorBrush(Colors.Gray);
        }

        private Brush GetCatBrush(Octocat cat)
        {
            return new SolidColorBrush(cat.IsEnclosed ? Colors.Purple : Colors.Orange);
        }

        private void RefreshBoardView()
        {
            int nodeHeight = (int)(canvas.RenderSize.Height / Game.BoardSize);
            int nodeWidth = nodeHeight;
            for (int i = 0; i < Game.BoardSize; i++)
            {
                for (int j = 0; j < Game.BoardSize; j++)
                {
                    string id = string.Format("node_{0}_{1}", i, j);
                    var node = FindName(id) as Ellipse;
                    var brush = GetNodeBrush(i, j);
                    node.Fill = brush;
                    node.Stroke = brush;
                }
            }
            int n = 0;
            foreach (Octocat cat in Game.Cats)
            {
                var catNode = FindName(string.Format("node_{0}_{1}", cat.RowIndex, cat.ColumnIndex)) as Ellipse;
                var catview = FindName("cat" + n++.ToString()) as Rectangle;
                catview.Fill = GetCatBrush(cat);
                catview.Margin = new Thickness(catNode.Margin.Left + nodeWidth / 4, catNode.Margin.Top + nodeHeight / 2 - nodeHeight, 0, 0);
            }
        }

        private bool gameEnded = false;

        private void node_Tapped(object sender, RoutedEventArgs e)
        {
            var node = sender as Ellipse;
            if (node == null)
                return;
            var pos = node.Name.Split("_".ToCharArray());
            int r = int.Parse(pos[1]);
            int c = int.Parse(pos[2]);
            if (Game.Board[r, c])
            {
                return;
            }
            var brush = GetNodeTapedBrush();
            node.Fill = brush;
            node.Stroke = brush;
            if (gameEnded)
            {
                Reset();
                return;
            }
            if (!Game.PlayerMove(r, c))
            {
                if (Game.Steps == -1)
                    gameEnded = true;
                else
                    Reset();
            }
        }

        private void Reset()
        {
            gameEnded = false;
            Game.Reset();
            RefreshBoardView();
        }

        private void buttonReset_Tapped(object sender, RoutedEventArgs e)
        {
            Reset();
        }
    }
}
