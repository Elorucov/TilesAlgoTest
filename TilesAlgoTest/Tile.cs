using Avalonia;
using System.Collections.Generic;

namespace TilesAlgoTest {
    //                       1x1,   2x2,    4x2,  3x3
    internal enum TileSize { Small, Medium, Wide, Large }

    internal class Tile {
        public int Id { get; private set; }
        public int Row {  get; set; }
        public int Column { get; set; }
        public TileSize Size { get; set; }
        public string Name { get; set; }
        public Rect Rect { get { return ToRect(); } }

        internal Tile(int id, int row, int col, TileSize size, string name) {
            Id = id;
            Row = row;
            Column = col;
            Size = size;
            Name = name;
        }

        private Rect ToRect() {
            int w = 1;
            int h = 1;

            switch (Size) {
                case TileSize.Medium: w = 2; h = 2; break;
                case TileSize.Wide: w = 4; h = 2; break;
                case TileSize.Large: w = 3; h = 3; break;
            }

            return new Rect(Column, Row, w, h);
        }

        internal static List<Tile> GetSampleTiles() {
            return new List<Tile> { 
                new Tile(1, 0, 0, TileSize.Small, "A"),
                new Tile(2, 0, 1, TileSize.Small, "B"),
                new Tile(3, 0, 2, TileSize.Small, "C"),
                new Tile(4, 0, 3, TileSize.Small, "D"),
                new Tile(5, 0, 4, TileSize.Small, "E"),
                new Tile(6, 0, 5, TileSize.Small, "F"),

                new Tile(7, 1, 0, TileSize.Wide, "G"),
                new Tile(8, 1, 4, TileSize.Small, "H"),
                new Tile(9, 1, 5, TileSize.Small, "I"),
                new Tile(10, 2, 4, TileSize.Small, "J"),
                new Tile(11, 2, 5, TileSize.Small, "K"),

                new Tile(12, 3, 0, TileSize.Small, "L"),
                new Tile(13, 3, 1, TileSize.Small, "M"),
                new Tile(14, 3, 2, TileSize.Medium, "N"),
                new Tile(15, 3, 4, TileSize.Medium, "O"),
                new Tile(16, 4, 0, TileSize.Small, "P"),
                new Tile(17, 4, 1, TileSize.Small, "Q"),

                new Tile(18, 5, 0, TileSize.Medium, "R"),
                new Tile(19, 5, 2, TileSize.Large, "S"),
                new Tile(20, 7, 0, TileSize.Small, "T"),
                new Tile(21, 7, 1, TileSize.Small, "U"),
            };
        }
    }
}
