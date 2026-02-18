using System;
using System.Numerics;
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
        void AnimateMove(List<(Vector2 from, Vector2 to)> moves, float duration, Action onComplete);
        void AnimateExplosion(List<Vector2> positions, float duration, Action onComplete);
        void AnimateSpawn(List<(Vector2 pos, int type)> spawns, float duration, Action onComplete);
        void SetHighlight(int x, int y, bool active);
        void HighlightHint(List<Vector2> positions);
        void ClearHighlights();
    }

    public interface IGameService
    {
        IReadOnlyList<IReadOnlyList<Tile>> StartGame(int width, int height);
        bool IsValidMovement(int x1, int y1, int x2, int y2);
        List<BoardSequence> SwapTile(int x1, int y1, int x2, int y2);
        List<Vector2> LookForHint();
        void ActivatePowerUp(PowerUp.PowerUp powerUp);
        List<BoardSequence> UsePowerUp(int x, int y);
    }

    public static class Constants
    {
        public delegate void TileClickDelegate(int x, int y);
    }
}
