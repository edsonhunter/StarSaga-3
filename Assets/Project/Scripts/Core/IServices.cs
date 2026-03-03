using System;
using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core
{
    public interface IBoardView
    {
        event Constants.TileClickDelegate TileClicked;
        void Initialize(int width, int height);
        void SetTile(int x, int y, int type);
        void ClearTile(int x, int y);
        void AnimateSwap(int x1, int y1, int x2, int y2, float duration, Action onComplete);
        void AnimateMove(List<(Vector2Int from, Vector2Int to)> moves, float duration, Action onComplete);
        void AnimateExplosion(List<Vector2Int> positions, float duration, Action onComplete);
        void AnimateSpawn(List<(Vector2Int pos, int type)> spawns, float duration, Action onComplete);
        void SetHighlight(int x, int y, bool active);
        void HighlightHint(List<Vector2Int> positions);
        void ClearHighlights();
    }

    public interface IGameService
    {
        Tile[,] StartGame(int width, int height);
        bool IsValidMovement(int x1, int y1, int x2, int y2);
        List<BoardSequence> SwapTile(int x1, int y1, int x2, int y2);
        List<Vector2Int> LookForHint();
        void ActivatePowerUp(PowerUp.PowerUp powerUp);
        List<BoardSequence> UsePowerUp(int x, int y);
    }

    public static class Constants
    {
        public delegate void TileClickDelegate(int x, int y);
    }
}
