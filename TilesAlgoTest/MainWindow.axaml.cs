using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace TilesAlgoTest {
    public partial class MainWindow : Window {
        int column = 6;
        double tilesLayoutWidth;
        List<Tile> SampleTiles;

        public MainWindow() {
            InitializeComponent();

            tilesLayoutWidth = TilesLayout.Width;
            SampleTiles = Tile.GetSampleTiles();
            Test();
        }

        private void CheckTiles() {
            CheckIsOutOfBounds();
            CheckVerticalIntersect();
            FixEmptyRows();
        }

        private void CheckIsOutOfBounds() {
            foreach (Tile tile in SampleTiles) {
                Rect trect = tile.Rect;
                if (trect.Right > column) {
                    tile.Column -= Convert.ToInt32(trect.Right - column);
                    tile.Row += 1;
                }
            }
        }

        private void CheckVerticalIntersect() {
            int extra = 0;
            int i = 0;
            foreach (Tile tile in SampleTiles) {
                if (i == 0) {
                    i += 1;
                    continue;
                }
                if (extra > 0) tile.Row += extra;

                Rect trect = tile.Rect;
                List<Rect> rects = SampleTiles.Take(i).Select(t => t.Rect).ToList();
                foreach (Rect rect in CollectionsMarshal.AsSpan(rects)) {
                    bool intersects = trect.Intersects(rect);
                    if (intersects) { // If tile (trect) have a collision, move it under (rect)
                        int add = Convert.ToInt32(rect.Height + rect.Top - trect.Top);
                        tile.Row += add;
                        extra += add;
                    }
                }

                i += 1;
            }
        }

        // Maybe there is a more optimized version?
        private void FixEmptyRows() {
            int rows = Convert.ToInt32(SampleTiles.Max(t => t.Rect.Bottom));
            //int emptyRows = 0;
            //for (int row = 0; row < rows; row++) {
            //    bool rowOccupied = false;
            //    foreach (Tile tile in SampleTiles) {
            //        if (tile.Rect.Top <= row && tile.Rect.Bottom <= row) { 
            //            rowOccupied = true;
            //            break;
            //        }
            //    }
            //    if (!rowOccupied) emptyRows++;
            //}

            List<int> nonEmptyRows = new List<int>();
            List<int> emptyRows = new List<int>();

            foreach (Tile tile in SampleTiles) {
                var rect = tile.Rect;
                for (int row = Convert.ToInt32(rect.Top); row <= Convert.ToInt32(rect.Bottom - 1); row++) {
                    if (!nonEmptyRows.Contains(row)) nonEmptyRows.Add(row);
                }
            }

            for (int i = 0; i < rows; i++) {
                if (!nonEmptyRows.Contains(i)) emptyRows.Add(i);
            }

            foreach (Tile tile in SampleTiles) {
                var move = emptyRows.Where(r => r < tile.Row).Count();
                if (move > 0) tile.Row = tile.Row - move;
            }
        }

        private void Test() {
            CheckTiles();

            double smallTileSize = tilesLayoutWidth / column;

            foreach (var tile in SampleTiles) {
                Rect rect = tile.Rect;
                double width = smallTileSize * rect.Width;
                double height = smallTileSize * rect.Height;
                double top = smallTileSize * tile.Row;
                double left = smallTileSize * tile.Column;

                Control c = TilesLayout.Children.Where(c => (c.DataContext as Tile)?.Id == tile.Id).FirstOrDefault();
                if (c != null) {
                    c.Width = width;
                    c.Height = height;
                    Canvas.SetLeft(c, left);
                    Canvas.SetTop(c, top);
                } else {
                    Border b = new Border {
                        DataContext = tile,
                        Padding = new Thickness(4),
                        Width = width,
                        Height = height,
                        Child = new Border {
                            Background = new SolidColorBrush(Color.FromArgb(224, 0, 122, 204)),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Child = new TextBlock {
                                VerticalAlignment = VerticalAlignment.Center,
                                TextAlignment = TextAlignment.Center,
                                Text = $"{tile.Id}: {tile.Name}",
                                Foreground = new SolidColorBrush(Colors.White)
                            }
                        }
                    };
                    Canvas.SetLeft(b, left);
                    Canvas.SetTop(b, top);

                    b.ContextRequested += TileContextRequested;
                    b.PointerPressed += TilePointerPressed;
                    b.PointerReleased += TilePointerReleased;

                    TilesLayout.Children.Add(b);
                }
            }
        }

        Point dragStartPos;
        Point tileStartPos;

        private void TilePointerPressed(object sender, PointerPressedEventArgs e) {
            Border b = sender as Border;
            dragStartPos = e.GetCurrentPoint(TilesLayout).Position;
            tileStartPos = new Point(Canvas.GetLeft(b), Canvas.GetTop(b));

            b.PointerMoved += TilePointerMoved;
        }

        private void TilePointerReleased(object sender, PointerReleasedEventArgs e) {
            Border b = sender as Border;
            b.PointerMoved -= TilePointerMoved;

            // Convert UI position to tile position
            double smallTileSize = tilesLayoutWidth / column;

            double ux = Canvas.GetLeft(b);
            double uy = Canvas.GetTop(b);

            double tx = ux / smallTileSize;
            double ty = uy / smallTileSize;

            int x = Convert.ToInt32(Math.Round(tx));
            int y = Convert.ToInt32(Math.Round(ty));

            Tile associatedTile = b.DataContext as Tile;
            Tile tile = SampleTiles.Where(t => t.Id == associatedTile.Id).FirstOrDefault();
            if (tile != null) {
                tile.Column = x;
                tile.Row = y;

                Tile interscectTile = SampleTiles.Where(t => t.Column == x && t.Row == y).FirstOrDefault();
                if (interscectTile != null) { 
                    int index = SampleTiles.IndexOf(interscectTile);
                    SampleTiles.Remove(tile);
                    SampleTiles.Insert(index, tile);
                }
            }
            Test();
        }

        private void TilePointerMoved(object sender, PointerEventArgs e) {
            Border b = sender as Border;
            double smallTileSize = tilesLayoutWidth / column;

            var currentPos = e.GetCurrentPoint(TilesLayout).Position;
            var moved = currentPos - dragStartPos;
            double x = tileStartPos.X + moved.X;
            double y = tileStartPos.Y + moved.Y;
            if (x >= 0 && x <= tilesLayoutWidth - smallTileSize) Canvas.SetLeft(b, x);
            if (y >= 0) Canvas.SetTop(b, y);
        }

        private void TileContextRequested(object sender, ContextRequestedEventArgs e) {
            Border b = sender as Border;
            Tile t = b.DataContext as Tile;

            MenuItem smaler = new MenuItem { Header = "Smaller" };
            MenuItem larger = new MenuItem { Header = "Larger" };
            MenuItem remove = new MenuItem { Header = "Remove" };

            smaler.Click += (a, c) => Resize(t, false);
            larger.Click += (a, c) => Resize(t, true);
            remove.Click += (a, c) => Remove(t, b);

            MenuFlyout mf = new MenuFlyout();
            mf.Items.Add(smaler);
            mf.Items.Add(larger);
            mf.Items.Add(remove);

            mf.ShowAt(b, true);
        }

        private void Resize(Tile tile, bool enlarge) {
            if (tile.Size == TileSize.Small) {
                if (enlarge) tile.Size = TileSize.Medium;
            } else if (tile.Size == TileSize.Medium) {
                tile.Size = enlarge ? TileSize.Wide : TileSize.Small;
            }
            else if (tile.Size == TileSize.Wide) {
                tile.Size = enlarge ? TileSize.Large : TileSize.Medium;
            }
            else if (tile.Size == TileSize.Large) {
                if (!enlarge) tile.Size = TileSize.Wide;
            }

            Test();
        }

        private void Remove(Tile tile, Border border) {
            SampleTiles.Remove(tile);
            TilesLayout.Children.Remove(border);
            Test();
        }

        private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
            SampleTiles = Tile.GetSampleTiles();
            Test();
        }
    }
}