using System.Collections.Generic;
using System.Numerics;

namespace StarSaga3.Project.Script.Models
{
    public class BoardSequence
    {
        public List<MovedTileInfo> MovedTiles { get; set; }
        public List<AddedTileInfo> AddedTiles { get; set; }
        public List<Vector2> MatchedPosition { get; set; }
        public int ScoreToAdd { get; set; }
    }
}
