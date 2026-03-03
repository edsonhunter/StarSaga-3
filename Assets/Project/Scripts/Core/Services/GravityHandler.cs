using System;
using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public interface IGravityHandler
    {
        List<MovedTileInfo> MoveTiles(Tile[,] board, List<Vector2Int> matchedPositions);
        List<AddedTileInfo> FillEmptyTiles(Tile[,] board, IReadOnlyList<int> tileTypes, ref int tileCount);
    }

    public class GravityHandler : IGravityHandler
    {
        public List<MovedTileInfo> MoveTiles(Tile[,] board, List<Vector2Int> matchedPositions)
        {
            var moved = new List<MovedTileInfo>();
            int height = board.GetLength(0);
            int width  = board.GetLength(1);

            // Build a fast lookup of matched rows by column
            var matchedByColumn = new Dictionary<int, HashSet<int>>();
            foreach (var p in matchedPositions)
            {
                int px = p.x;
                int py = p.y;
                if (!matchedByColumn.TryGetValue(px, out var rows))
                {
                    rows = new HashSet<int>();
                    matchedByColumn[px] = rows;
                }
                rows.Add(py);
            }

            // Only process columns that actually had matched tiles
            foreach (var kv in matchedByColumn)
            {
                int x = kv.Key;
                var matchedRows = kv.Value;

                int writeY = height - 1; // lowest slot we can write into

                // Compact this column: skip matched (empties), pack non-empty tiles down
                for (int y = height - 1; y >= 0; y--)
                {
                    // Treat matched cells as empty; never try to move from them
                    if (matchedRows.Contains(y)) continue;

                    var tile = board[y, x];
                    if (tile.Type == -1) continue; // already empty, nothing to move

                    if (y != writeY)
                    {
                        // Move tile down to writeY
                        board[writeY, x] = tile;
                        board[y, x] = new Tile { Id = -1, Type = -1, Score = 0 };

                        moved.Add(new MovedTileInfo
                        {
                            From = new Vector2Int(x, y),
                            To   = new Vector2Int(x, writeY)
                        });
                    }

                    writeY--;
                }

                // Everything above the last written tile becomes empty
                for (int y = writeY; y >= 0; y--)
                {
                    board[y, x] = new Tile { Id = -1, Type = -1, Score = 0 };
                }
            }

            return moved;
        }

        public List<AddedTileInfo> FillEmptyTiles(Tile[,] board, IReadOnlyList<int> tileTypes, ref int tileCount)
        {
            List<AddedTileInfo> addedTiles = new();
            int height = board.GetLength(0);
            int width  = board.GetLength(1);
            var random = new Random();

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = width - 1; x >= 0; x--)
                {
                    if (board[y, x].Type == -1)
                    {
                        int randomTypeIndex = random.Next(0, tileTypes.Count);

                        Tile tile = board[y, x];
                        tile.Id = tileCount++;
                        tile.Type = tileTypes[randomTypeIndex];
                        tile.Score = tile.Type * 10;
                        board[y, x] = tile; // struct reassignment

                        addedTiles.Add(new AddedTileInfo
                        {
                            Position = new Vector2Int(x, y),
                            Type = tile.Type
                        });
                    }
                }
            }

            return addedTiles;
        }
    }
}
